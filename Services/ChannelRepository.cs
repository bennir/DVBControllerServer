using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DVBViewerController.Models;
using DVBViewerServer;
using System.Text.RegularExpressions;

namespace DVBViewerController.Services
{
    public class ChannelRepository
    {
        public IEnumerable<Channel> GetAllChannels()
        {
            List<Channel> resp = new List<Channel>();

            DVBViewer dvb;

            try
            {
                dvb = (DVBViewer)System.Runtime.InteropServices.Marshal.GetActiveObject("DVBViewerServer.DVBViewer");

                IFavoritesManager fav = dvb.FavoritesManager;
                IFavoritesCollection favcol = fav.GetFavorites();
                IEPGManager epgManager = dvb.EPGManager;

                DateTime start = DateTime.Now;
                DateTime stop = start.AddSeconds(1);
                
                for (int i = 0; i < favcol.Count; i++)
                {
                    String channelName = favcol[i].Name;
                    string pattern = "\\s\\(.+\\)";
                    Regex rgx = new Regex(pattern);
                    channelName = rgx.Replace(channelName, "");

                    Channel chan = new Channel();
                    chan.Name = channelName;
                    chan.Id = favcol[i].Nr;
                    chan.Group = favcol[i].Group;
                    chan.ChannelId = favcol[i].ChannelID;

                    try
                    {
                        IChannelCollection col = dvb.ChannelManager;

                        int channelNr = 0;
                        IChannelItem channel = col.GetChannel(favcol[i].ChannelID, ref channelNr);
                        IEPGCollection epgCol = epgManager.Get(channel.Tuner.SID, channel.Tuner.TransportStreamID, start, stop);

                        string epgTitle = epgCol[0].Title;
                        string epgTime = epgCol[0].Time.ToShortTimeString();
                        string epgDuration = epgCol[0].Duration.ToShortTimeString();

                        chan.EpgTitle = epgTitle;
                        chan.EpgTime = epgTime;
                        chan.EpgDuration = epgDuration;
                    }
                    catch (Exception ex)
                    {
                        // Can not retrieve EPG
                    }

                    resp.Add(chan);
                }

            }
            catch (Exception ex)
            {
            }

            return resp;
        }

        public Channel GetChannel(int id)
        {
            Channel resp = new Channel();

            DVBViewer dvb;

            try
            {
                dvb = (DVBViewer)System.Runtime.InteropServices.Marshal.GetActiveObject("DVBViewerServer.DVBViewer");

                IFavoritesManager fav = dvb.FavoritesManager;
                IFavoritesCollection favcol = fav.GetFavorites();
                IEPGManager epgManager = dvb.EPGManager;

                DateTime start = DateTime.Now;
                DateTime stop = start.AddSeconds(1);

                String channelName = favcol[id].Name;
                string pattern = "\\s\\(.+\\)";
                Regex rgx = new Regex(pattern);
                channelName = rgx.Replace(channelName, "");

                resp.Name = channelName;
                resp.Id = favcol[id].Nr;
                resp.Group = favcol[id].Group;
                resp.ChannelId = favcol[id].ChannelID;

                try
                {
                    IChannelCollection col = dvb.ChannelManager;

                    int channelNr = 0;
                    IChannelItem channel = col.GetChannel(favcol[id].ChannelID, ref channelNr);
                    IEPGCollection epgCol = epgManager.Get(channel.Tuner.SID, channel.Tuner.TransportStreamID, start, stop);

                    string epgTitle = epgCol[0].Title;
                    string epgTime = epgCol[0].Time.ToShortTimeString();
                    string epgDuration = epgCol[0].Duration.ToShortTimeString();

                    resp.EpgTitle = epgTitle;
                    resp.EpgTime = epgTime;
                    resp.EpgDuration = epgDuration;
                }
                catch (Exception ex)
                {
                    // Can not retrieve EPG
                }

            }
            catch (Exception ex)
            {
            }

            return resp;
        }
    }
}
