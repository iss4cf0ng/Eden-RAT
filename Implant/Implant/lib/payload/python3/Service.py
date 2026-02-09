import sys

sys.path.append('../../../')

from lib.tool import EZData, Encoder

#[THE CODE ABOVE WILL NOT BE INCLUDED IN PAYLOAD]

import os
import subprocess
from dataclasses import dataclass

class Service:
    def __init__(self):
        pass

    @dataclass
    class ServiceInfo:
        name: str
        load: str
        active: str
        sub: str
        description: str

    def get_services(self) -> list:
        result = subprocess.run(
            ['systemctl', 'list-units', '--type=service', '--all', '--no-pager', '--plain'],
            stdout=subprocess.PIPE,
            stderr=subprocess.PIPE,
            text=True
        )

        if result.returncode != 0:
            return []
        
        services = result.stdout.strip().splitlines()
        
        lsService = []
        for service in services[1:]:
            columns = service.split()
            if len(columns) < 5 or '=' in columns:
                continue

            name = columns[0]
            load = columns[1]
            active = columns[2]
            sub = columns[3]
            description = ' '.join(columns[4:])

            lsService.append(Service.ServiceInfo(name, load, active, sub, description))

        return lsService

    def run(self, clnt: object, szToken: str, lsMsg: list) -> list:
        if lsMsg[0] == 'ls':
            ls = []
            for service in self.get_services():
                assert isinstance(service, Service.ServiceInfo)
                ls.append([service.name, service.load, service.active, service.sub, service.description])

            return [
                'serv',
                'ls',
                EZData.twoDlist2str(ls),
            ]
        elif lsMsg[0] == 'kill':
            pass

if __name__ == '__main__':
    serv = Service()
    print(serv.get_services())