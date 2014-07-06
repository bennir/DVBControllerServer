using Bonjour;
using Microsoft.Owin.Hosting;
using System;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;


namespace DVBViewerController
{


    public partial class DVBServer : Form
    {
        public IDisposable server = null;

        private short port = 0;

        public static int[] FavNumbers = new int[] { 38, 11, 12, 13, 14, 15, 16, 17, 18, 19 };

        delegate void addLogCallback(string msg);

        /**
         * Zeroconf
         */
        private DNSSDEventManager mEventManager = null;
        private DNSSDService mService = null;
        private DNSSDService mRegistrar = null;
        private DNSSDService mBrowser = null;

        /**
         * Recording Service
         */
        public string recIP = null;
        public string recPort = null;

        public DVBServer()
        {
            InitializeComponent();

            cbDebug.Checked = Properties.Settings.Default.debug;
            cbMinimizeOnStart.Checked = Properties.Settings.Default.minimizeOnStart;
            cbStartServer.Checked = Properties.Settings.Default.startServer;
            recIP = Properties.Settings.Default.recIP;
            recPort = Properties.Settings.Default.recPort;
            tbPort.Text = Properties.Settings.Default.port;

            try
            {
                mService = new DNSSDService();
            }
            catch
            {
                new BonjourErrorForm().ShowDialog();
                Application.Exit();
            }



            if (Properties.Settings.Default.startServer)
            {
                startServer();
            }

            if (Properties.Settings.Default.minimizeOnStart)
            {
                WindowState = FormWindowState.Minimized;
                Hide();
            }

            updateDebugDisplay();
        }

        /**
         * Zeroconf
         */
        #region Zeroconf

        private void AdvertiseDnsSd()
        {
            try
            {
                if (mService == null)
                {
                    mService = new DNSSDService();
                }

                mRegistrar = mService.Register(0, 0, System.Environment.UserName, "_dvbctrl._tcp", "local", null, (ushort)this.port, null, mEventManager);

                mEventManager = new DNSSDEventManager();
                mEventManager.ServiceRegistered += new _IDNSSDEvents_ServiceRegisteredEventHandler(this.ServiceRegistered);
            }
            catch (Exception ex)
            {
                addLog(ex.Message);
            }
        }

        public void ServiceRegistered(DNSSDService service, DNSSDFlags flags, String name, String regType, String domain)
        {
            try
            {
                mBrowser = mService.Browse(0, 0, "_dvbctrl._udp", null, mEventManager);
            }
            catch
            {
                MessageBox.Show("Browse Failed", "Error");
                Application.Exit();
            }
        }
        /**
         * Zeroconf End
         */
        #endregion

        private void startServer()
        {
            bool error = false;
            tbPort.Enabled = false;
            try
            {
                port = Convert.ToInt16(tbPort.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Bitte Zahl als Port angeben!");
            }

            string baseAddress = "http://+:" + port + "/";

            AdvertiseDnsSd();

            try
            {
                btnStart.Visible = false;
                btnStop.Visible = true;

                IDisposable s = WebApp.Start<DVBVCS>(url: baseAddress);
                server = s;
            }
            catch (Exception ex)
            {
                try
                {

                    // Elevation - Add Url ACL
                    NetAclChecker.AddAddress(baseAddress);

                    IDisposable s = WebApp.Start<DVBVCS>(url: baseAddress);
                    server = s;

                }
                catch (Exception ex2)
                {
                    addLog(ex2.Message);
                    error = true;
                }
            }

            if (!error)
            {
                addLog("Server running on Port " + port.ToString() + "...");
            }
        }

        private void stopServer()
        {
            tbPort.Enabled = true;

            if (!server.Equals(null))
            {
                server.Dispose();
                server = null;
            }

            btnStop.Visible = false;
            btnStart.Visible = true;

            mService.Stop();
            mRegistrar.Stop();

            mService = null;
            mRegistrar = null;

            addLog("Server stopping...");
        }

        public void addLog(string msg)
        {
            if (tbLog.InvokeRequired)
            {
                addLogCallback d = new addLogCallback(addLog);

                try
                {
                    this.Invoke(d, msg);
                }
                catch (Exception ex)
                {
                }
            }
            else
            {
                tbLog.Text += System.DateTime.Now.ToShortDateString() + " " + System.DateTime.Now.ToShortTimeString() + "\t" + msg + "\r\n";
                tbLog.SelectionStart = tbLog.Text.Length;
                tbLog.ScrollToCaret();
                tbLog.Refresh();
            }
        }

        private void runServer_Click(object sender, EventArgs e)
        {
            startServer();
        }

        private void stopServer_Click(object sender, EventArgs e)
        {
            stopServer();
        }

        private void DVBServer_FormClosing(object sender, FormClosingEventArgs e)
        {
            bool error = false;

            try
            {
                port = Convert.ToInt16(tbPort.Text);
            }
            catch (Exception ex)
            {
                error = true;
            }

            if (!error && port != 0)
            {
                Properties.Settings.Default.port = port.ToString();
            }

            Properties.Settings.Default.Save();

            if (server != null)
            {
                server.Dispose();
            }
        }

        public static bool IsIPv4(string value)
        {
            IPAddress address;

            if (IPAddress.TryParse(value, out address))
            {
                if (address.AddressFamily == AddressFamily.InterNetwork)
                {
                    return true;
                }
            }

            return false;
        }

        private void updateDebugDisplay()
        {
            if (Properties.Settings.Default.debug)
            {
                // 375; 460 default dpi
                int bottom = tbLog.Location.Y + tbLog.Height + 50;
                int right = tbLog.Location.X + tbLog.Width + 25;
                Size window = new Size(right, bottom);

                this.Size = window;
                this.MinimumSize = window;
                this.MaximumSize = window;

                btnDebug.Visible = true;
                tbLog.Visible = true;
            }
            else
            {
                // 375; 138 default dpi
                int bottom = btnDebug.Location.Y + btnDebug.Height + 50;
                int right = btnDebug.Location.X + btnDebug.Width + 25;
                Size window = new Size(right, bottom);

                this.Size = window;
                this.MinimumSize = window;
                this.MaximumSize = window;

                btnDebug.Visible = false;
                tbLog.Visible = false;
            }
        }

        private void DVBServer_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                DVBNotification.Visible = false;
            }
            else if (WindowState == FormWindowState.Minimized)
            {
                DVBNotification.Visible = true;
                Hide();
            }
        }

        private void DVBNotification_Click(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
            DVBNotification.Visible = false;
        }

        private void cbStartServer_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.startServer = cbStartServer.Checked;
        }

        private void cbMinimizeOnStart_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.minimizeOnStart = cbMinimizeOnStart.Checked;
        }

        private void cbDebug_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.debug = cbDebug.Checked;

            updateDebugDisplay();
        }

        private void btnDebug_Click(object sender, EventArgs e)
        {

        }

        private void btnRecService_Click(object sender, EventArgs e)
        {
            RecordingServiceForm rec = new RecordingServiceForm(this);
            rec.ShowDialog();
        }
    }
}
