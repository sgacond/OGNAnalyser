using OGNAnalyser.Client.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OGNAnalyser.Core.Analysis
{
    public class ExtendedAircraftBeacon
    {
        public AircraftBeacon Beacon { get; internal set; }

        public float GroundSpeedMs { get; internal set; }

        public ExtendedAircraftBeacon(AircraftBeacon beacon)
        {
            this.Beacon = beacon;
        }
    }

    public class AircraftTrackAnalysisAircraftData
    {
        public TimeSpan AnalysedTimespan { get; internal set; }
        public DateTime LastBeaconAnalysedLocalTime { get; internal set; }
        public float StartSpeedMs { get; internal set; }
        public float EndSpeedMs { get; internal set; }

        public IEnumerable<ExtendedAircraftBeacon> AnalysedBeacons { get; internal set; }
    }
}
