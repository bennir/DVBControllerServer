
namespace DVBViewerController.Models
{
    public class Channel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Group { get; set; }

        public string ChannelId { get; set; }

        public EpgInfo Epg { get; set; }
    }
}
