import hashlib

from lib.database import DB

def authorization(user: str, password: str) -> bool:
    db = DB()
    ls_result = db.sql_execute('select Username, Password from User')
    
    if len(ls_result) == 0:
        return False
    
    result = ls_result[0]
    user = result[0]
    passwd = result[1]

    return password == passwd