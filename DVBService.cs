using Griffin.Networking.Buffers;
using Griffin.Networking.Protocol.Http;
using Griffin.Networking.Protocol.Http.Protocol;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace DVBViewerController
{
    class DVBService : HttpService
    {
        private static readonly BufferSliceStack Stack = new BufferSliceStack(50, 32000);
        private DVBServer dvb;

        public DVBService(DVBServer dvb)
            : base(Stack)
        {
            this.dvb = dvb;
        }

        public override void Dispose()
        {
        }

        public override void OnRequest(IRequest request)
        {
            var response = request.CreateResponse(HttpStatusCode.OK, "Welcome");

            response.Body = new MemoryStream();
            response.ContentType = "text/plain";
            var buffer = HandleRequest(request.Uri.Query);
            response.Body.Write(buffer, 0, buffer.Length);
            response.Body.Position = 0;

            Send(response);

            dvb.addLog("Sent " + buffer.Length + " Bytes");
        }

        private byte[] HandleRequest(string sQuery)
        {
            byte[] b = new Byte[0];

            if (sQuery.Length > 0)
            {
                string command = sQuery.Substring(1);
                dvb.addLog("Command: " + command);

                if (!command.Contains('='))
                {
                    switch (command)
                    {
                        case "getFavList":
                            {
                                b = Encoding.UTF8.GetBytes(dvb.DVBgetFavList());
                                break;
                            }
                        case "getCurrentChanName":
                            {
                                b = Encoding.UTF8.GetBytes(dvb.DVBgetCurrentChanName());
                                break;
                            }
                        case "getRecordingService":
                            {
                                b = Encoding.UTF8.GetBytes(dvb.DVBgetRecordingService());
                                break;
                            }
                        case "sendMenu":
                            {
                                dvb.DVBsendMenu();

                                b = Encoding.UTF8.GetBytes("Success");
                                break;
                            }
                        case "sendOk":
                            {
                                dvb.DVBsendOk();

                                b = Encoding.UTF8.GetBytes("Success");
                                break;
                            }
                        case "sendLeft":
                            {
                                dvb.DVBsendLeft();

                                b = Encoding.UTF8.GetBytes("Success");
                                break;
                            }
                        case "sendRight":
                            {
                                dvb.DVBsendRight();

                                b = Encoding.UTF8.GetBytes("Success");
                                break;
                            }
                        case "sendUp":
                            {
                                dvb.DVBsendUp();

                                b = Encoding.UTF8.GetBytes("Success");
                                break;
                            }
                        case "sendDown":
                            {
                                dvb.DVBsendDown();

                                b = Encoding.UTF8.GetBytes("Success");
                                break;
                            }
                        case "sendBack":
                            {
                                dvb.DVBsendBack();

                                b = Encoding.UTF8.GetBytes("Success");
                                break;
                            }
                        case "sendRed":
                            {
                                dvb.DVBsendRed();

                                b = Encoding.UTF8.GetBytes("Success");
                                break;
                            }
                        case "sendYellow":
                            {
                                dvb.DVBsendYellow();

                                b = Encoding.UTF8.GetBytes("Success");
                                break;
                            }
                        case "sendGreen":
                            {
                                dvb.DVBsendGreen();

                                b = Encoding.UTF8.GetBytes("Success");
                                break;
                            }
                        case "sendBlue":
                            {
                                dvb.DVBsendBlue();

                                b = Encoding.UTF8.GetBytes("Success");
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
                                dvb.DVBsetFavChannel(value);

                                b = Encoding.UTF8.GetBytes("Success");
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
                                String resp = dvb.DVBgetChannelEPG(channelId, time, sYear, sMonth, sDay);

                                dvb.addLog("Sending:");
                                dvb.addLog(resp);

                                b = Encoding.UTF8.GetBytes(resp);
                                break;

                            }
                        case "getChannelLogo":
                            {
                                string value = command.Split('=')[1];
                                value = value.Replace('+', ' ');
                                value = Uri.UnescapeDataString(value);

                                string filename = dvb.DVBgetLogo(value);

                                if (filename != "")
                                {
                                    MemoryStream mStream = new MemoryStream();
                                    Bitmap image = dvb.ResizeToLongSide(Image.FromFile(filename), 200);
                                    image.Save(mStream, ImageFormat.Png);

                                    Byte[] img = mStream.ToArray();
                                    int size = img.Length;

                                    /*
                                    Byte[] res = BuildImageHeader(sHttpVersion, size);
                                    clientStream.Write(res, 0, res.Length);
                                    clientStream.Write(img, 0, size);
                                    addLog("Sent " + size + " Bytes Picture");
                                    */
                                    b = img;

                                    mStream.Close();
                                }

                                break;
                            }
                        default:
                            break;
                    }
                }
            }

            return b;
        }

    }
}
