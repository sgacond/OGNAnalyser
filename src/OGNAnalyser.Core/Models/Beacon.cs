using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OGNAnalyser.Core.Models
{
    public enum BeaconType
    {
        Aircraft,
        Receiver,
        Informational
    }

    /// <summary>
    /// Abstract APRS Beacon message. Use static ParseBeacon to parse a APRS Message.
    /// </summary>
    public abstract class Beacon
    {
        public abstract BeaconType BeaconType { get; }

        public DateTimeOffset ParsedDateTime { get; internal set; }

        public string BeaconSender { get; internal set; }

        public string BeaconReceiver { get; internal set; }
    }
}
