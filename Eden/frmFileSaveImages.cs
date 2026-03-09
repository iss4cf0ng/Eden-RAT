using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Eden
{
    public partial class frmFileSaveImages : Form
    {
        private clsVictim m_victim { get; init; }
        private List<frmFileImage.stImageItem> m_lsImages { get; init; }
        private string m_szDir { get; set; }

        public frmFileSaveImages(clsVictim victim, List<frmFileImage.stImageItem> lsImages)
        {
            InitializeComponent();

            m_victim = victim;
            m_lsImages = lsImages;
            m_szDir = Path.Combine(m_victim.m_szDirectory, "Images", clsTools.GetFileNameFromDatetime());

            Text = @$"Save Images\\{victim.m_szID}";
        }

        void fnSave()
        {
            string szDir = m_szDir;
            if (!Directory.Exists(szDir))
                Directory.CreateDirectory(szDir);

            _ = Task.Run(() =>
            {
                try
                {
                    for (int i = 0; i < m_lsImages.Count; i++)
                    {
                        string szFilePath = Path.Combine(szDir, Path.GetFileName(m_lsImages[i].szFilename));
                        Image img = m_lsImages[i].img;

                        Invoke(() =>
                        {
                            using (Bitmap bmp = new Bitmap(img))
                            {
                                bmp.Save(szFilePath, ImageFormat.Png);
                            }

                            richTextBox1.AppendText($"[{DateTime.Now.ToString("F")}] Saved image: " + szFilePath);
                            richTextBox1.AppendText(Environment.NewLine);

                            progressBar1.Increment(1);
                        });
                    }

                    MessageBox.Show("Save images successfully.", "Finished", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, ex.GetType().Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            });
        }

        void fnSetup()
        {
            progressBar1.Maximum = m_lsImages.Count;
            progressBar1.Value = 0;

            fnSave();
        }

        private void frmFileSaveImages_Load(object sender, EventArgs e)
        {
            fnSetup();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName = "explorer.exe",
                Arguments = m_szDir,
                CreateNoWindow = true,
                UseShellExecute = false,
            });
        }
    }
}
