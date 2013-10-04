﻿using System;
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


namespace DVBViewerController
{
    public partial class DVBServer : Form
    {
        private TcpListener                 tcpListener =       null;
        private short                       port =              0;
        private bool                        running =           false;

        public int[]                        FavNumbers =        new int[] { 38, 11, 12, 13, 14, 15, 16, 17, 18, 19 };

        Thread                              listenThread =      null;
        private Object                      thisLock =          new Object();
        private volatile bool KillThreads = false;

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

            if (running)
            {
                runServer.Visible = false;
                stopServer.Visible = true;
            }
            else
            {
                stopServer.Visible = false;
                runServer.Visible = true;
            }

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

            if (!error)
            {
                try
                {
                    tcpListener = new TcpListener(IPAddress.Any, port);

                    running = true;
                    runServer.Visible = false;
                    stopServer.Visible = true;

                    addLog("Server running on Port " + port.ToString() + "...");

                    listenThread = new Thread(new ThreadStart(ListenForClients));
                    listenThread.Start();
                }
                catch (Exception ex)
                {
                    addLog(ex.Message);
                }
            }
        }

        private void ListenForClients()
        {
            if (KillThreads)
                return;

            tcpListener.Start();

            while (true)
            {
                try
                {
                    TcpClient client = this.tcpListener.AcceptTcpClient();

                    Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
                    clientThread.Start(client);
                } catch (Exception ex) {
                    addLog(ex.Message);
                }
            }
        }

