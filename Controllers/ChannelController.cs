using DVBViewerController.Models;
using DVBViewerController.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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


        // GET dvb/channels 
        public IEnumerable<Channel> Get()
        {
            return channelRepository.GetAllChannels();
        }

        // GET dvb/channels/5 
        public Channel Get(int id)
        {
            return channelRepository.GetChannel(id);
        }
    }
}
