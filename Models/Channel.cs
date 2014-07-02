using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVBViewerController.Models
{
    public class Channel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Group { get; set; }

        public string ChannelId { get; set; }

        public string EpgTitle { get; set; }

        public string EpgTime { get; set; }

        public string EpgDuration { get; set; }
    }
}
