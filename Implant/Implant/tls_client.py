import socket
import ssl
import threading
import base64
import time

SERVER_IP = '127.0.0.1'
SERVER_PORT = 5000

g_bConnected = False

g_szTag = 'python3'

context = ssl.create_default_context()
context.check_hostname = False
context.verify_mode = ssl.CERT_NONE

class Client:
    def __init__(self, sock: socket.socket) -> None:
        self.sock = sock
        self.addr = None

        self.dic_class = dict()

    

def combine_bytes(abFirst: bytes, nIdxFirst: int, nLenFirst: int, abSecond: bytes, nIdxSecond: int, nLenSecond: int) -> bytes:
    return abFirst[nIdxFirst:nIdxFirst + nLenFirst] + abSecond[nIdxSecond:nIdxSecond + nLenSecond]

def handler(clnt_sock: ssl.SSLSocket):
    abStaticRecv = b''
    abDynamicRecv = b''

    

def main():
    while True:
        if g_bConnected:
            time.sleep(1)
            continue

        with socket.create_connection((SERVER_IP, SERVER_PORT)) as sock:
            with context.wrap_socket(sock, server_hostname=SERVER_IP) as ssock:
                threading.Thread(target=handler, args=[ssock, ]).start()

if __name__ == '__main__':
    main()