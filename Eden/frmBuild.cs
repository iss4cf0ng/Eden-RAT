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
    public partial class frmBuild : Form
    {
        private enum CommProtocol
        {
            TCP,
            DNS,
            HTTP,
        }

        private Client m_clnt;
        private List<List<string>> m_lsListener;

        public frmBuild(Client clnt, List<List<string>> lsListener)
        {
            InitializeComponent();

            m_clnt = clnt;
            m_lsListener = lsListener;
        }

        private void fnSetup()
        {
            foreach (List<string> ls in m_lsListener)
                comboBox1.Items.Add(ls[0]);

            if (comboBox1.Items.Count > 0)
                comboBox1.SelectedIndex = 0;

            textBox1.Text = m_clnt.m_szSrvIP;
        }

        private void frmBuild_Load(object sender, EventArgs e)
        {
            fnSetup();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            numericUpDown1.Value = int.Parse(m_lsListener[comboBox1.SelectedIndex][3]);
        }
    }
}
