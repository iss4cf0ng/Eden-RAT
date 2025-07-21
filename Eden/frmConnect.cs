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
    public partial class frmConnect : Form
    {
        public Client m_clnt;

        public frmConnect()
        {
            InitializeComponent();
        }

        private void fnLogin()
        {
            Client clnt = new Client();
            clnt.SecureServerSocketEstablished += ServerSocketEstablished;
            clnt.LoginSuccessful += LoginSuccessed;
            clnt.LoginFailed += LoginFailed;
            clnt.StatusMessage += StatusMessage;

            new Thread(() => clnt.Connect(textBox1.Text, (int)numericUpDown1.Value)).Start();
        }

        private void ServerSocketEstablished(Client clnt)
        {
            clnt.Login(textBox2.Text, textBox3.Text);
        }
        private void LoginSuccessed(Client clnt, string szUsername, int nAuthority)
        {
            MessageBox.Show("Login successfully.", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
            m_clnt = clnt;

            Invoke(new Action(Close));
        }
        private void LoginFailed(Client clnt, string szUsername, string szMsg)
        {
            MessageBox.Show(szMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        private void StatusMessage(int nCode, string szMsg)
        {
            Invoke(new Action(() => toolStripStatusLabel1.Text = szMsg));
        }

        void setup()
        {
            toolStripStatusLabel1.Text = "Login Panel";
        }

        private void frmConnect_Load(object sender, EventArgs e)
        {
            setup();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            fnLogin();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            textBox3.PasswordChar = checkBox1.Checked ? '\0' : '*';
        }

        private void frmConnect_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void textBox3_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    fnLogin();
                    break;
            }
        }
    }
}
