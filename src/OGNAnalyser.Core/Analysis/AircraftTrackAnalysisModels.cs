using OGNAnalyser.Client.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OGNAnalyser.Core.Analysis
{
    public class AircraftBeaconSpeedAndTrack : AircraftBeacon
    {
        public bool Analysed { get; internal set; }
        public float GroundSpeedMs { get; internal set; }
        public float TrackDegrees { get; internal set; }

        public AircraftBeaconSpeedAndTrack(AircraftBeacon beacon)
        {
            Analysed = false;
            AircraftId = beacon.AircraftId;
            PositionLatDegrees = beacon.PositionLatDegrees;
            PositionLonDegrees = beacon.PositionLonDegrees;
            PositionAltitudeMeters = beacon.PositionAltitudeMeters;
            PositionTimeUTC = beacon.PositionTimeUTC;
            RotationRateHalfTurnPerTwoMins = beacon.RotationRateHalfTurnPerTwoMins;
            ClimbRateMetersPerSecond = beacon.ClimbRateMetersPerSecond;
            SignalNoiseRatioDb = beacon.SignalNoiseRatioDb;
            TransmissionErrorsCorrected = beacon.TransmissionErrorsCorrected;
            CenterFrequencyOffsetKhz = beacon.CenterFrequencyOffsetKhz;
            GpsSatellitesVisible = beacon.GpsSatellitesVisible;
            GpsSatelliteChannelsAvailable = beacon.GpsSatelliteChannelsAvailable;
        }
    }

    public enum AircraftTrackEventTypes
    {
        TakeOff,
        Landing,
        TowRelease
    }

    public class AircraftTrackEvent : IComparable<AircraftTrackEvent>
    {
        public AircraftTrackEventTypes EventType { get; internal set; }

        public DateTime EventDateTimeUTC { get; internal set; }
        
        public AircraftBeaconSpeedAndTrack ReferenceBeacon { get; internal set; }

        public int CompareTo(AircraftTrackEvent other)
        {
            var diffSecs = other.EventDateTimeUTC.Subtract(EventDateTimeUTC).TotalSeconds;

            if (other.EventType == EventType && Math.Abs(diffSecs) <= 3)
                return 0;

            if(diffSecs != 0d)
                return Math.Sign(diffSecs);

            return -1;
        }
    }
    
    public class AircraftTrackEventComparer : IEqualityComparer<AircraftTrackEvent>
    {
        public bool Equals(AircraftTrackEvent x, AircraftTrackEvent y)
        {
            return x.CompareTo(y) == 0;
        }

        public int GetHashCode(AircraftTrackEvent obj)
        {
            return obj.EventDateTimeUTC.GetHashCode() & obj.EventType.GetHashCode();
        }
    }

    internal class AirfieldSubscription
    {
        public string AirfieldKey { get; internal set; }
        public IGeographicPosition AirfieldPosition { get; internal set; }
        public List<AircraftTrackEvent> Events { get; internal set; }
        internal event Action<AircraftTrackEvent> EventDetected;

        internal void FireEventDetected(AircraftTrackEvent evt)
        {
            if (EventDetected != null)
                EventDetected(evt);
        }
    }
}
