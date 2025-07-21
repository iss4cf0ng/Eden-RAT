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
        public int m_nIndex { get; set; }
        public long m_nFileSize { get; set; }

        private int m_nChunkSize { get; set; }
        private FileStream m_fstream { get; set; }

        public long fnGetChunkCount() => (int)Math.Ceiling((decimal)(m_nFileSize / m_nChunkSize));
        public bool fnbIsDone() => m_nIndex + 1 == fnGetChunkCount();

        public clsTransferFileHandler(string szFilePath, int nChunkSize = 1024 * 5)
        {
            m_szFilePath = szFilePath;
            m_nIndex = 0;
            m_nChunkSize = nChunkSize;
            m_nFileSize = new FileInfo(szFilePath).Length;
            m_fstream = new FileStream(szFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        /// <summary>
        /// Read file data by chunk.
        /// </summary>
        /// <returns>nRead: Count of read bytes, abBuffer: File buffer.</returns>
        public (int nRead, int nIndex, byte[] abBuffer) fnabGetChunk()
        {
            byte[] abBuffer = new byte[m_nChunkSize];
            int nRead = 0;
            nRead = m_fstream.Read(abBuffer, 0, abBuffer.Length);
            byte[] abChunkBuffer = new byte[nRead];
            Array.Copy(abBuffer, 0, abChunkBuffer, 0, nRead);

            m_nIndex++;

            return (nRead, m_nIndex, abChunkBuffer);
        }

        public bool fnbWriteChunk(int nIndex, byte[] abBuffer)
        {
            try
            {
                int nOffset = nIndex * m_nChunkSize;
                m_fstream.Seek(nOffset, SeekOrigin.Begin);
                m_fstream.Write(abBuffer);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "fnbWriteChunk()", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public void fnClose()
        {
            m_fstream.Close();
        }
    }
}
