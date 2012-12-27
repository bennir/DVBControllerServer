using System;
using System.Windows.Forms;

namespace DVBViewerController
{
    public partial class RecordingService : Form
    {
        DVBServer mainForm = null;

        public RecordingService(DVBServer form)
        {
            this.mainForm = form;

            InitializeComponent();

            if(this.mainForm.recIP != null)
                tbIP.Text = this.mainForm.recIP;
            if(this.mainForm.recPort != null)
                tbPort.Text = this.mainForm.recPort;
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            ushort port;

            try
            {
                port = UInt16.Parse(tbPort.Text);

                if(!DVBServer.IsIPv4(tbIP.Text))
                    throw new Exception();

                this.mainForm.recIP = tbIP.Text;
                this.mainForm.recPort = port.ToString();
                Properties.Settings.Default.recIP = tbIP.Text;
                Properties.Settings.Default.recPort = port.ToString();

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }            
        }
    }
}
