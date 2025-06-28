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
    public partial class frmShell : Form
    {
        private Client m_clnt;
        private string m_szVictimID;

        public frmShell(Client clnt, string szVictimID)
        {
            InitializeComponent();

            m_clnt = clnt;
            m_szVictimID = szVictimID;
        }

        void fnSetup()
        {
            //Controls
            StartPosition = FormStartPosition.CenterScreen;
        }

        private void frmShell_Load(object sender, EventArgs e)
        {
            fnSetup();
        }
    }
}
