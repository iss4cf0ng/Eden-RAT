'''
Project: Eden-RAT
Author: ISSAC
Github: https://github.com/iss4cf0ng/Eden-RAT

Terminology:
    Listener: socket listener.
    Template: A listener for handling victim.
    Client: A class for C2 users.
    Victim: A class for compromised machines.
    Payload: Class object for executing.
'''

import argparse
import sys
import threading
import os
import logging

from lib.database import DB, Interactive as db_interactive
from lib import Listener

from lib.Logger import setup_logging

banner = '''
      _        ,..
 ,--._\\_.--, (-00)
; #         _:(  -)
:          (_____/
:            :
 `.___..___.`

-   Can we prove anyone has ever reached paradise?
-   If paradise exists, then the ones who reach it will remain there for eternity,
    forever enveloped in joy and satisfaction.
-   If someone does leave paradise, it's because they weren't fully satisfied,
    so it was never a paradise to begin with.
-   Therefore, those who reach paradise cannot be found——and should not exist——outside of paradise.

Project: Eden-RAT
Version: 1.0.0
Author: ISSAC
Github: https://github.com/Eden-RAT/
'''

print(banner)

parser = argparse.ArgumentParser(
    prog='eden_server.py',
    description='A remote access tool designed for the early stage of penetration testing.',
    epilog='Example: eden_server.py -lvvp 4444',
)

general_group = parser.add_argument_group('General Options')
general_group.add_argument('-v', '--verbose', action='count', help='Increase output verbosity. Use -v, -vv, or -vvv for more detail.', default=0)

network_group = parser.add_argument_group('Network Options')
network_group.add_argument('-l', '--listen', action='store_true', help='Start the application in server mode and listen for incoming connections.')
network_group.add_argument('-p', '--port', help='Port number to bind the server to (1-65535).')

db_group = parser.add_argument_group('Database Options')
db_group.add_argument('--db', action='store_true', help='Database console.')

args = parser.parse_args()

setup_logging(args.verbose)

sql_db = None

def main():
    if os.name == 'nt': # Windows OS
        os.system('color')

    sql_db = DB()
    if not sql_db:
        logging.error('Database initialization failed.')
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