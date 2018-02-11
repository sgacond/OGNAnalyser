using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OGNAnalyser.Client.Models
{
    public class AircraftBeacon : ConcreteBeacon, IGeographicPositionAndDateTime
    {
        public enum AddressTypes
        {
            Random = 0x0,
            ICAO = 0x1,
            Flarm = 0x2,
            OGN = 0x3
        }

        public enum AircraftTypes
        {
            Unknown = 0x0,
            Glider = 0x1,
            TowPlane = 0x2,
            Helicopter = 0x3,
            Skydiver = 0x4,
            DropPlane = 0x5,
            Hangglider = 0x6,
            Paraglider = 0x7,
            PoweredPiston = 0x8,
            PoweredJet = 0x9,
            Unknown2 = 0xA,
            Balloon = 0xB,
            Airship = 0xC,
            UAV = 0xD,
            Unknown3 = 0xE,
            Static = 0xF
        }

        public override BeaconType BeaconType { get { return BeaconType.Aircraft; } }

        public string AircraftOgnId { get { return BeaconSender; } }

        public string ReceiverName { get { return BeaconReceiver; } }

        public ulong AircraftId { get; set; }

        public double PositionLatDegrees { get; set; }
        public double PositionLonDegrees { get; set; }
        public int PositionAltitudeMeters { get; set; }
        public DateTime PositionTimeUTC { get; set; }

        public float RotationRateHalfTurnPerTwoMins { get; set; }
        public float ClimbRateMetersPerSecond { get; set; }
        public float SignalNoiseRatioDb { get; set; }
        public int TransmissionErrorsCorrected { get; set; }
        public float CenterFrequencyOffsetKhz { get; set; }
        public int? GpsSatellitesVisible { get; set; }
        public int? GpsSatelliteChannelsAvailable { get; set; }

        // -- id bits and mask ----------------------------------------------------------------
        // according: http://wiki.glidernet.org/wiki:subscribe-to-ogn-data
        //       and: http://www.ediatec.ch/pdf/FLARM%20Data%20Port%20Specification%20v7.00.pdf
        // idXXYYYYYY => XX encoding, YY address

        private byte addressTypeAndFlagsByte { get { return (byte) ((AircraftId & 0xFF000000) >> 24); } }

        public AddressTypes AddressType { get { return (AddressTypes)(addressTypeAndFlagsByte & 0x03); } }

        public AircraftTypes AircraftType { get { return (AircraftTypes)((addressTypeAndFlagsByte & 0x3C) >> 2); } }
        
        public bool StealthMode { get { return (addressTypeAndFlagsByte & 0x80) > 0; } }
        public bool NoTrackingFlag { get { return (addressTypeAndFlagsByte & 0x40) > 0; } }
        public uint AircraftAddress { get { return (uint) (AircraftId & 0x00FFFFFF); } }
    }
}
