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
    public partial class frmListenerEdit : Form
    {
        public clsClient m_clnt;
        public Func<string, int, bool> m_atCheckExists;

        public bool m_bEdit = false;
        public struct stListener
        {
            public string szName;
            public string szTemplate;
            public int nPort;
        }
        public stListener m_stListener;

        public frmListenerEdit()
        {
            InitializeComponent();
        }

        void Received(clsClient clnt, string szVictimID, List<string> aMsg)
        {
            try
            {
                if (aMsg[0] == "listener")
                {
                    if (aMsg[1] == "list")
                    {
                        if (aMsg[2] == "temp")
                        {
                            List<string> lsTemplate = clsTools.EZData.String2OneDList(aMsg[3]);
                            Invoke(new Action(() =>
                            {
                                foreach (string szTemplate in lsTemplate)
                                    comboBox1.Items.Add(szTemplate);

                                if (comboBox1.Items.Count > 0)
                                    comboBox1.SelectedIndex = 0;
                            }));
                        }
                    }
                    else if (aMsg[1] == "add")
                    {
                        int nCode = int.Parse(aMsg[2]);
                        if (nCode == 1)
                        {
                            MessageBox.Show("Add listener successfully.", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            Close();
                        }
                        else
                        {
                            MessageBox.Show("Add listener failed.", "Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else if (aMsg[1] == "edit")
                    {
                        int nCode = int.Parse(aMsg[2]);
                        if (nCode == 1)
                        {
                            MessageBox.Show("Edit listener successfully.", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            Close();
                        }
                        else
                        {
                            MessageBox.Show("Edit listener failed.", "Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else if (aMsg[1] == "del")
                    {
                        int nCode = int.Parse(aMsg[2]);
                        if (nCode == 1)
                        {
                            MessageBox.Show("Delete listener successfully.", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("Delete listener failed.", "Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        void setup()
        {
            m_clnt.ServerMessageReceived += Received;

            if (m_bEdit)
            {
                textBox1.Text = m_stListener.szName;
                comboBox1.Text = m_stListener.szTemplate;
                numericUpDown1.Value = m_stListener.nPort;
            }
            else
            {
                m_clnt.SendCommand("listener|list|temp");
            }
        }

        private void frmListenerEdit_Load(object sender, EventArgs e)
        {
            setup();
        }

        //Save
        private void button1_Click(object sender, EventArgs e)
        {
            if (m_atCheckExists.Invoke(textBox1.Text, (int)numericUpDown1.Value) && !m_bEdit)
            {
                DialogResult dr = MessageBox.Show("Listener exists. Do you want to overwrite?", "Overwrite", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dr != DialogResult.Yes)
                    return;
            }

            if (m_bEdit)
                m_clnt.SendCommand(string.Join("|", new string[]
                {
                    "listener",
                    "edit",
                    comboBox1.Text, //Template
                    EZCrypto.Encoder.stre2b64(m_stListener.szName), //Original name
                    EZCrypto.Encoder.stre2b64(textBox1.Text), //New name
                    numericUpDown1.Value.ToString(), //Listen port
                }));
            else
                m_clnt.SendCommand($"listener|add|{comboBox1.Text}|{EZCrypto.Encoder.stre2b64(textBox1.Text)}|{numericUpDown1.Value}");
        }
    }
}
