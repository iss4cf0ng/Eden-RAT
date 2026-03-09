import os
import platform

class bcolors:
    HEADER = '\033[95m'
    OKBLUE = '\033[94m'
    OKCYAN = '\033[96m'
    OKGREEN = '\033[92m'
    WARNING = '\033[93m'
    FAIL = '\033[91m'
    ENDC = '\033[0m'
    BOLD = '\033[1m'
    UNDERLINE = '\033[4m'

class ColorPrint:
    def __init__(self):
        if platform.platform == 'nt':
            os.system('color')

    @staticmethod
    def pf_info(msg, id = None):
        id = f'{id}: ' if id else ''
        print(f'{bcolors.OKBLUE}[*]{bcolors.ENDC} {id}{msg}')

    @staticmethod
    def pf_ok(msg, id = None):
        id = f'{id}: ' if id else ''
        print(f'{bcolors.OKGREEN}[+]{bcolors.ENDC} {id}{msg}')

    @staticmethod
    def pf_warn(msg, id = None):
        id = f'{id}: ' if id else ''
        print(f'{bcolors.WARNING}[!]{bcolors.ENDC} {id}{msg}')

    @staticmethod
    def pf_err(msg, id = None):
        id = f'{id}: ' if id else ''
        print(f'{bcolors.FAIL}[-]{bcolors.ENDC} {id}{msg}')