using OGNAnalyser.Client.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OGNAnalyser.Core.Analysis
{
    public class AircraftBeaconSpeedAndTrack
    {
        public AircraftBeacon Beacon { get; internal set; }

        public bool Analysed { get; internal set; }
        public float GroundSpeedMs { get; internal set; }
        public float TrackDegrees { get; internal set; }

        public AircraftBeaconSpeedAndTrack(Client.Models.AircraftBeacon beacon)
        {
            Analysed = false;
            Beacon = beacon;
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
}
