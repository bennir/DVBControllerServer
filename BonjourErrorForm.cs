using System;
using System.Windows.Forms;

namespace DVBViewerController
{
    public partial class BonjourErrorForm : Form
    {
        public BonjourErrorForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void linkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://support.apple.com/kb/DL999");
        }
    }
}
