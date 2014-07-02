using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Bonjour;
using DVBViewerServer;
using Microsoft.Owin.Hosting;
using DVBViewerController.Models;


namespace DVBViewerController
{
    

    public partial class DVBServer : Form
    {
        public IDisposable server = null;

        private short                       port =              0;

        public int[]                        FavNumbers =        new int[] { 38, 11, 12, 13, 14, 15, 16, 17, 18, 19 };

        delegate void addLogCallback(string msg);

        /**
         * Zeroconf
         */
        private DNSSDEventManager           mEventManager =     null;
        private DNSSDService                mService =          null;
        private DNSSDService                mRegistrar =        null;
        private DNSSDService                mBrowser =          null;
        private String                      mName;

        /**
         * Recording Service
         */
        public string                       recIP =             null;
        public string                       recPort =           null;

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

            try
            {
                mRegistrar = mService.Register(0, 0, System.Environment.UserName, "_dvbctrl._tcp", "local", null, (ushort)this.port, null, mEventManager);
            }
            catch (Exception ex)
            {
                addLog(ex.Message);
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

            /**
             * Zeroconf
             */
            try
            {
                mService = new DNSSDService();
            }
            catch
            {
                MessageBox.Show("Bonjour Service is not available", "Error");
                Application.Exit();
            }

            mEventManager = new DNSSDEventManager();
            mEventManager.ServiceRegistered += new _IDNSSDEvents_ServiceRegisteredEventHandler(this.ServiceRegistered);
        }

        private void startServer()
        {
            tbPort.Enabled = false;

            bool error = false;
            try
            {
                port = Convert.ToInt16(tbPort.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Bitte Zahl als Port angeben!");
            }

            if (!error)
            {
                try
                {
                    runServer.Visible = false;
                    stopServer.Visible = true;

                    port = Convert.ToInt16(tbPort.Text);


                    string baseAddress = "http://+:" + port + "/";

                    IDisposable s = WebApp.Start<DVBVCS>(url: baseAddress);
                    server = s;

                    addLog("Server running on Port " + port.ToString() + "...");
                }
                catch (Exception ex)
                {
                    addLog(ex.Message);
                }
            }
        }

        public Bitmap ResizeToLongSide(Image src, int size)
        {
            Bitmap dst;

            int oldWidth = src.Size.Width;
            int oldHeight = src.Size.Height;

            if (oldWidth == oldHeight)
                dst = new Bitmap(size, size);
            else if (oldWidth > oldHeight)
            {
                dst = new Bitmap(size, (size*oldHeight/oldWidth));
            }
            else
            {
                dst = new Bitmap((size * oldWidth / oldHeight), size);
            }

            using (Graphics g = Graphics.FromImage(dst))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                //MessageBox.Show(dst.Width.ToString() + "x" + dst.Height.ToString());
                g.DrawImage(src, 0, 0, dst.Width, dst.Height);
            }

            return dst;
        }

        #region DVB Commands

        public void DVBsendMenu()
        {
            DVBViewer dvb;

            try
            {
                dvb = (DVBViewer)System.Runtime.InteropServices.Marshal.GetActiveObject("DVBViewerServer.DVBViewer");

                dvb.SendCommand(111);
            }
            catch (Exception ex)
            {
                addLog(ex.Message);
            }
        }

        public void DVBsendOk()
        {
            DVBViewer dvb;

            try
            {
                dvb = (DVBViewer)System.Runtime.InteropServices.Marshal.GetActiveObject("DVBViewerServer.DVBViewer");

                dvb.SendCommand(73);
            }
            catch (Exception ex)
            {
                addLog(ex.Message);
            }
        }

        public void DVBsendLeft()
        {
            DVBViewer dvb;

            try
            {
                dvb = (DVBViewer)System.Runtime.InteropServices.Marshal.GetActiveObject("DVBViewerServer.DVBViewer");

                dvb.SendCommand(2000);
            }
            catch (Exception ex)
            {
                addLog(ex.Message);
            }
        }

        public void DVBsendRight()
        {
            DVBViewer dvb;

            try
            {
                dvb = (DVBViewer)System.Runtime.InteropServices.Marshal.GetActiveObject("DVBViewerServer.DVBViewer");

                dvb.SendCommand(2100);
            }
            catch (Exception ex)
            {
                addLog(ex.Message);
            }
        }

        public void DVBsendVolUp()
        {
            DVBViewer dvb;

            try
            {
                dvb = (DVBViewer)System.Runtime.InteropServices.Marshal.GetActiveObject("DVBViewerServer.DVBViewer");

                dvb.SendCommand(26);
            }
            catch (Exception ex)
            {
                addLog(ex.Message);
            }
        }

