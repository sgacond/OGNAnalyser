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
}
