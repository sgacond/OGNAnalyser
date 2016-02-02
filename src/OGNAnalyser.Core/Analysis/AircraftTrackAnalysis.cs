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

        private Dictionary<ulong, CircularFifoBuffer<AircraftBeacon>> aircraftBuffer = new Dictionary<ulong, CircularFifoBuffer<AircraftBeacon>>();
        private Dictionary<ulong, AircraftTrackAnalysisAircraftData> aircraftAnalysisData = new Dictionary<ulong, AircraftTrackAnalysisAircraftData>();
        private Timer bufferAnalysisTimer = new Timer(bufferAnalysisTimerIntervalMillis);

        public AircraftTrackAnalyser()
        {
            bufferAnalysisTimer.Elapsed += (s, e) =>
            {
                foreach(var key in aircraftBuffer.Keys)
                {
                    // analysis data for this track not yet addes
                    if (!aircraftAnalysisData.ContainsKey(key))
                        aircraftAnalysisData.Add(key, aircraftBuffer[key].AnalyseTrack(TimeSpan.FromSeconds(maxAircraftTrackAnalysisSecs)));

                    // analysis data for this track outdated                    
                    else if (aircraftAnalysisData[key].LastBeaconAnalysedLocalTime < aircraftBuffer[key].First().PositionLocalTime)
                        aircraftAnalysisData[key] = aircraftBuffer[key].AnalyseTrack(TimeSpan.FromSeconds(maxAircraftTrackAnalysisSecs));
                }

                Console.WriteLine("Analysis - Aircraft:");
                foreach (var kvp in aircraftAnalysisData)
                    Console.WriteLine($"\t{kvp.Key}: {kvp.Value.AnalysedBeacons} ({kvp.Value.AnalysedTimespan.TotalSeconds}) - {kvp.Value.EndSpeedMs}");
            };
            bufferAnalysisTimer.Start();
        }

        public void AddAircraftBeacon(AircraftBeacon beacon)
        {
            if (!aircraftBuffer.ContainsKey(beacon.AircraftId))
                aircraftBuffer.Add(beacon.AircraftId, new CircularFifoBuffer<AircraftBeacon>(aircraftBuffersCapacity));

            aircraftBuffer[beacon.AircraftId].Enqueue(beacon);
        }
    }
}
