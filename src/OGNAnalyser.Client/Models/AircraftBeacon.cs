using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OGNAnalyser.Client.Models
{
    public class AircraftBeacon : ConcreteBeacon, IGeographicPositionAndDateTime
    {
        public override BeaconType BeaconType { get { return BeaconType.Aircraft; } }

        public string AircraftOgnId { get { return BeaconSender; } }

        public string ReceiverName { get { return BeaconReceiver; } }

        public ulong AircraftId { get; set; }

        public double PositionLatDegrees { get; set; }
        public double PositionLonDegrees { get; set; }
        public int PositionAltitudeMeters { get; set; }
        public DateTime PositionTimeUtc { get; set; }

        public float RotationRateHalfTurnPerTwoMins { get; set; }
        public float ClimbRateMetersPerSecond { get; set; }
        public float SignalNoiseRatioDb { get; set; }
        public int TransmissionErrorsCorrected { get; set; }
        public float CenterFrequencyOffsetKhz { get; set; }
        public int GpsSatellitesVisible { get; set; }
        public int GpsSatelliteChannelsAvailable { get; set; }
    }
}
