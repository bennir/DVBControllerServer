using DVBViewerController.Services;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;

namespace DVBViewerController.Controllers
{
    public class LogoController : ApiController
    {
        private LogoRepository logoRepository;

        public LogoController()
        {
            this.logoRepository = new LogoRepository();
        }

        // GET dvb/Logo/ZDF%20HD
        [Route("dvb/Logo/{channelName}")]
        public HttpResponseMessage Get(string channelName)
        {
            HttpResponseMessage response = new HttpResponseMessage();

            MemoryStream ms = logoRepository.GetImage(channelName);

            if (ms != null) {
                response.Content = new StreamContent(ms); // this file stream will be closed by lower layers of web api for you once the response is completed.
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                response.Content.Headers.ContentLength = ms.Length;

                return response;
            }
            else
            {
                response.StatusCode = System.Net.HttpStatusCode.NotFound;

                return response;
            }
        }
    }
}
