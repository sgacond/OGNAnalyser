using OGNAnalyser.Client.Models;
using OGNAnalyser.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OGNAnalyser.Core.Analysis
{
    public static class AircraftTrackAnalysisExtensions
    {
        public static AircraftTrackAnalysisAircraftData AnalyseTrack(this CircularFifoBuffer<AircraftBeacon> fifoBuffer, TimeSpan maxTimeSpan)
        {
            var evalDateTime = DateTime.Now;
            DateTime deadline = evalDateTime.Subtract(maxTimeSpan);
            
            var result = new AircraftTrackAnalysisAircraftData();

            result.AnalysedBeacons = fifoBuffer.TakeWhile(b => b.PositionLocalTime >= deadline).Select(b => new ExtendedAircraftBeacon(b)).ToList();
            result.AnalysedTimespan = evalDateTime.Subtract(result.AnalysedBeacons.Last().Beacon.PositionLocalTime);

            var lagBeacon = result.AnalysedBeacons.First();
            bool fistCaught = false;

            foreach (var beacon in result.AnalysedBeacons.Skip(1))
            {
                beacon.GroundSpeedMs = Math.Abs((float) beacon.Beacon.DistanceToGeoPositionMeters(lagBeacon.Beacon) / (float) beacon.Beacon.PositionLocalTime.Subtract(lagBeacon.Beacon.PositionLocalTime).TotalSeconds);

                if (!fistCaught)
                {
                    result.EndSpeedMs = beacon.GroundSpeedMs;
                    fistCaught = true;
                }

                lagBeacon = beacon;
            }

            result.StartSpeedMs = lagBeacon.GroundSpeedMs;
            
            return result;
        }
    }
}
