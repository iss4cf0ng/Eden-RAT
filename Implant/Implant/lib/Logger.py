import logging
import datetime
import sys

class Formatter(logging.Formatter):
    RESET = '\033[0m'
    GOLD = '\033[38;5;178m'
    GREEN = '\033[32m'
    BLUE = '\033[94m'
    RED = '\033[31m'
    YELLOW = '\033[33m'
    GRAY = '\033[90m'

    SYMBOLS = {
    logging.DEBUG:    ('[*]', BLUE),
    logging.INFO:     ('[+]', GREEN),
    logging.WARNING:  ('[!]', YELLOW),
    logging.ERROR:    ('[-]', RED),
    logging.CRITICAL: ('[!]', RED),
}

    def __init__(self, use_color=True):
        super().__init__()
        self.use_color = use_color and sys.stdout.isatty()

    def format(self, record):
        symbol, sym_color = self.SYMBOLS.get(
            record.levelno, ('[?]', self.BLUE)
        )

        ts = datetime.datetime.fromtimestamp(
            record.created
        ).strftime('%Y-%m-%d %H:%M:%S')

        msg = record.getMessage()

        if self.use_color:
            symbol = f'{sym_color}{symbol}{self.RESET}'
            ts = f'{self.GOLD}{ts}{self.RESET}'

        return f'{symbol} [{ts}] {msg}'

def setup_logging(verbose: int):
    level = logging.WARNING
    if verbose == 1:
        level = logging.INFO
    elif verbose >= 2:
        level = logging.DEBUG

    print("VERBOSE =", verbose)

    root = logging.getLogger()
    root.setLevel(level)

    handler = logging.StreamHandler()
    handler.setLevel(logging.NOTSET)
    handler.setFormatter(Formatter())

    root.handlers.clear()
    root.addHandler(handler)