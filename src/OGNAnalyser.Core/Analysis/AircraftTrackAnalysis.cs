using OGNAnalyser.Client.Models;
using OGNAnalyser.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace OGNAnalyser.Core.Analysis
{
    public class AircraftTrackAnalyser
    {
        private const int aircraftBuffersCapacity = 60;
        private const int maxAircraftTrackAnalysisSecs = 180;
        private const double bufferAnalysisTimerIntervalMillis = 2000;

        private Dictionary<uint, CircularFifoBuffer<AircraftBeaconSpeedAndTrack>> aircraftBuffer = new Dictionary<uint, CircularFifoBuffer<AircraftBeaconSpeedAndTrack>>();
        private Timer bufferAnalysisTimer = new Timer(bufferAnalysisTimerIntervalMillis);

        public AircraftTrackAnalyser()
        {
            bufferAnalysisTimer.Elapsed += (s, e) =>
            {
                foreach(var acft in aircraftBuffer.Values)
                {
                    if (acft.First().Analysed)
                        continue;

                    acft.AnalyseSpeedAndTrack(DateTime.Now.ToUniversalTime(), TimeSpan.FromSeconds(maxAircraftTrackAnalysisSecs));
                }

                Console.Clear();
                Console.WriteLine("Analysis - Aircraft:");
                var nowUtc = DateTime.Now.ToUniversalTime();
                foreach (var row in aircraftBuffer.Select(b => new { id = b.Key, type = b.Value.First().Beacon.AircraftType, lastSpeed = b.Value.First().GroundSpeedMs, lastTrack = b.Value.First().TrackDegrees, lastBeaconSecsAgo = nowUtc.Subtract(b.Value.First().Beacon.PositionTimeUTC).TotalSeconds }))
                    Console.WriteLine($"\t{row.id:X} {row.type}\t: ({Math.Round(row.lastBeaconSecsAgo, 1)}s ago) {row.lastSpeed}ms ({Math.Round(row.lastSpeed*3.6f, 1)}km/h) - {row.lastTrack}°");
            };
            bufferAnalysisTimer.Start();
        }

        public void AddAircraftBeacon(AircraftBeacon beacon)
        {
            if (!aircraftBuffer.ContainsKey(beacon.AircraftAddress))
                aircraftBuffer.Add(beacon.AircraftAddress, new CircularFifoBuffer<AircraftBeaconSpeedAndTrack>(aircraftBuffersCapacity));

            aircraftBuffer[beacon.AircraftAddress].Enqueue(new AircraftBeaconSpeedAndTrack(beacon));
        }
    }
}
