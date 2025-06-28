import random
import string

from dataclasses import dataclass

class C2P:
    HEADER_LENGTH = 6
    BUFFER_MAX_LENGTH = 65565

    def __init__(self, nCmd: int = None, nParam: int = None, abData: bytes = None, abBuffer: bytes = None) -> None:
        self.abMoreData = b''
        if abBuffer:
            self.abBuffer = abBuffer
            self.get_header()

            if len(abBuffer) - C2P.HEADER_LENGTH >= self.nLength:
                self.abData = self.abBuffer[C2P.HEADER_LENGTH:C2P.BUFFER_MAX_LENGTH]
            if len(abBuffer) - C2P.HEADER_LENGTH - self.nLength > 0:
                self.abMoreData = abBuffer[C2P.BUFFER_MAX_LENGTH:C2P.BUFFER_MAX_LENGTH+len(abBuffer) - C2P.HEADER_LENGTH - self.nLength]
        else:
            self.nCmd = nCmd
            self.abCmd = nCmd.to_bytes(1, 'big')
            self.nParam = nParam
            self.abParam = nParam.to_bytes(1, 'big')
            self.abData = abData
            self.nLength = len(abData)
            self.abLength = self.nLength.to_bytes(C2P.HEADER_LENGTH - 2, 'big')

            self.abBuffer = self.abCmd + self.abParam + self.abLength + self.abData

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