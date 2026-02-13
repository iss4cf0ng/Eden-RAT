

#[THE CODE ABOVE WILL NOT BE INCLUDED IN PAYLOAD]

import os

class Connection:
    def __init__(self):
        pass

    def run(self, clnt: object, szToken: str, lsMsg: list) -> list:
        if lsMsg[0] == 'disconnect':
            os._exit(0)

        elif lsMsg[0] == 'reconnect':
            clnt.sock.close()

        return []