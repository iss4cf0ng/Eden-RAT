import sqlite3
import hashlib
import getpass
from datetime import datetime

from lib.ColorPrint import ColorPrint as cp

class DB:
    def __init__(self, db_file: str = None):
        self.dic_table = {
            'Listener' : [
                'Name',
                'Template',
                'IP',
                'Port',
                'Creator',
                'CreateDate',
                'LastModified',
            ],
            'User' : [
                'Username',
                'Password',
                'Type',
                'CreateDate',
                'LastLogin',
                'LastModified',
            ],
            'Victim' : [
                'OnlineID',
                'Listener',
                'Host',
                'Username',
                'PluginDir',
                'CreateDate',
                'LastOnline',
                'LastModified',
                'LastAccessed',
            ],
        }

        if not db_file:
            self.db_file = 'db.sqlite3'
            if not self.create_db():
                raise Exception('Cannot create database.')
        else:
            self.db_file = db_file
            self.sql_conn = sqlite3.connect(self.db_file)
            self.obj_cursor = self.sql_conn.cursor()

    '''
    Database Function
    '''

    def sql_execute(self, sql_query: str) -> list:
        self.obj_cursor.execute(sql_query)
        ls_output = self.obj_cursor.fetchall()

        self.sql_conn.commit()

        return ls_output

    def create_db(self) -> bool:
        try:
            self.sql_conn = sqlite3.connect(self.db_file)
            self.obj_cursor = self.sql_conn.cursor()

            for key in self.dic_table.keys():
                self.create_table(key, self.dic_table[key])

            return True
        except Exception as ex:
            print(ex)
            return False

    def create_table(self, name: str, ls_columns: list) -> bool:
        try:
            sql_query = f'CREATE TABLE IF NOT EXISTS {name} ({",".join(ls_columns)})'
            self.sql_execute(sql_query)

            return True
        except Exception as ex:
            print(ex)
            return False

    def insert_row(self, table_name: str, dic_row: dict) -> bool:
        try:
            #sql_query = f'INSERT INTO {table_name} ({",".join(dic_row.keys())}) VALUES ({",".join([f"'{dic_row[x]}'" for x in dic_row.keys()])})'

            cols_str = ','.join(dic_row.keys())
            vals_str = ','.join([f'\'{dic_row[x]}\'' for x in dic_row.keys()])

            sql_query = f'INSERT INTO {table_name} ({cols_str}) VALUES ({vals_str})'

            print(sql_query)
            print(self.sql_execute(sql_query))

            return True
        except Exception as ex:
            print(ex)
            return False

    def update_row(self, table_name: str, dic_row: dict) -> bool:
        try:
            first_key = next(iter(dic_row))
            payload = ','.join([f'{key} = \'{dic_row[key]}\'' for key in dic_row.keys()])
            sql_query = f'UPDATE {table_name} SET {payload} WHERE {first_key} = \'{dic_row[first_key]}\''
            
            return self.sql_execute(sql_query)
        except Exception as ex:
            print(ex)
            return False

    def close_db(self) -> bool:
        try:
            self.sql_conn.close()
            return True
        except Exception as ex:
            print(ex)
            return False

    '''
    C2 SERVER FUNCTION    
    '''

    def add_listener(self, szName: str, szTemplate: str, szIP: str, nPort: int, szCreator: str = None) -> bool:
        try:
            now = datetime.now()
            dic_listener = {
                'Name': szName,
                'Template': szTemplate,
                'IP': szIP,
                'Port': str(nPort),
                'Creator': szCreator,
                'CreateDate': now,
                'LastModified': now
            }

            if self.exists_listener(szName):
                dic_listener.pop('CreateDate')
                return self.update_row('Listener', dic_listener)
            else:
                return self.insert_row('Listener', dic_listener)
        except Exception as ex:
            cp.pf_err(ex)
            return False

    def del_listener(self, szName: str) -> bool:
        if not self.exists_listener(szName):
            return False
        
        sql_query = f'DELETE FROM Listener WHERE Name = \'{szName}\''
        self.sql_execute(sql_query)

        return not self.exists_listener(szName)

    def exists_listener(self, name):
        sql_query = f'SELECT 1 FROM Listener WHERE Name = \'{name}\' LIMIT 1'
        ls_result = self.sql_execute(sql_query)

        return len(ls_result) > 0 and ls_result[0][0] == 1

    def get_listener(self) -> list:
        ls_output = self.sql_execute('SELECT * FROM Listener')
        ls_result = list()

        for row in ls_output:
            dic_row = dict()
            for i in range(0, len(self.dic_table['Listener'])):
                dic_row[self.dic_table['Listener'][i]] = row[i]
            
            ls_result.append(dic_row)

        return ls_result

    def add_user(self, username: str, password: str) -> bool:
        try:
            dt_now = datetime.now()
            dic_user = {
                'Username': username,
                'Password': hashlib.sha512(password.encode('utf-8')).hexdigest().upper(),
                'Type': 'user',
                'CreateDate': dt_now,
                'LastModified': dt_now
            }

            if self.exists_user(username):
                dic_user.pop('CreateDate')
                self.update_row('User', dic_user)
            else:
                return self.insert_row('User', dic_user)
        except Exception as ex:
            print(ex)
            return False

    def del_user(self, username) -> bool:
        try:
            sql_query = f'DELETE FROM User WHERE Username = "{username}"'
            self.sql_execute(sql_query)
            return True
        except Exception as ex:
            print(ex)
            return False

    def exists_user(self, username: str) -> bool:
        sql_query = f'SELECT 1 FROM User WHERE Username = \'{username}\' LIMIT 1'
        ls_result = self.sql_execute(sql_query)

        return len(ls_result) > 0 and ls_result[0][0] == 1
    
    def get_user(self) -> list:
        pass

    def user_login(self, szUsername: str) -> bool:
        dic_user = {
            'Username': szUsername,
            'LastLogin': datetime.now()
        }

        return self.update_row('User', dic_user)

    def add_victim(self, dic_victim_info: dict) -> bool:
        try:
            if 'OnlineID' in dic_victim_info.keys():
                if not self.exists_victim(dic_victim_info['OnlineID']):
                    pass
                else:
                    pass
            else:
                pass

            return True
        except Exception as ex:
            print(ex)
            return False

    def del_victim(self):
        pass

    def exists_victim(self, online_id: str) -> bool:
        sql_query = f'SELECT 1 FROM Victim WHERE OnlineID = \'{online_id}\' LIMIT 1'
        ls_result = self.sql_execute(sql_query)

        return len(ls_result) > 0 and ls_result[0][0] == 1
    
