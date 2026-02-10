import socket
import ssl
import threading
import base64
import time
import random
import string

SERVER_IP = '127.0.0.1'
SERVER_PORT = 5000

g_bConnected = False

g_szTag = 'python3'

context = ssl.create_default_context()
context.check_hostname = False
context.verify_mode = ssl.CERT_NONE

class C2P:
    HEADER_LENGTH = 6
    BUFFER_MAX_LENGTH = 65565

    def __init__(self, nCmd: int = None, nParam: int = None, abData: bytes = None, abBuffer: bytes = None) -> None:

        self.abMoreData = b''
        self.abData = b''
        self.abBuffer = b''
        self.nCmd = 0
        self.nParam = 0
        self.nLength = 0

        if abBuffer is not None:
            if len(abBuffer) < C2P.HEADER_LENGTH:
                return

            self.abBuffer = abBuffer

            self.nCmd = abBuffer[0]
            self.nParam = abBuffer[1]
            self.nLength = int.from_bytes(abBuffer[2:C2P.HEADER_LENGTH], byteorder='big')

            data_start = C2P.HEADER_LENGTH
            data_end = data_start + self.nLength

            if len(abBuffer) - C2P.HEADER_LENGTH >= self.nLength:
                self.abData = abBuffer[data_start:data_end]

            if len(abBuffer) > data_end:
                self.abMoreData = abBuffer[data_end:]

        else:
            if abData is None:
                abData = b''

            self.nCmd = nCmd
            self.nParam = nParam
            self.abData = abData
            self.nLength = len(abData)

            self.abBuffer = nCmd.to_bytes(1, 'big') + nParam.to_bytes(1, 'big') + self.nLength.to_bytes(4, 'big') + abData


    def get_header(self) -> tuple[int, int, int]:
        self.nCmd = self.abBuffer[0]
        self.nParam = self.abBuffer[1]
        self.nLength = int.from_bytes(self.abBuffer[2:C2P.HEADER_LENGTH], byteorder='big')

        return (self.nCmd, self.nParam, self.nLength)

    def get_msg(self) -> bytes:
        return self.abData
    
    def get_buffer(self) -> bytes:
        return self.abBuffer
    
    def more_data(self) -> bytes:
        return self.abMoreData

    def combine_bytes(self, abFirst: bytes, nIdxFirst: int, nLenFirst: int, abSecond: bytes, nIdxSecond: int, nLenSecond: int) -> bytes:
        return abFirst[nIdxFirst:nIdxFirst + nLenFirst] + abSecond[nIdxSecond:nIdxSecond + nLenSecond]
    
    @staticmethod
    def random_str(n_str_len = 10):
        return ''.join(random.SystemRandom().choice(string.ascii_uppercase + string.digits) for _ in range(n_str_len))

class Client:
    def __init__(self, sock: socket.socket) -> None:
        self.sock = sock
        self.addr = None

        self.xml_rsa_pubKey = None
        self.xml_rsa_privKey = None

        self.szChallenge = None

        self.dic_class = dict()

    def send(self, nCmd: int, nParam: int, szMsg: str):
        buffer = C2P(nCmd=nCmd, nParam=nParam, abData=szMsg.encode('ascii')).get_buffer()
        self.sock.send(buffer)

    def sendcommand(self, nCmd: int, nParam: int):
        self.send(nCmd=nCmd, nParam=nParam, szMsg=C2P.random_str())

    def sendcipher(self, nCmd: int, nParam: int, szMsg: str):
        pAES = self.get_aes()
        abMsg = szMsg.encode()
        abEncMsg = pAES.encrypt_cbc(abMsg)
        szEncMsg = base64.b64encode(abEncMsg).decode('ascii')

        self.send(nCmd=nCmd, nParam=nParam, szMsg=szEncMsg)

    def sendserver(self, aMsg: list):
        
        #self.sendcipher(2, 3, '|'.join([Encoder.stre2b64(x) for x in aMsg]))
        pass

    def close(self):
        self.sock.close()

def combine_bytes(abFirst: bytes, nIdxFirst: int, nLenFirst: int, abSecond: bytes, nIdxSecond: int, nLenSecond: int) -> bytes:
    return abFirst[nIdxFirst:nIdxFirst + nLenFirst] + abSecond[nIdxSecond:nIdxSecond + nLenSecond]

def handler(clnt_sock: socket.socket):
    abStaticRecv = b''
    abDynamicRecv = b''

    clnt = Client(clnt_sock)

    clnt.sendcommand(0, 1)

    while True:
        try:
            abStaticRecv = clnt_sock.recv(C2P.BUFFER_MAX_LENGTH)
            nRecvLength = len(abStaticRecv)

            abDynamicRecv = combine_bytes(abDynamicRecv, 0, len(abDynamicRecv), abStaticRecv, 0, nRecvLength)

            if not nRecvLength:
                break
            elif len(abDynamicRecv) < C2P.HEADER_LENGTH:
                continue
            else:
                c2p = C2P(abBuffer=abDynamicRecv)
                tpHeader = c2p.get_header()
                nLength = tpHeader[2]

                while len(abDynamicRecv) - C2P.HEADER_LENGTH >= nLength:
                    try:
                        c2p = C2P(abBuffer=abDynamicRecv)
                        abDynamicRecv = c2p.more_data()
                        tpHeader = c2p.get_header()

                        nCmd = tpHeader[0]
                        nParam = tpHeader[1]
                        nLength = tpHeader[2]
                        abMsg = c2p.get_msg()

                        print(abMsg)
                    except Exception as ex:
                        pass
        except Exception as ex:
            pass

def main():
    while True:
        if g_bConnected:
            time.sleep(1)
            continue

        with socket.create_connection((SERVER_IP, SERVER_PORT)) as sock:
            with context.wrap_socket(sock, server_hostname=SERVER_IP) as ssock:
                threading.Thread(target=handler, args=[ssock, ]).start()

        time.sleep(1)

if __name__ == '__main__':
    main()