        private void HandleClientComm(object client)
        {
            if (KillThreads)
                return;

            TcpClient tcpClient = (TcpClient)client;
            NetworkStream clientStream = tcpClient.GetStream();
            ASCIIEncoding encoder = new ASCIIEncoding();

            byte[] message = new byte[4096];
            int bytesRead;

            while (true)
            {
                bytesRead = 0;

                try
                {
                    bytesRead = clientStream.Read(message, 0, 4096);
                }
                catch
                {
                    break;
                }

                if (bytesRead == 0)
                {
                    break;
                }

                string sBuffer = encoder.GetString(message, 0, bytesRead);

                addLog(sBuffer);

                // Only GET
                if (sBuffer.Length > 3)
                {
                    if (sBuffer.Substring(0, 3) != "GET")
                    {
                        addLog("Only GET requests are supported");

                        break;
                    }
                }

                // Look for HTTP request
                int iStartPos = sBuffer.IndexOf("HTTP", 1);

                // Get HTTP Text and Version
                string sHttpVersion = sBuffer.Substring(iStartPos, 8);

                // Extract the Requested Type and Requested file/directory
                string sRequest = sBuffer.Substring(0, iStartPos - 1);

                // Extract QueryString
                string sQuery = sRequest.Substring(sRequest.IndexOf("/", 1)+1, sRequest.Length - sRequest.IndexOf("/", 1) - 1);

                if (sQuery.Length > 0 && sRequest.IndexOf("?") != -1)
                {
                    string command = sQuery.Substring(1, sQuery.Length - 1);
                    addLog("Command: " + command);
                    string msg;

                    if (!command.Contains('='))
                    {
                        switch (command)
                        {
                            case "getFavList":
                                {
                                    msg = DVBgetFavList();
                                    
                                    Byte[] res = BuildResponse(sHttpVersion, msg);
                                    clientStream.Write(res, 0, res.Length);

                                    break;
                                }
                            case "getCurrentChanName":
                                {
                                    msg = DVBgetCurrentChanName();

                                    Byte[] res = BuildResponse(sHttpVersion, msg);
                                    clientStream.Write(res, 0, res.Length);

                                    break;
                                }
                            case "getRecordingService":
                                {
                                    msg = DVBgetRecordingService();

                                    Byte[] res = BuildResponse(sHttpVersion, msg);
                                    clientStream.Write(res, 0, res.Length);

                                    break;
                                }
                            case "sendMenu":
                                {
                                    DVBsendMenu();

                                    Byte[] res = BuildResponse(sHttpVersion, "Success");
                                    clientStream.Write(res, 0, res.Length);

                                    break;
                                }
                            case "sendOk":
                                {
                                    DVBsendOk();

                                    Byte[] res = BuildResponse(sHttpVersion, "Success");
                                    clientStream.Write(res, 0, res.Length);

                                    break;
                                }
                            case "sendLeft":
                                {
                                    DVBsendLeft();

                                    Byte[] res = BuildResponse(sHttpVersion, "Success");
                                    clientStream.Write(res, 0, res.Length);

                                    break;
                                }
                            case "sendRight":
                                {
                                    DVBsendRight();

                                    Byte[] res = BuildResponse(sHttpVersion, "Success");
                                    clientStream.Write(res, 0, res.Length);

                                    break;
                                }
                            case "sendUp":
                                {
                                    DVBsendUp();

                                    Byte[] res = BuildResponse(sHttpVersion, "Success");
                                    clientStream.Write(res, 0, res.Length);

                                    break;
                                }
                            case "sendDown":
                                {
                                    DVBsendDown();

                                    Byte[] res = BuildResponse(sHttpVersion, "Success");
                                    clientStream.Write(res, 0, res.Length);

                                    break;
                                }
                            case "sendBack":
                                {
                                    DVBsendBack();

                                    Byte[] res = BuildResponse(sHttpVersion, "Success");
                                    clientStream.Write(res, 0, res.Length);

                                    break;
                                }
                            case "sendRed":
                                {
                                    DVBsendRed();

                                    Byte[] res = BuildResponse(sHttpVersion, "Success");
                                    clientStream.Write(res, 0, res.Length);

                                    break;
                                }
                            case "sendYellow":
                                {
                                    DVBsendYellow();

                                    Byte[] res = BuildResponse(sHttpVersion, "Success");
                                    clientStream.Write(res, 0, res.Length);

                                    break;
                                }
                            case "sendGreen":
                                {
                                    DVBsendGreen();

                                    Byte[] res = BuildResponse(sHttpVersion, "Success");
                                    clientStream.Write(res, 0, res.Length);

                                    break;
                                }
                            case "sendBlue":
                                {
                                    DVBsendBlue();

                                    Byte[] res = BuildResponse(sHttpVersion, "Success");
                                    clientStream.Write(res, 0, res.Length);

                                    break;
                                }
                            default:
                                {
                                    break;
                                }
                        }
                    }
                    else
                    {
                        string cmd = command.Split('=')[0];

                        //MessageBox.Show("Cmd: " + cmd);



                        switch (cmd)
                        {
                            case "setChannel":
                                {
                                    string value = command.Split('=')[1];
                                    DVBsetFavChannel(value);

                                    Byte[] res = BuildResponse(sHttpVersion, "Success");
                                    clientStream.Write(res, 0, res.Length);

                                    break;
                                }
                            case "getEPG":
                                {
                                    string channelId = "";
                                    string time = "";
                                    string sYear = "";
                                    string sMonth = "";
                                    string sDay = "";

                                    string[] cmds = command.Split('&');

                                    foreach (string befehl in cmds)
                                    {
                                        //MessageBox.Show(befehl);
                                        string[] x = befehl.Split('=');
                                        switch (x[0])
                                        {
                                            case "getEPG":
                                                {
                                                    string value = x[1];
                                                    value = value.Replace('+', ' ');
                                                    channelId = Uri.UnescapeDataString(value);
                                                    break;
                                                }
                                            case "time":
                                                {
                                                    string value = x[1];
                                                    value = value.Replace('+', ' ');
                                                    time = Uri.UnescapeDataString(value);
                                                    break;
                                                }
                                            case "year":
                                                {
                                                    sYear = x[1];
                                                    break;
                                                }
                                            case "month":
                                                {
                                                    sMonth = x[1];
                                                    break;
                                                }
                                            case "day":
                                                {
                                                    sDay = x[1];
                                                    break;
                                                }
                                        }
                                    }

                                    //MessageBox.Show(channelId + "-" + time + "-" + sYear + "-" + sMonth + "-" + sDay);
                                    msg = DVBgetChannelEPG(channelId, time, sYear, sMonth, sDay);

                                    addLog("Sending:");
                                    addLog(msg);

                                    Byte[] res = BuildResponse(sHttpVersion, msg);
                                    clientStream.Write(res, 0, res.Length);

                                    break;

                                }
                            case "getChannelLogo":
                                {
                                    string value = command.Split('=')[1];
                                    value = value.Replace('+', ' ');
                                    value = Uri.UnescapeDataString(value);

                                    string filename = DVBgetLogo(value);

                                    if (filename != "")
                                    {
                                        MemoryStream mStream = new MemoryStream();
                                        Bitmap image = ResizeToLongSide(Image.FromFile(filename), 200);
                                        image.Save(mStream, ImageFormat.Png);

                                        Byte[] img = mStream.ToArray();
                                        int size = img.Length;

                                        Byte[] res = BuildImageHeader(sHttpVersion, size);
                                        clientStream.Write(res, 0, res.Length);
                                        clientStream.Write(img, 0, size);
                                        addLog("Sent " + size + " Bytes Picture");

                                        mStream.Close();
                                    }
                                    else
                                    {
                                        clientStream.Close();
                                    }

                                    break;
                                }
                            default:
                                break;
                        }
                    }
                }
                clientStream.Close();
            } // while ende
            tcpClient.Close();
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

        public Byte[] BuildImageHeader(string sHttpVersion, int bytes)
        {
            string HttpDate = DateTime.Now.ToUniversalTime().ToString("r");
            String sBuffer = "";
            sBuffer += "HTTP/1.1 200 OK\n";
            sBuffer += "Date: " + HttpDate + "\n";
            sBuffer += "Content-Type: image/png\n";
            sBuffer += "Connection: close\n";
            sBuffer += "Content-Length: " + bytes + "\n\n";

            Byte[] res = Encoding.ASCII.GetBytes(sBuffer);

            addLog("Sent " + res.Length + " Bytes Header");

            return res;
        }

        public Byte[] Build404Response(string sHttpVersion)
        {
            string HttpDate = DateTime.Now.ToUniversalTime().ToString("r");
            String sBuffer = "";
            sBuffer += "HTTP/1.1 404 Not Found" + "\n";
            sBuffer += "Date: " + HttpDate + "\n";
            sBuffer += "Connection: close\n";
            sBuffer += "Content-Type: text/html\n\n";

            Byte[] bSendData = Encoding.UTF8.GetBytes(sBuffer);

            Byte[] res = Encoding.UTF8.GetBytes(sBuffer);
            addLog("Sent " + res.Length + " Bytes");

            return res;
        }

        public Byte[] BuildResponse(string sHttpVersion, string sData)
        {
            Byte[] data = Encoding.UTF8.GetBytes(sData);
            string HttpDate = DateTime.Now.ToUniversalTime().ToString("r");
            String sBuffer = "";
            sBuffer += "HTTP/1.1 200 OK\n";
            sBuffer += "Content-Type: text/html; charset=UTF-8\n";
            sBuffer += "Connection: close\n";
            sBuffer += "Content-Length: " + sData.Length + "\n";
            sBuffer += "Server: DVBViewer Controller Server\n";
            sBuffer += "Date: " + HttpDate + "\n\n";
            sBuffer += sData;

            Byte[] res = Encoding.UTF8.GetBytes(sBuffer);
            addLog("Sent " + res.Length + " Bytes");

            return res;
        }

        #region DVB Commands

        private void DVBsendMenu()
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

        private void DVBsendOk()
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

        private void DVBsendLeft()
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

        private void DVBsendRight()
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

        private void DVBsendUp()
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

        private void DVBsendDown()
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

        private void DVBsendBack()
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

        private void DVBsendRed()
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

        private void DVBsendYellow()
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

        private void DVBsendGreen()
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

        private void DVBsendBlue()
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

        private void DVBsetFavChannel(string channelId)
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
        

        private string DVBgetChannelEPG(string channelId, string time, string sYear, string sMonth, string sDay)
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

        private string DVBgetCurrentChanName()
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

        private string DVBgetFavList()
        {
            DVBViewer dvb;
            string resp="";

            try
            {
                dvb = (DVBViewer)System.Runtime.InteropServices.Marshal.GetActiveObject("DVBViewerServer.DVBViewer");

                int i;

                IFavoritesManager fav = dvb.FavoritesManager;
                IFavoritesCollection favcol = fav.GetFavorites();
                IEPGManager epgManager = dvb.EPGManager;

                DateTime start = DateTime.Now;
                DateTime stop = start.AddSeconds(1);

                resp += "{ \"channels\": [";

                for (i = 0; i < favcol.Count; i++)
                {
                    String channelName = favcol[i].Name;
                    string pattern = "\\s\\(.+\\)";
                    Regex rgx = new Regex(pattern);
                    channelName = rgx.Replace(channelName, "");

                    resp += "{";
                    resp += "\"name\" : \"" + channelName + "\",";
                    resp += "\"id\" : \"" + favcol[i].Nr + "\",";
                    resp += "\"group\" : \"" + favcol[i].Group + "\",";
                    resp += "\"channelid\" : \"" + favcol[i].ChannelID + "\"";

                    try
                    {
                        IChannelCollection col = dvb.ChannelManager;

                        int channelNr = 0;
                        IChannelItem channel = col.GetChannel(favcol[i].ChannelID, ref channelNr);
                        IEPGCollection epgCol = epgManager.Get(channel.Tuner.SID, channel.Tuner.TransportStreamID, start, stop);

                        string epgTitle = epgCol[0].Title;
                        string epgTime = epgCol[0].Time.ToShortTimeString();
                        string epgDuration = epgCol[0].Duration.ToShortTimeString();

                        resp += ",";
                        resp += "\"epgtitle\" : \"" + Uri.EscapeDataString(epgTitle) + "\",";
                        resp += "\"epgtime\" : \"" + epgTime + "\",";
                        resp += "\"epgduration\" : \"" + epgDuration + "\"";
                    }
                    catch (Exception ex)
                    {
                        // Can not retrieve EPG
                        resp += ",";
                        resp += "\"epgtitle\" : \"\",";
                        resp += "\"epgtime\" : \"\",";
                        resp += "\"epgduration\" : \"\"";
                    }

                    resp += "}";
                    if (i != (favcol.Count - 1))
                        resp += ",";
                }
                resp += "] }";
            }
            catch (Exception ex)
            {
                addLog("DVBViewer not running");
                resp += "{ \"channels\": [";
                resp += "] }";
            }
            return resp;
        }

        private string DVBgetRecordingService()
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

        private string DVBgetLogo(string file)
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

        private void addLog(string msg)
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

            if (running)
            {
                tcpListener.Stop();
                listenThread.Abort();
                running = false;
                stopServer.Visible = false;
                runServer.Visible = true;

                addLog("Server stopping...");
            }

            mService.Stop();
            mRegistrar.Stop();
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

            if (!listenThread.Join(0))
            {
                e.Cancel = true;
                KillThreads = true;
                tcpListener.Stop();
                listenThread.Abort();
                var timer = new System.Timers.Timer();
                timer.AutoReset = false;
                timer.SynchronizingObject = this;
                timer.Interval = 1000;
                timer.Elapsed += (send, args) =>
                {
                    if (listenThread.Join(0))
                    {
                        Close();
                    }
                    else
                    {
                        timer.Start();
                    }
                };
                timer.Start();
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
            

        }

        private void DVBServer_Load(object sender, EventArgs e)
        {
            /*
            try
            {
                mRegistrar = mService.Register(0, 0, System.Environment.UserName, "_dvbctrl._tcp", "local", null, (ushort)this.port, null, mEventManager);
            }
            catch (Exception ex)
            {
                addLog(ex.Message);
            }
             */
        }

        private void btnRecService_Click(object sender, EventArgs e)
        {
            RecordingService rec = new RecordingService(this);
            rec.ShowDialog();
        }
    }
}
