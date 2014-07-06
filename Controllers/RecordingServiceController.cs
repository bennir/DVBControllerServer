using DVBViewerController.Models;
using System.Web.Http;

namespace DVBViewerController.Controllers
{
    public class RecordingServiceController : ApiController
    {
        // GET dvb/RecordingService
        public RecordingService Get()
        {
            RecordingService rec = new RecordingService();
            rec.Ip = Properties.Settings.Default.recIP;
            rec.Port = Properties.Settings.Default.recPort;

            return rec;            
        }
    }
}
