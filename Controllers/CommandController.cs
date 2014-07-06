using DVBViewerServer;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace DVBViewerController.Controllers
{
    public class CommandController : ApiController
    {
        // POST dvb/Command/111
        [Route("dvb/Command/{id:int}")]        
        [HttpPost]
        public HttpResponseMessage sendMenu(int command)
        {
            DVBViewer dvb;

            try
            {
                dvb = (DVBViewer)System.Runtime.InteropServices.Marshal.GetActiveObject("DVBViewerServer.DVBViewer");

                dvb.SendCommand(command);

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);                
            }
        }
    }
}
