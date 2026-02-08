import sys

sys.path.append('../../../')

from lib.tool import EZData, Encoder

#[THE CODE ABOVE WILL NOT BE INCLUDED IN PAYLOAD]

import os
from dataclasses import dataclass

class Service:
    def __init__(self):
        pass

    @dataclass
    class ServiceInfo:
        name = ''

    def get_services(self) -> list:
        pass

    def run(self, clnt: object, szToken: str, lsMsg: list) -> list:
        pass

if __name__ == '__main__':
    pass