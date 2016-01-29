using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OGNAnalyserCore.Models
{
    /// <summary>
    /// Receiver beacon. Parsed from APRS string.
    /// Sample: Cambridge>APRS,TCPIP*,qAC,GLIDERN2:/074555h5212.73NI00007.80E&/A=000066 CPU:4.0 RAM:242.7/458.8MB NTP:0.8ms/-28.6ppm +56.2C RF:+38+2.4ppm/+1.7dB
    /// </summary>
    public class ReceiverBeacon : Beacon
    {
        public override BeaconType BeaconType { get { return BeaconType.Receiver; } }

        public string ReceiverName { get { return Sender; } }

        public string RegistredNetwork { get; private set; }
        public float PositionLat { get; private set; }
        public float PositionLon { get; private set; }
        public int Altitude { get; private set; }
        public string SystemInfo { get; private set; }

        /// <summary>
        /// Parse after last comma.
        /// </summary>
        /// <param name="concretePart">sample: GLIDERN2:/074555h5212.73NI00007.80E&/A=000066 CPU:4.0 RAM:242.7/458.8MB NTP:0.8ms/-28.6ppm +56.2C RF:+38+2.4ppm/+1.7dB</param>
        protected override void parseConcretePart(string concretePart)
        {
            
        }
    }
}
