'''
Obfuscator: Binary
'''

class Obfuscator:
    def __init__(self, code: str):
        self.code = code

    def run(self) -> str:
        result = ''.join(f'\\x{byte:02x}' for byte in self.code.encode('utf-8'))
        result = f'exec(b\'{result}\'.decode(\'utf-8\'))'

        return result

if __name__ == '__main__':
    obf = Obfuscator('xx', False)
    print(obf.run())