        public void DVBsendVolDown()
        {
            DVBViewer dvb;

            try
            {
                dvb = (DVBViewer)System.Runtime.InteropServices.Marshal.GetActiveObject("DVBViewerServer.DVBViewer");

                dvb.SendCommand(27);
            }
            catch (Exception ex)
            {
                addLog(ex.Message);
            }
        }

        public void DVBsendUp()
        {
            DVBViewer dvb;

            try
            {
                dvb = (DVBViewer)System.Runtime.InteropServices.Marshal.GetActiveObject("DVBViewerServer.DVBViewer");

                dvb.SendCommand(78);
            }
            catch (Exception ex)
            {
                addLog(ex.Message);
            }
        }

        public void DVBsendDown()
        {
            DVBViewer dvb;

            try
            {
                dvb = (DVBViewer)System.Runtime.InteropServices.Marshal.GetActiveObject("DVBViewerServer.DVBViewer");

                dvb.SendCommand(79);
            }
            catch (Exception ex)
            {
                addLog(ex.Message);
            }
        }

        public void DVBsendBack()
        {
            DVBViewer dvb;

            try
            {
                dvb = (DVBViewer)System.Runtime.InteropServices.Marshal.GetActiveObject("DVBViewerServer.DVBViewer");

                dvb.SendCommand(84);
            }
            catch (Exception ex)
            {
                addLog(ex.Message);
            }
        }

        public void DVBsendRed()
        {
            DVBViewer dvb;

            try
            {
                dvb = (DVBViewer)System.Runtime.InteropServices.Marshal.GetActiveObject("DVBViewerServer.DVBViewer");

                dvb.SendCommand(74);
            }
            catch (Exception ex)
            {
                addLog(ex.Message);
            }
        }

        public void DVBsendYellow()
        {
            DVBViewer dvb;

            try
            {
                dvb = (DVBViewer)System.Runtime.InteropServices.Marshal.GetActiveObject("DVBViewerServer.DVBViewer");

                dvb.SendCommand(76);
            }
            catch (Exception ex)
            {
                addLog(ex.Message);
            }
        }

        public void DVBsendGreen()
        {
            DVBViewer dvb;

            try
            {
                dvb = (DVBViewer)System.Runtime.InteropServices.Marshal.GetActiveObject("DVBViewerServer.DVBViewer");

                dvb.SendCommand(75);
            }
            catch (Exception ex)
            {
                addLog(ex.Message);
            }
        }

        public void DVBsendBlue()
        {
            DVBViewer dvb;

            try
            {
                dvb = (DVBViewer)System.Runtime.InteropServices.Marshal.GetActiveObject("DVBViewerServer.DVBViewer");

                dvb.SendCommand(77);
            }
            catch (Exception ex)
            {
                addLog(ex.Message);
            }
        }

        public void DVBsetFavChannel(string channelId)
        {
            DVBViewer dvb;

            try
            {
                dvb = (DVBViewer)System.Runtime.InteropServices.Marshal.GetActiveObject("DVBViewerServer.DVBViewer");

                foreach (char x in channelId)
                {
                    int cmd = Int16.Parse(x.ToString());
                    dvb.SendCommand(FavNumbers[cmd]);
                }
            }
            catch (Exception ex)
            {
                addLog(ex.Message);
            }
        }


