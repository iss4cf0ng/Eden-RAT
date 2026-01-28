using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eden
{
    internal class clsTransferFileHandler
    {
        public string m_szLocalFilePath { get; set; }
        public string m_szRemoteFilePath { get; set; }

        public int m_nIndex { get; set; }
        public long m_nFileSize { get; set; }

        public int m_nChunkSize { get; set; }

        private TransferFileType m_transferType { get; set; }
        private FileStream m_fstream { get; set; }

        public long fnGetChunkCount()
        {
            long nQ = m_nFileSize / m_nChunkSize;
            long nR = m_nFileSize % m_nChunkSize;

            return nR == 0 ? nQ : nQ + 1;
        }
        public bool fnbIsDone() => m_nIndex + 1 >= fnGetChunkCount();

        public clsTransferFileHandler(TransferFileType transferType, string szLocalFilePath, string szRemoteFilePath, int nChunkSize = 1024 * 20, int nFileSize = 0)
        {
            m_transferType = transferType;

            m_szLocalFilePath = szLocalFilePath;
            m_szRemoteFilePath = szRemoteFilePath;

            m_nIndex = 0;
            m_nChunkSize = nChunkSize;
            m_nFileSize = transferType == TransferFileType.UploadFile ? new FileInfo(szLocalFilePath).Length : nFileSize;

            if (transferType == TransferFileType.UploadFile)
                m_fstream = new FileStream(szLocalFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            else
                m_fstream = new FileStream(szLocalFilePath, FileMode.Create, FileAccess.Write, FileShare.Read);
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
                /*
                using (FileStream f = new FileStream(m_szLocalFilePath, FileMode.Create, FileAccess.Write, FileShare.Write))
                {
                    f.Seek(nOffset, SeekOrigin.Begin);
                    f.Write(abBuffer);
                }
                */

                Task.Run(() =>
                {
                    m_fstream.Seek(nOffset, SeekOrigin.Begin);
                    m_fstream.Write(abBuffer);
                    m_fstream.Flush();
                });

                m_nIndex++;

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
