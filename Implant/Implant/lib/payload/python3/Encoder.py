import base64

class Encoder:
    @staticmethod
    def bytes2b64str(buffer: bytes) -> str:
        return base64.b64encode(buffer).decode('utf-8')

    @staticmethod
    def b64str2bytes(text: str) -> bytes:
        return base64.b64decode(text)
    
    @staticmethod
    def b64d2str(text: str) -> str:
        return base64.b64decode(text.encode('utf-8')).decode('utf-8')
    
    @staticmethod
    def stre2b64(text: str) -> str:
        return base64.b64encode(text.encode('utf-8')).decode('utf-8')