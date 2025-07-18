using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eden
{
    internal class clsTransferFileHandler
    {
        public string m_szFilePath { get; set; }
        private int m_nChunkSize { get; set; }
        private FileStream m_fstream { get; set; }

        public clsTransferFileHandler(string szFilePath, int nChunkSize = 1024 * 5)
        {
            m_szFilePath = szFilePath;
            m_nChunkSize = nChunkSize;
            m_fstream = new FileStream(szFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        public (int nRead, byte[] abBuffer) fnabGetChunk()
        {
            byte[] abBuffer = new byte[m_nChunkSize];
            int nRead = 0;
            nRead = m_fstream.Read(abBuffer, 0, abBuffer.Length);

            return (nRead, abBuffer);
        }

        public void fnClose()
        {
            m_fstream.Close();
        }
    }
}
