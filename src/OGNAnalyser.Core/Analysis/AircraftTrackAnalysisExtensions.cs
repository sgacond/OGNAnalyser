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
        public static void AnalyseSpeedAndTrack(this CircularFifoBuffer<AircraftBeaconSpeedAndTrack> fifoBuffer, TimeSpan maxTimeSpan)
        {
            var evalDateTime = DateTime.Now;
            DateTime deadline = evalDateTime.Subtract(maxTimeSpan);

            // get all beacons until deadline
            List<AircraftBeaconSpeedAndTrack> recalc = fifoBuffer.TakeWhile(b => !b.Analysed && b.Beacon.PositionLocalTime >= deadline).ToList();

            // if more are available, include one more (as lag beacon for first calculation).
            if (recalc.Count < fifoBuffer.Length)
                recalc.Add(fifoBuffer.Skip(recalc.Count).First());

            // reverse collection -> calc oldest first, keep lag ("older")
            recalc.Reverse();

            var lagBeacon = recalc.First();
            foreach (var beacon in recalc.Skip(1))
            {
                beacon.GroundSpeedMs = Math.Abs((float) beacon.Beacon.DistanceToGeoPositionMeters(lagBeacon.Beacon) / (float) beacon.Beacon.PositionLocalTime.Subtract(lagBeacon.Beacon.PositionLocalTime).TotalSeconds);
                lagBeacon = beacon;
            }
        }
    }
}
