using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OGNAnalyser.Client.Models
{
    /// <summary>
    /// Abstract APRS Beacon message. Use static ParseBeacon to parse a APRS Message.
    /// </summary>
    public abstract class ConcreteBeacon : Beacon, IBaseAPRSBeacon
    {
        public string BeaconSender { get; set; }

        public string BeaconReceiver { get; set; }
    }

    public abstract class Beacon
    {
        public DateTimeOffset ParsedDateTime { get; set; }
        public abstract BeaconType BeaconType { get; }
    }
}
