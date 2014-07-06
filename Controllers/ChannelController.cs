using DVBViewerController.Models;
using DVBViewerController.Services;
using DVBViewerServer;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace DVBViewerController.Controller
{
    public class ChannelController : ApiController
    {
        private ChannelRepository channelRepository;

        public ChannelController()
        {
            this.channelRepository = new ChannelRepository();
        }


        // GET dvb/Channel
        public IEnumerable<Channel> Get()
        {
            return channelRepository.GetAllChannels();
        }

        // GET dvb/Channel/5 
        [Route("dvb/Channel/{id:int}")] 
        public Channel Get(int id)
        {
            return channelRepository.GetChannel(id);
        }

        // GET dvb/Channel/current
        [Route("dvb/Channel/current")]
        public Channel GetCurrent()
        {
            return channelRepository.GetCurrentChannel();
        }

        // GET dvb/Channel/current/name
        [Route("dvb/Channel/current/name")]
        public string GetCurrentName()
        {
            return channelRepository.GetCurrentChannel().Name;
        }

        // POST dvb/Channel/set/1
        [Route("dvb/Channel/set/{channelId}")]
        [HttpPost]
        public HttpResponseMessage sendMenu(string channelId)
        {
            DVBViewer dvb;

            try
            {
                dvb = (DVBViewer)System.Runtime.InteropServices.Marshal.GetActiveObject("DVBViewerServer.DVBViewer");

                foreach (char x in channelId)
                {
                    int cmd = Int16.Parse(x.ToString());
                    dvb.SendCommand(DVBServer.FavNumbers[cmd]);
                }

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }


        }
    }
}
