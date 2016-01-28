using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OGNAnalyserCore.Models
{
    public enum BeaconType
    {
        Aircraft,
        Receiver,
        Informational
    }

    public abstract class Beacon
    {
        public abstract BeaconType BeaconType { get; }

        internal abstract void Parse(string receivedLine);

        public DateTime ReceivedOn { get; set; }
    }
}
