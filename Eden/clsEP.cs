using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Eden
{
    public class clsEP
    {
        //HEADER
        public const int HEADER_SIZE = 6; //6 BYTES
        public byte _Command = 0;
        public byte Command { get { return _Command; } }
        public byte _Param = 0;
        public byte Param { get { return _Param; } }
        private int _DataLength = 0;
        public int DataLength { get { return _DataLength; } }

        //DATA
        private byte[] _MessageData = new byte[0];
        private byte[] MessageData = new byte[0];
        private byte[] _MoreData = new byte[0];
        public byte[] MoreData { get { return _MoreData; } }

        //CONSTRUCTOR-1
        public clsEP(byte[] buffer)
        {
            if (buffer == null || buffer.Length < HEADER_SIZE)
                return;

            //BUFFER INFORMATION
            using (MemoryStream ms = new MemoryStream(buffer))
            {
                using (BinaryReader br = new BinaryReader(ms))
                {
                    try
                    {
                        //HEADER
                        _Command = br.ReadByte(); // 1 BYTE
                        _Param = br.ReadByte(); // 1 BYTE

                        byte[] abLength = br.ReadBytes(4);
                        _DataLength = BitConverter.ToInt32(abLength);
                        if (BitConverter.IsLittleEndian)
                        {
                            Array.Reverse(abLength);
                            _DataLength = BitConverter.ToInt32(abLength);
                        }

                        if (buffer.Length - HEADER_SIZE >= DataLength)
                            _MessageData = br.ReadBytes(_DataLength);
                        if (buffer.Length - HEADER_SIZE - DataLength > 0)
                            _MoreData = br.ReadBytes(buffer.Length - HEADER_SIZE - _DataLength);
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
        }
        //CONSTRUCTOR-2
        public clsEP(byte cmd, byte para, byte[] msg)
        {
            _Command = cmd;
            _Param = para;
            _MessageData = msg;
            _DataLength = msg.Length;
        }

        public byte[] GetBytes()
        {
            try
            {
                byte[] bytes = null;
                byte[] abLength = BitConverter.GetBytes(_DataLength);
                
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(abLength);

                MemoryStream ms = new MemoryStream();
                BinaryWriter bw = new BinaryWriter(ms);
                bw.Write(_Command);
                bw.Write(_Param);
                bw.Write(abLength);
                bw.Write(_MessageData);
                bytes = ms.ToArray();
                bw.Close();
                ms.Close();

                return bytes;
            }
            catch (Exception ex)
            {
                return new byte[0];
            }
        }

        public (byte cmd, byte para, int len, byte[] msg) GetMsg()
        {
            (byte cmd, byte para, int len, byte[] msg) ret = (
                _Command,
                _Param,
                _MessageData.Length,
                _MessageData
                );

            return ret;
        }

        public static (byte cmd, byte para, int len) GetHeader(byte[] buf)
        {
            (byte cmd, byte para, int len) ret = (0, 0, 0);
            if (buf == null || buf.Length < HEADER_SIZE)
                return ret;

            ret.cmd = buf[0];
            ret.para = buf[1];

            byte[] abLength = buf.Skip(2).Take(4).ToArray();
            if (BitConverter.IsLittleEndian)
                Array.Reverse(abLength);

            ret.len = BitConverter.ToInt32(abLength);

            return ret;
        }

        public static byte[] CombineBytes(
            byte[] abFirst, 
            int nFirstIdx, 
            int nFirstLengnth,

            byte[] abSecond,
            int nSecondIdx,
            int nSecondLength
        )
        {
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(abFirst, nFirstIdx, nFirstLengnth);
                ms.Write(abSecond, nSecondIdx, nSecondLength);

                return ms.ToArray();
            }
        }
    }
}
