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
        // constants predicates for track analysis
        private const double accuracyCheck4BeaconsMaxTimeSpanSeconds = 120;
        private const double movingSpeedThresholdMs = 10d;
        private const double onGroundAltitudeThresholdMeters = 60;
        private const double airportRangeRadiusMeters = 2000d;
        private static bool isMoving(this AircraftBeaconSpeedAndTrack beacon) => beacon.GroundSpeedMs > movingSpeedThresholdMs;
        private static bool isStoppedOnGround(this AircraftBeaconSpeedAndTrack beacon, int groundAltMeters) => beacon.GroundSpeedMs <= movingSpeedThresholdMs && Math.Abs(beacon.PositionAltitudeMeters - groundAltMeters) < onGroundAltitudeThresholdMeters;
        private static bool isWithinRefPositionRange(this AircraftBeaconSpeedAndTrack beacon, IGeographicPosition refPos) => beacon.DistanceToGeoPositionMeters(refPos) <= airportRangeRadiusMeters;

        public static void AnalyseSpeedAndTrack(this CircularFifoBuffer<AircraftBeaconSpeedAndTrack> fifoBuffer, DateTime evalDateTimeUtc, TimeSpan maxTimeSpan)
        {
            List<AircraftBeaconSpeedAndTrack> recalc = fifoBuffer.extractAndReverseBufferForEvalWindow(evalDateTimeUtc, maxTimeSpan);

            var lagBeacon = recalc.First();
            foreach (var beacon in recalc.Skip(1))
            {
                var dTime = beacon.PositionTimeUTC.Subtract(lagBeacon.PositionTimeUTC).TotalSeconds;
                if (dTime <= 0)
                    continue;

                beacon.GroundSpeedMs = (float)Math.Round(beacon.DistanceToGeoPositionMeters(lagBeacon) / dTime, 2);
                beacon.TrackDegrees = lagBeacon.InitialBearingToGeoPositionDegrees(beacon);
                lagBeacon = beacon;
            }
        }

        public static IEnumerable<AircraftTrackEvent> DetectTrackEvents(this CircularFifoBuffer<AircraftBeaconSpeedAndTrack> fifoBuffer, DateTime evalDateTimeUtc, TimeSpan maxTimeSpan, IGeographicPosition eventTargetCenter)
        {
            var events = new List<AircraftTrackEvent>();

            List<AircraftBeaconSpeedAndTrack> evalWindow = fifoBuffer.extractAndReverseBufferForEvalWindow(evalDateTimeUtc, maxTimeSpan, getAlreadyAnalysed: true);

            // we need 4 beacons to detect - two one state - two other state
            if (evalWindow.Count < 4)
                return events;

            int i = 0;
            while(++i <= evalWindow.Count - 4)
            {
                var subWindow = evalWindow.Skip(i).Take(4).ToArray();

                // accuracy check
                if (subWindow[3].PositionTimeUTC.Subtract(subWindow[0].PositionTimeUTC).TotalSeconds > accuracyCheck4BeaconsMaxTimeSpanSeconds)
                    continue;
                
                bool landing = subWindow[0].isMoving()
                            && subWindow[1].isMoving()
                            && subWindow[2].isStoppedOnGround(eventTargetCenter.PositionAltitudeMeters)
                            && subWindow[2].isWithinRefPositionRange(eventTargetCenter)
                            && subWindow[3].isStoppedOnGround(eventTargetCenter.PositionAltitudeMeters)
                            && subWindow[3].isWithinRefPositionRange(eventTargetCenter);

                if (landing)
                    events.Add(new AircraftTrackEvent { EventType = AircraftTrackEventTypes.Landing, EventDateTimeUTC = subWindow[2].PositionTimeUTC, ReferenceBeacon = subWindow[2] });

                bool takeOff = subWindow[0].isStoppedOnGround(eventTargetCenter.PositionAltitudeMeters)
                            && subWindow[0].isWithinRefPositionRange(eventTargetCenter)
                            && subWindow[1].isStoppedOnGround(eventTargetCenter.PositionAltitudeMeters)
                            && subWindow[1].isWithinRefPositionRange(eventTargetCenter)
                            && subWindow[2].isMoving()
                            && subWindow[3].isMoving();

                if (takeOff)
                    events.Add(new AircraftTrackEvent { EventType = AircraftTrackEventTypes.TakeOff, EventDateTimeUTC = subWindow[1].PositionTimeUTC, ReferenceBeacon = subWindow[2] });
            }

            return events.Distinct(new AircraftTrackEventComparer());
        }

        private static List<AircraftBeaconSpeedAndTrack> extractAndReverseBufferForEvalWindow(this CircularFifoBuffer<AircraftBeaconSpeedAndTrack> fifoBuffer, DateTime evalDateTimeUtc, TimeSpan maxTimeSpan, bool getAlreadyAnalysed = false)
        {
            DateTime deadline = evalDateTimeUtc.Subtract(maxTimeSpan);

            // get all beacons until deadline
            List<AircraftBeaconSpeedAndTrack> recalc = fifoBuffer.TakeWhile(b => (!b.Analysed || getAlreadyAnalysed) && b.PositionTimeUTC >= deadline).ToList();

            // if more are available, include one more (as lag beacon for first calculation).
            if (recalc.Count < fifoBuffer.Length)
                recalc.Add(fifoBuffer.Skip(recalc.Count).First());

            // reverse collection -> calc oldest first, keep lag ("older")
            recalc.Reverse();

            return recalc;
        }
    }
}
