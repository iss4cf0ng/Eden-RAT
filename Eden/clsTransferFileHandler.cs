using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eden
{
    public partial class clsTransferFileHandler : IDisposable
    {
        public long m_nFileSize = -1;

        public string m_szSrcFilePath { get; init; }
        public string m_szDstFilePath { get; init; }
        public enMethod m_enMethod { get; init; }
        public int m_nChunkSize { get; init; }
        public string szRemoteFile { get { return m_enMethod == enMethod.Upload ? m_szDstFilePath : m_szSrcFilePath; } }

        public bool m_bFinished = false;

        private FileStream m_fileStream { get; init; }

        public long m_nWritten = 0;
        public long m_nRead = 0;

        public clsTransferFileHandler(string szSrcFilePath, string szDstFilePath, enMethod method, int nChunkSize)
        {
            m_szSrcFilePath = szSrcFilePath;
            m_szDstFilePath = szDstFilePath;
            m_nChunkSize = nChunkSize;
            m_enMethod = method;

            if (method == enMethod.Upload)
            {
                m_nFileSize = new FileInfo(szSrcFilePath).Length;
                m_fileStream = File.Open(szSrcFilePath, FileMode.Open);
            }
            else
            {
                m_fileStream = File.Open(szDstFilePath, FileMode.Append);
            }
        }

        public enum enMethod
        {
            Upload,
            Download,
        }

        public void Dispose()
        {
            try
            {
                m_fileStream.Close();
            }
            catch
            {

            }
            
            try
            {
                m_fileStream.Dispose();
            }
            catch
            {

            }
        }

        public int fnWriteChunk(byte[] abFileChunk)
        {
            if (abFileChunk.Length == 0 || !m_fileStream.CanWrite)
                return 0;

            if (m_enMethod == enMethod.Upload)
                throw new Exception("This method is used for downloading file.");

            m_fileStream.Write(abFileChunk);

            m_nWritten += abFileChunk.Length;

            return abFileChunk.Length;
        }

        public byte[] fnReadNextChunk()
        {
            if (m_enMethod == enMethod.Download)
                throw new Exception("This method is used for uploading file.");

            if (!m_fileStream.CanRead || m_nChunkSize == 0)
                return new byte[] { };

            byte[] abBuffer = new byte[m_nChunkSize];
            long nRead = m_fileStream.Read(abBuffer, 0, m_nChunkSize);

            byte[] abChunk = new byte[nRead];
            Array.Copy(abBuffer, 0, abChunk, 0, nRead);

            m_nRead += nRead;

            m_bFinished = nRead == 0;

            return abChunk;
        }
    }
}
