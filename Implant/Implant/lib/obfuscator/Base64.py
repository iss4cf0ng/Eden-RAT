'''
Obfuscator: Base64
'''

import base64

class Obfuscator:
    def __init__(self, code: str):
        self.code = code

    def run(self) -> str:
        b64 = base64.b64encode(self.code.encode('utf-8')).decode('utf-8')
        result = f'import base64;exec(base64.b64decode(\'{b64}\'.encode(\'utf-8\')).decode(\'utf-8\'))'

        return result