class Interactive:
    def __init__(self, db: DB):
        self.db = db
        self.dic_func = [
            self.add_user,
            self.del_user,
            self.sql_shell,
        ]

    def do_interactive(self):
        for i in range(0, len(self.dic_func)):
            print(f'{i}: {self.dic_func[i].__name__}')
        
        while True:
            szCmd = input('> ')
            aCmd = szCmd.split(' ')

            if aCmd[0] == 'ls':
                pass
            elif aCmd[0] == 'help':
                pass
            elif aCmd[0] == 'exit':
                return
            elif aCmd[0] == 'use':
                pass
            elif aCmd[0].isdigit():
                self.dic_func[int(aCmd[0])]()

    def add_user(self):
        while True:
            username = input('Username> ')
            if self.db.exists_user(username):
                cp.pf_err('Username exists.')
                continue

            password = getpass.getpass('Password> ')
            if input('Press [ENTER] to add> ') != '':
                cp.pf_err('add_user() cancell.')
            else:
                if self.db.add_user(username, password):
                    cp.pf_ok('add_user() ok')
                else:
                    cp.pf_err('add_user() error.')

            if input('Enter \'q\' for quit. Otherwise for next> ').lower() == 'q':
                cp.pf_info('Quit')
                return

    def del_user(self):
        pass

    def sql_shell(self):
        while True:
            sql_query = input('> ')

            if sql_query == 'exit' or sql_query == 'q':
                return
            else:
                print(self.db.sql_execute(sql_query))
        
def main():
    db = DB()

    while True:
        sql_query = input('> ')
        print(db.sql_execute(sql_query))

        print(db.exists_victim('aaa'))

if __name__ == '__main__':
    main()