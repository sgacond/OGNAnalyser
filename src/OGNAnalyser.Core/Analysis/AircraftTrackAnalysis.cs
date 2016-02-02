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
        private const int maxAircraftTrackAnalysisSecs = 60;
        private const double bufferAnalysisTimerIntervalMillis = 2000;

        private Dictionary<ulong, CircularFifoBuffer<AircraftBeaconSpeedAndTrack>> aircraftBuffer = new Dictionary<ulong, CircularFifoBuffer<AircraftBeaconSpeedAndTrack>>();
        private Timer bufferAnalysisTimer = new Timer(bufferAnalysisTimerIntervalMillis);

        public AircraftTrackAnalyser()
        {
            bufferAnalysisTimer.Elapsed += (s, e) =>
            {
                foreach(var acft in aircraftBuffer.Values)
                {
                    if (acft.First().Analysed)
                        continue;

                    acft.AnalyseSpeedAndTrack(TimeSpan.FromSeconds(maxAircraftTrackAnalysisSecs));
                }

                Console.WriteLine("Analysis - Aircraft:");
                foreach (var row in aircraftBuffer.Select(b => new { id = b.Key, lastSpeed = b.Value.First().GroundSpeedMs, lastTrack = b.Value.First().TrackDegrees }))
                    Console.WriteLine($"\t{row.id}: {row.lastSpeed} - {row.lastTrack}");
            };
            bufferAnalysisTimer.Start();
        }

        public void AddAircraftBeacon(AircraftBeacon beacon)
        {
            if (!aircraftBuffer.ContainsKey(beacon.AircraftId))
                aircraftBuffer.Add(beacon.AircraftId, new CircularFifoBuffer<AircraftBeaconSpeedAndTrack>(aircraftBuffersCapacity));

            aircraftBuffer[beacon.AircraftId].Enqueue(new AircraftBeaconSpeedAndTrack(beacon));
        }
    }
}
