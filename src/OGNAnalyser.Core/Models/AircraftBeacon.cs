using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OGNAnalyser.Core.Models
{
    public class AircraftBeacon : ConcreteBeacon, IGeographicPositionAndDateTime
    {
        public override BeaconType BeaconType { get { return BeaconType.Aircraft; } }

        public string AircraftId { get { return BeaconSender; } }
        public string ReceiverName { get { return BeaconReceiver; } }

        public float PositionLatDegrees { get; set; }
        public float PositionLonDegrees { get; set; }
        public int PositionAltitudeMeters { get; set; }
        public DateTime PositionLocalTime { get; set; }
    }
}
