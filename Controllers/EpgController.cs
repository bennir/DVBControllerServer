using DVBViewerController.Models;
using DVBViewerServer;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web.Http;

namespace DVBViewerController.Controllers
{
    public class EpgController : ApiController
    {
        // GET dvb/Epg/chanId/Time/Year/Month/Day
        [Route("dvb/Epg/{channelId}")]
        public IEnumerable<EpgInfo> Get(string channelId, string time = "Current", int year = 0, int month = 0, int day = 0, int hour = 0, int minute = 0, int second = 0)
        {
            List <EpgInfo> resp = new List<EpgInfo>();
            DVBViewer dvb;

            try
            {
                dvb = (DVBViewer)System.Runtime.InteropServices.Marshal.GetActiveObject("DVBViewerServer.DVBViewer");

                IChannelCollection col = dvb.ChannelManager;

                int channelNr = 0;
                channelId = Uri.UnescapeDataString(channelId);
                IChannelItem channel = col.GetChannel(channelId, ref channelNr);

                IEPGManager epgManager = dvb.EPGManager;

                DateTime start = DateTime.Now;
                DateTime stop = DateTime.Now;


                if(time == "Current") 
                {
                    start = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                    stop = start.AddHours(10);
                }
                else
                {
                    long binaryTime = Convert.ToInt64(time);

                    start = DateTime.FromBinary(binaryTime);
                    start = start.AddMinutes(1);

                    stop = start.AddHours(10);
                }

                IEPGCollection epgCol = epgManager.Get(channel.Tuner.SID, channel.Tuner.TransportStreamID, start, stop);

                for (int i = 0; i < epgCol.Count; i++)
                {
                    EpgInfo epg = new EpgInfo();
                    epg.Time = epgCol[i].Time.ToShortTimeString();
                    epg.EndTime = epgCol[i].EndTime.ToBinary();
                    epg.ChannelName = channel.ChannelID;
                    epg.Title = epgCol[i].Title;
                    epg.Desc = epgCol[i].Description.Split('[')[0];

                    epg.Duration = epgCol[0].Duration.ToShortTimeString();
                    epg.Date = epgCol[i].Time.ToShortDateString();

                    string pattern = "<.+>";
                    Regex rgx = new Regex(pattern);
                    epg.Title = rgx.Replace(epg.Title, "");
                    epg.Desc = rgx.Replace(epg.Desc, "");

                    epg.Title = Uri.EscapeUriString(epg.Title);
                    epg.Desc = Uri.EscapeUriString(epg.Desc);

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
