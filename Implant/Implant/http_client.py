import socket
import time
import base64

HTTP_SERVER_IP = '127.0.0.1'
HTTP_SERVER_PORT = 8080

g_bConnected = False

class Encoder:
    @staticmethod
    def bytes2b64str(buffer: bytes) -> str:
        return base64.b64encode(buffer).decode('utf-8')

    @staticmethod
    def b64str2bytes(text: str) -> bytes:
        return base64.b64decode(text)
    
    @staticmethod
    def b64d2str(text: str) -> str:
        return base64.b64decode(text.encode('utf-8'))
    
    @staticmethod
    def stre2b64(text: str) -> str:
        return base64.b64encode(text.encode('utf-8')).decode('utf-8')


def get_body_post(szData: str, szResource = '/') -> str:
    return get_body_post(szData, 'POST', szResource)

def get_body_get(szData: str, szResource = '/') -> str:
    return get_resp_body(szData, 'GET', szResource)

def get_resp_body(szData: str, szMethod = 'POST', szResource = '/') -> str:
    szData = Encoder.stre2b64(szData)
    nDataLength = len(szData)

    request = (
        f'{szMethod} {szResource} HTTP/1.1\r\n'
        'Host: google.com\r\n'
        'Content-Type: text/plain\r\n'
        f'Content-Length: {nDataLength}\r\n'
        '\r\n'
    ) + szData

    return request

def send_http_req(serv_sock: socket.socket, szData):
    serv_sock.sendall(szData.encode('utf-8'))

def handler(serv_sock: socket.socket):

    req = get_body_get('hello', '/0')
    send_http_req(serv_sock, req)

    while serv_sock.fileno() != -1:
        resp = serv_sock.recv(1024).decode('utf-8')
        headers, body = resp.split('\r\n\r\n', 1)

        aHeader = headers.split('\r\n')
        nContentLength = 0

        for szHeaderLine in aHeader:
            if szHeaderLine.lower().startswith('content-length:'):
                nContentLength = int(szHeaderLine.split(':')[1].strip())
                break

        while len(body) < nContentLength:
            body += serv_sock.recv(nContentLength - len(body)).decode('utf-8')

        if not body:
            continue
        
        # Handler data

        print(body)

def main():
    global g_bConnected

    sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    while True:
        if not g_bConnected:
            try:
                sock.connect((HTTP_SERVER_IP, HTTP_SERVER_PORT))

                g_bConnected = True

                handler(sock)
            except Exception as ex:
                raise ex
        
        time.sleep(1)
        

if __name__ == '__main__':
    main()