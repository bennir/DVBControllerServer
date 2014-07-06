using DVBViewerController.Models;
using DVBViewerServer;
using System;
using System.Collections.Generic;
using System.Web.Http;

namespace DVBViewerController.Controllers
{
    public class EpgController : ApiController
    {
        // GET dvb/Epg/chanId/Time/Year/Month/Day
        [Route("dvb/Epg/{channelId}")]
        public IEnumerable<EpgInfo> Get(string channelId, string time = "Current", int year = 2014, int month = 7, int day = 6)
        {
            List <EpgInfo> resp = new List<EpgInfo>();
            DVBViewer dvb;

            try
            {
                dvb = (DVBViewer)System.Runtime.InteropServices.Marshal.GetActiveObject("DVBViewerServer.DVBViewer");

                IChannelCollection col = dvb.ChannelManager;

                int channelNr = 0;
                IChannelItem channel = col.GetChannel(channelId, ref channelNr);

                IEPGManager epgManager = dvb.EPGManager;

                DateTime start = DateTime.Now;
                DateTime stop = DateTime.Now;


                switch (time)
                {
                    case "Current":
                        start = new DateTime(year, month, day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                        stop = start.AddHours(2);
                        break;
                    case "In 1 Hour":
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

                IEPGCollection epgCol = epgManager.Get(channel.Tuner.SID, channel.Tuner.TransportStreamID, start, stop);

                for (int i = 0; i < epgCol.Count; i++)
                {
                    EpgInfo epg = new EpgInfo();

                    epg.Time = epgCol[i].Time.ToShortTimeString();
                    epg.ChannelName = channel.Name;
                    epg.Title = Uri.EscapeUriString(epgCol[i].Title);
                    epg.Desc = Uri.EscapeUriString(epgCol[i].Description.Split('[')[0]);
                    epg.Duration = epgCol[i].Duration.Hour + "h " + epgCol[i].Duration.Minute + "min";

                    resp.Add(epg);
                }
            }
            catch (Exception ex)
            {
            }

            return resp;
        }
    }
}
