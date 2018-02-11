using OGNAnalyser.Client.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OGNAnalyser.Client.Models
{
    /// <summary>
    /// Receiver beacon. Parsed from APRS string.
    /// Sample: Cambridge>APRS,TCPIP*,qAC,GLIDERN2:/074555h5212.73NI00007.80E&/A=000066 CPU:4.0 RAM:242.7/458.8MB NTP:0.8ms/-28.6ppm +56.2C RF:+38+2.4ppm/+1.7dB
    /// </summary>
    public class ReceiverBeacon : ConcreteBeacon, IGeographicPositionAndDateTime
    {
        public override BeaconType BeaconType { get { return BeaconType.Receiver; } }

        public string ReceiverName { get { return BeaconSender; } }

        public double PositionLatDegrees { get; set; }
        public double PositionLonDegrees { get; set; }
        public DateTime PositionTimeUTC { get; set; }
        public int PositionAltitudeMeters { get; set; }
        public string SystemInfo { get; internal set; }
    }
}
