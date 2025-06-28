import os

SPLITER = '#[THE CODE ABOVE WILL NOT BE INCLUDED IN PAYLOAD]\n'

def get_payload(szTag: str, szName: str) -> str:
    with open(f'./lib/payload/{szTag}/{szName}.py', encoding='utf-8') as f:
        szPayload = f.read()
        if SPLITER in szPayload:
            szPayload = '\n'.join(szPayload.split(SPLITER)[1:])

        return szPayload