import random
import string

from dataclasses import dataclass

class C2P:
    HEADER_LENGTH = 6
    BUFFER_MAX_LENGTH = 65536

    def __init__(
        self,
        nCmd: int = None,
        nParam: int = None,
        abData: bytes = None,
        abBuffer: bytes = None
    ) -> None:

        self.abMoreData = b''
        self.abData = b''
        self.abBuffer = b''
        self.nCmd = 0
        self.nParam = 0
        self.nLength = 0

        # ===== Constructor-1 (from buffer) =====
        if abBuffer is not None:
            if len(abBuffer) < C2P.HEADER_LENGTH:
                return

            self.abBuffer = abBuffer

            # HEADER
            self.nCmd = abBuffer[0]
            self.nParam = abBuffer[1]
            self.nLength = int.from_bytes(
                abBuffer[2:C2P.HEADER_LENGTH],
                byteorder='big'
            )

            # DATA
            data_start = C2P.HEADER_LENGTH
            data_end = data_start + self.nLength

            if len(abBuffer) - C2P.HEADER_LENGTH >= self.nLength:
                self.abData = abBuffer[data_start:data_end]

            # MORE DATA
            if len(abBuffer) > data_end:
                self.abMoreData = abBuffer[data_end:]

        # ===== Constructor-2 (cmd, param, msg) =====
        else:
            if abData is None:
                abData = b''

            self.nCmd = nCmd
            self.nParam = nParam
            self.abData = abData
            self.nLength = len(abData)

            self.abBuffer = (
                nCmd.to_bytes(1, 'big') +
                nParam.to_bytes(1, 'big') +
                self.nLength.to_bytes(4, 'big') +
                abData
            )

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