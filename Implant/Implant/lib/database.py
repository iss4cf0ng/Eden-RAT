'''
The database manager for Eden-RAT
'''

import os
import sqlite3
import hashlib
import getpass
import logging
from datetime import datetime

class DB:
    log = logging.getLogger(__name__ + '.Listener')
    log.setLevel(logging.NOTSET)

    def __init__(self, db_file: str = None):
        # database archtecture
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
    Database Functions
    '''

    '''
    Execute SQL query.
    '''
    def sql_execute(self, sql_query: str) -> list:
        self.obj_cursor.execute(sql_query)
        ls_output = self.obj_cursor.fetchall()

        self.sql_conn.commit()

        return ls_output

    '''
    Create database.
    '''
    def create_db(self) -> bool:
        try:
            if not os.path.exists(self.db_file):
                self.log.debug('SQLite file not found, try to create a new database file: ' + self.db_file)

            self.sql_conn = sqlite3.connect(self.db_file)
            self.obj_cursor = self.sql_conn.cursor()

            for key in self.dic_table.keys():
                self.create_table(key, self.dic_table[key])

            return True
        except Exception as ex:
            self.log.error(str(ex))
            return False

    '''
    Create table.
    '''
    def create_table(self, name: str, ls_columns: list) -> bool:
        try:
            sql_query = f'CREATE TABLE IF NOT EXISTS {name} ({",".join(ls_columns)})'
            self.sql_execute(sql_query)

            return True
        except Exception as ex:
            self.log.error(str(ex))
            return False

    '''
    Insert table.
    '''
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
            self.log.error(str(ex))
            return False

    '''
    Update row.
    '''
    def update_row(self, table_name: str, dic_row: dict) -> bool:
        try:
            first_key = next(iter(dic_row))
            payload = ','.join([f'{key} = \'{dic_row[key]}\'' for key in dic_row.keys()])
            sql_query = f'UPDATE {table_name} SET {payload} WHERE {first_key} = \'{dic_row[first_key]}\''
            
            return self.sql_execute(sql_query)
        except Exception as ex:
            self.log.error(str(ex))
            return False

    '''
    Close database.
    '''
    def close_db(self) -> bool:
        try:
            self.sql_conn.close()
            return True
        except Exception as ex:
            self.log.error(str(ex))
            return False

    '''
    C2 SERVER FUNCTION    
    '''

    '''
    Add new listener.
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
            self.log.error(ex)
            return False

    '''
    Delete listener.
    '''
    def del_listener(self, szName: str) -> bool:
        if not self.exists_listener(szName):
            return False
        
        sql_query = f'DELETE FROM Listener WHERE Name = \'{szName}\''
        self.sql_execute(sql_query)

        return not self.exists_listener(szName)

    '''
    Check listener's existence.
    '''
    def exists_listener(self, name):
        sql_query = f'SELECT 1 FROM Listener WHERE Name = \'{name}\' LIMIT 1'
        ls_result = self.sql_execute(sql_query)

        return len(ls_result) > 0 and ls_result[0][0] == 1

    '''
    Get all listeners.
    '''
    def get_listener(self) -> list:
        ls_output = self.sql_execute('SELECT * FROM Listener')
        ls_result = list()

        for row in ls_output:
            dic_row = dict()
            for i in range(0, len(self.dic_table['Listener'])):
                dic_row[self.dic_table['Listener'][i]] = row[i]
            
            ls_result.append(dic_row)

        return ls_result

    '''
    Add user into database.
    '''
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
            self.log.error(str(ex))
            return False

    '''
    Delete user.
    '''
    def del_user(self, username) -> bool:
        try:
            sql_query = f'DELETE FROM User WHERE Username = "{username}"'
            self.sql_execute(sql_query)
            return True
        except Exception as ex:
            self.log.error(str(ex))
            return False

    '''
    Check user's existence.
    '''
    def exists_user(self, username: str) -> bool:
        sql_query = f'SELECT 1 FROM User WHERE Username = \'{username}\' LIMIT 1'
        ls_result = self.sql_execute(sql_query)

        return len(ls_result) > 0 and ls_result[0][0] == 1
    
    def get_user(self) -> list:
        pass

    '''
    Update user log.
    '''
    def user_login(self, szUsername: str) -> bool:
        dic_user = {
            'Username': szUsername,
            'LastLogin': datetime.now()
        }

        return self.update_row('User', dic_user)

    '''
    Add victim (preserved)
    '''
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
            self.log.error(str(ex))
            return False

    def del_victim(self):
        pass

    def exists_victim(self, online_id: str) -> bool:
        sql_query = f'SELECT 1 FROM Victim WHERE OnlineID = \'{online_id}\' LIMIT 1'
        self.obj_cursor.execute(sql_query)

        exists = self.obj_cursor.fetchall() is not None

        return exists
    
class Interactive:
    def __init__(self, db: DB):
        self.log = logging.getLogger(__name__ + '.Listener')
        self.log.setLevel(logging.NOTSET)

        self.db = db
        self.dic_func = [
            self.add_user,
            self.del_user,
            self.sql_shell,
        ]

    '''
    Interactive console.
    '''
    def do_interactive(self):
        for i in range(0, len(self.dic_func)):
            print(f'{i}: {self.dic_func[i].__name__}')
        
        while True:
            try:
                szCmd = input('(Enter \'exit\' or \'q\' to exit)> ').lower()
                aCmd = szCmd.split(' ')

                if aCmd[0] == 'ls':
                    pass
                elif aCmd[0] == 'help':
                    pass
                elif aCmd[0] == 'exit' or aCmd[0] == 'q':
                    return
                elif aCmd[0] == 'use':
                    pass
                elif aCmd[0].isdigit():
                    self.dic_func[int(aCmd[0])]()
            
            except KeyboardInterrupt:
                self.log.debug('User exit.')
            except Exception as ex:
                self.log.error(str(ex))

    '''
    Add user.
    '''
    def add_user(self):
        while True:
            username = input('Username> ')
            if self.db.exists_user(username):
                self.log.error('Username exists.')
                continue

            password = getpass.getpass('Password> ')
            ans = input('Sure?(y/N)> ').lower()
            if ans != 'y':
                continue

            if self.db.add_user(username, password):
                print('Add user successfully: ' + username)
            else:
                print('Add user failed.')

            if input('Enter \'q\' for quit. Otherwise for next> ').lower() == 'q':
                self.log.debug('Quit')
                return

    '''
    Delete user.
    '''
    def del_user(self):
        while True:
            try:
                username = input('Username (Enter q to quit)> ')
                if username == 'q' or username == 'exit':
                    self.log.debug("User exit.")
                    break
                
                ans = input('Sure?(y/N)> ').lower()
                if ans == 'y':
                    if self.db.del_user(username):
                        self.log.info('Delete user successfully: ' + username)
                        break
                else:
                    break

            except KeyboardInterrupt:
                self.log.debug("User exit.")
            except Exception as ex:
                self.log.error(str(ex))

    '''
    Execute SQL query.
    '''
    def sql_shell(self):
        while True:
            try:
                sql_query = input('> ')

                if sql_query == 'exit' or sql_query == 'q':
                    self.log.debug("User exit.")
                    return
                else:
                    print(self.db.sql_execute(sql_query))
            except KeyboardInterrupt:
                self.log.debug("User exit.")
            except Exception as ex:
                self.log.error(str(ex))
        
def main():
    db = DB()

    while True:
        sql_query = input('> ')
        print(db.sql_execute(sql_query))

        print(db.exists_victim('aaa'))

if __name__ == '__main__':
    main()