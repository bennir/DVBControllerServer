using DVBViewerController.Models;
using DVBViewerServer;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace DVBViewerController.Controllers
{
    public class CommandController : ApiController
    {
        // POST dvb/Command
        public HttpResponseMessage Post([FromBody] DVBCommand cmd)
        {
            DVBViewer dvb;

            try
            {
                dvb = (DVBViewer)System.Runtime.InteropServices.Marshal.GetActiveObject("DVBViewerServer.DVBViewer");

                dvb.SendCommand(cmd.Command);

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }
        }
    }
}
