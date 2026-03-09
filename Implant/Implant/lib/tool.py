from dataclasses import dataclass, field

from lib.EZCrypto import Encoder


class EZData:
    @staticmethod
    def list2str(ls: list, spliter: str = ',') -> str:
        return spliter.join([Encoder.stre2b64(x) for x in ls])

    @staticmethod
    def lsDic2Str(ls: list, spliter_inner = ',', spliter_outer = ';') -> str:
        ls_result = list()
        for dic in ls:
            assert dic is dict

            ls_tmp = [Encoder.stre2b64(dic[key]) for key in dic.keys()]
            ls_result.append(spliter_inner.join(ls_tmp))

        return spliter_outer.join(ls_result)
    
    @staticmethod
    def twoDlist2str(ls: list, spliter_inner = ',', spliter_outer = ';') -> str:
        ls_result = list()
        for _ls in ls:
            ls_result.append(spliter_inner.join([Encoder.stre2b64(x) for x in _ls]))
        
        return spliter_outer.join(ls_result)
    
    @staticmethod
    def oneSpliter2list(szData: str, spliter = ',') -> list:
        return [Encoder.b64d2str(x) for x in szData.split(spliter)]
    
    @staticmethod
    def twoSpliter2list(szData: str, spliter_inner = ',', spliter_outer = ';') -> list:
        ls_result = list()
        for x in szData.split(spliter_outer):
            ls_result.append([Encoder.b64d2str(s) for s in x.split(spliter_inner)])
        
        return ls_result
    
class EZClass:
    @dataclass
    class Listener:
        szName: str
        szTemplate: str
        szIP: str
        nPort: int
        objListener: object

        nLattency: int = None

    @dataclass
    class Victim:
        szTag: str
        szOnlineID: str
        objClient: object
        
        szIP: str = None
        szUsername: str = None
        nLattency: int = None
        lsLoadedPayload: list = field(default_factory=list)