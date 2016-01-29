using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.Framework.DependencyInjection;
using NLog;
using System.Net.Sockets;
using OGNAnalyserCore.Comm;

namespace OGNAnalyserCore
{
    public class OGNAnalyser : IDisposable
    {
        APRSComm comm;

        public OGNAnalyser()
        {
            comm = new APRSComm("glidern1.glidernet.org", 14580, "sgtest", 47.170869f, 9.039742f, 150);
        }

        public void Dispose()
        {
            if (comm != null)
                comm.Dispose();
        }
    }
}
