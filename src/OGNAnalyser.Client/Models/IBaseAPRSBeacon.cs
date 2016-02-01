using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OGNAnalyser.Client.Models
{
    /// <summary>
    /// Represents first part content - like FLRDF0A52>APRS,qAS,LSTB:
    /// </summary>
    internal interface IBaseAPRSBeacon
    {
        BeaconType BeaconType { get; }

        DateTimeOffset ParsedDateTime { get; set; }

        string BeaconSender { get; set; }

        string BeaconReceiver { get; set; }
    }
}