        public string DVBgetChannelEPG(string channelId, string time, string sYear, string sMonth, string sDay)
        {
            DVBViewer dvb;
            string resp = "";

            try
            {
                dvb = (DVBViewer)System.Runtime.InteropServices.Marshal.GetActiveObject("DVBViewerServer.DVBViewer");

                IChannelCollection col = dvb.ChannelManager;

                int channelNr = 0;
                IChannelItem channel = col.GetChannel(channelId, ref channelNr);

                IEPGManager epgManager = dvb.EPGManager;

                DateTime start = DateTime.Now;
                DateTime stop = DateTime.Now;

                int year = Convert.ToInt16(sYear);
                int month = Convert.ToInt16(sMonth);
                int day = Convert.ToInt16(sDay);


                switch (time)
                {
                    case "Current":
                        start = new DateTime(year, month, day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                        stop = start.AddHours(2);
                        break;
                    case "In 1 Hour":
                        addLog("In 1 Hour");
                        start = new DateTime(year, month, day, DateTime.Now.AddHours(1).Hour, DateTime.Now.Minute, DateTime.Now.Second);
                        stop = start.AddHours(2);
                        break;
                    case "20:15":
                        start = new DateTime(year, month, day, 20, 15, 0);
                        stop = start.AddHours(2);
                        break;
                    case "Morning":
                        start = new DateTime(year, month, day, 6, 0, 0);
                        stop = start.AddHours(5);
                        break;
                    case "Noon":
                        start = new DateTime(year, month, day, 11, 0, 0);
                        stop = start.AddHours(5);
                        break;
                    case "Afternoon":
                        start = new DateTime(year, month, day, 14, 0, 0);
                        stop = start.AddHours(5);
                        break;
                    case "Evening":
                        start = new DateTime(year, month, day, 17, 0, 0);
                        stop = start.AddHours(5);
                        break;
                    case "Night":
                        start = new DateTime(year, month, day, 22, 0, 0);
                        stop = start.AddHours(5);
                        break;
                }

                addLog(start.ToShortDateString() + " " + start.ToShortTimeString() + " - " + stop.ToShortDateString() + " " + stop.ToShortTimeString());

                IEPGCollection epgCol = epgManager.Get(channel.Tuner.SID, channel.Tuner.TransportStreamID, start, stop);
                
                resp += "{ \"epg\": [";

                for (int i = 0; i < epgCol.Count; i++)
                {
                    string duration = epgCol[i].Duration.Hour + "h " + epgCol[i].Duration.Minute + "min";
                    if (i != (epgCol.Count - 1))
                    {
                        resp += "{ \"time\" : \"" + epgCol[i].Time.ToShortTimeString() + "\", \"channel\" : \"" + channel.Name + "\", \"title\" : \"" + Uri.EscapeUriString(epgCol[i].Title) + "\", \"desc\" : \"" + Uri.EscapeUriString(epgCol[i].Description.Split('[')[0]) + "\", \"duration\" : \"" + duration + "\" },";
                    }
                    else
                    {
                        resp += "{ \"time\" : \"" + epgCol[i].Time.ToShortTimeString() + "\", \"channel\" : \"" + channel.Name + "\", \"title\" : \"" + Uri.EscapeUriString(epgCol[i].Title) + "\", \"desc\" : \"" + Uri.EscapeUriString(epgCol[i].Description.Split('[')[0]) + "\", \"duration\" : \"" + duration + "\" }";
                    }
                }
                resp += "] }";
            }
            catch (Exception ex)
            {
                addLog(ex.Message);
            }
            return resp;
        }

        public string DVBgetCurrentChanName()
        {
            DVBViewer dvb;
            string resp = "";

            try
            {
                dvb = (DVBViewer)System.Runtime.InteropServices.Marshal.GetActiveObject("DVBViewerServer.DVBViewer");

                

                IChannelItem chan = dvb.CurrentChannel;

                resp = chan.Name;
            }
            catch (Exception ex)
            {
                addLog(ex.Message);
            }
            return resp;
        }

        public string DVBgetRecordingService()
        {
            string resp = "";
            ushort port;

            try
            {
                port = UInt16.Parse(recPort);

                if (!IsIPv4(recIP))
                    throw new Exception();

                resp += "{ \"recordingService\": {";
                resp += "  \"ip\": \"" + recIP + "\",";
                resp += "  \"port\": \"" + recPort + "\"";
                resp += "} }";
            }
            catch (Exception ex)
            {
                addLog("DVBViewer not running");
                resp += "{ \"recordingService\": {";
                resp += "  \"ip\": \"0.0.0.0\",";
                resp += "  \"port\": \"0\"";
                resp += "} }";
            }

            return resp;
        }

        public string DVBgetLogo(string file)
        {
            DVBViewer dvb;
            string filename = "";
            try
            {
                dvb = (DVBViewer)System.Runtime.InteropServices.Marshal.GetActiveObject("DVBViewerServer.DVBViewer");


                IDataManager data = dvb.DataManager;

                string search = file;
                search = search.ToLower();

                string pattern = "\\s\\(.+\\)";
                Regex rgx = new Regex(pattern);
                search = rgx.Replace(search, "");

                pattern = "\\.";
                rgx = new Regex(pattern);
                search = rgx.Replace(search, " ");

                string appfolder = data.get_Value("#appfolder") + "Images\\Logos\\";

                String[] logoFiles = Directory.GetFiles(appfolder, search+"*.png", SearchOption.AllDirectories);
                if (logoFiles.Length != 0)
                {
                    filename = logoFiles[0];
                }
                addLog(filename);
                     
            }
            catch (Exception ex)
            {
                addLog(ex.Message);
            }

            return filename;
        }

        #endregion

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
            tbPort.Enabled = true;

            if (!server.Equals(null))
            {
                server.Dispose();
            }

            stopServer.Visible = false;
            runServer.Visible = true;

            addLog("Server stopping...");
        }

        private void DVBServer_FormClosing(object sender, FormClosingEventArgs e)
        {
            bool error = false;
            int port = 0;

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

            if (!server.Equals(null))
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



        /**
         * Zeroconf
         */
        #region Zeroconf

        public void ServiceRegistered(DNSSDService service, DNSSDFlags flags, String name, String regType, String domain)
        {
            mName = name;

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

        private void btnDebug_Click(object sender, EventArgs e)
        {
            DVBsendMenu();
        }

        private void btnRecService_Click(object sender, EventArgs e)
        {
            RecordingService rec = new RecordingService(this);
            rec.ShowDialog();
        }
    }
}
