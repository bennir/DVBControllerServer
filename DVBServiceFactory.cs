using Griffin.Networking.Servers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace DVBViewerController
{
    class DVBServiceFactory : IServiceFactory
    {
        #region IServiceFactory Members

        DVBServer dvb;

        public DVBServiceFactory(DVBServer dvb)
        {
            this.dvb = dvb;
        }

        /// <summary>
        /// Create a new client
        /// </summary>
        /// <param name="remoteEndPoint">IP address of the remote end point</param>
        /// <returns>Created client</returns>
        public INetworkService CreateClient(EndPoint remoteEndPoint)
        {
            return new DVBService(dvb);
        }

        #endregion
    }
}
