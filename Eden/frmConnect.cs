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
        public clsClient m_clnt { get; set; }

        public frmConnect(clsClient clnt)
        {
            InitializeComponent();

            m_clnt = clnt;
            Text = "Login Panel";
        }

        private async void fnLogin()
        {
            if (string.IsNullOrEmpty(textBox1.Text))
            {
                MessageBox.Show("Server host cannot be null or empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrEmpty(textBox2.Text))
            {
                MessageBox.Show("Username cannot be null or empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrEmpty(textBox3.Text))
            {
                MessageBox.Show("Passowrd cannot be null or empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            toolStripStatusLabel1.Text = "Connecting, please wait...";

            clsClient clnt = new clsClient();
            clnt.SecureServerSocketEstablished += ServerSocketEstablished;
            clnt.LoginSuccessful += LoginSuccessed;
            clnt.LoginFailed += LoginFailed;
            clnt.StatusMessage += StatusMessage;

            if (!await clnt.ConnectAsync(textBox1.Text, (int)numericUpDown1.Value))
            {
                clnt.SecureServerSocketEstablished -= ServerSocketEstablished;
                clnt.LoginSuccessful -= LoginSuccessed;
                clnt.LoginFailed -= LoginFailed;
                clnt.StatusMessage -= StatusMessage;
            }
        }

        private void ServerSocketEstablished(clsClient clnt)
        {
            clnt.Login(textBox2.Text, textBox3.Text);
        }
        private void LoginSuccessed(clsClient clnt, string szUsername, int nAuthority)
        {
            MessageBox.Show("Login successfully.", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
            if (m_clnt != null)
            {
                m_clnt.Disconnect();
            }

            m_clnt = clnt;

            Invoke(new Action(Close));
        }
        private void LoginFailed(clsClient clnt, string szUsername, string szMsg)
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

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
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
