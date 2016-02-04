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
        private Dictionary<string, AirfieldSubscription> airfieldSubscriptions = new Dictionary<string, AirfieldSubscription>();

        public event Action<string, AircraftTrackEvent> EventDetected;

        public AircraftTrackAnalyser()
        {
            bufferAnalysisTimer.Elapsed += analysisTimerElapsed;
            bufferAnalysisTimer.Start();
        }

        public void AddAircraftBeacon(AircraftBeacon beacon)
        {
            if (!aircraftBuffer.ContainsKey(beacon.AircraftAddress))
                aircraftBuffer.Add(beacon.AircraftAddress, new CircularFifoBuffer<AircraftBeaconSpeedAndTrack>(aircraftBuffersCapacity));

            aircraftBuffer[beacon.AircraftAddress].Enqueue(new AircraftBeaconSpeedAndTrack(beacon));
        }

        public IEnumerable<AircraftBeaconSpeedAndTrack> GetAircraftAnalysedPath(uint aircraftId)
        {
            return aircraftBuffer[aircraftId].SkipWhile(a => !a.Analysed);
        }

        public void SubscribeAirfieldForMovmentEvents(string airfieldKey, IGeographicPosition airfieldPosition, Action<AircraftTrackEvent> eventDetectedCallback = null)
        {
            if(airfieldSubscriptions.ContainsKey(airfieldKey))
            {
                if(eventDetectedCallback != null)
                    airfieldSubscriptions[airfieldKey].EventDetected += eventDetectedCallback;

                return;
            }

            var subscription = new AirfieldSubscription { AirfieldKey = airfieldKey, AirfieldPosition = airfieldPosition };

            if (eventDetectedCallback != null)
                subscription.EventDetected += eventDetectedCallback;

            airfieldSubscriptions.Add(airfieldKey, subscription);
        }

        private void analysisTimerElapsed(object sender, ElapsedEventArgs args)
        {
            foreach (var acft in aircraftBuffer.Values)
            {
                if (acft.First().Analysed)
                    continue;

                var evalDt = DateTime.Now.ToUniversalTime();

                acft.AnalyseSpeedAndTrack(evalDt, TimeSpan.FromSeconds(maxAircraftTrackAnalysisSecs));

                foreach(var airfieldSubscription in airfieldSubscriptions.Values)
                {
                    var newEvents = acft.DetectTrackEvents(evalDt, TimeSpan.FromSeconds(maxAircraftTrackAnalysisSecs), airfieldSubscription.AirfieldPosition);

                    foreach (var newEvent in newEvents)
                    {
                        if (airfieldSubscription.Events.Contains(newEvent, new AircraftTrackEventComparer()))
                            continue;

                        airfieldSubscription.Events.Add(newEvent);
                        airfieldSubscription.FireEventDetected(newEvent);

                        if (EventDetected != null)
                            EventDetected(airfieldSubscription.AirfieldKey, newEvent);
                    }
                }
            }

            // TODO: REMOVE - just for debugging visualsation. maybe introduce an event?
            Console.Clear();
            Console.WriteLine("Analysis - Aircraft:");
            var nowUtc = DateTime.Now.ToUniversalTime();
            foreach (var row in aircraftBuffer.Select(b => new { id = b.Key, type = b.Value.First().AircraftType, lastSpeed = b.Value.First().GroundSpeedMs, lastTrack = b.Value.First().TrackDegrees, lastBeaconSecsAgo = nowUtc.Subtract(b.Value.First().PositionTimeUTC).TotalSeconds }))
                Console.WriteLine($"\t{row.id:X} {row.type}\t: ({Math.Round(row.lastBeaconSecsAgo, 1)}s ago) {row.lastSpeed}ms ({Math.Round(row.lastSpeed * 3.6f, 1)}km/h) - {row.lastTrack}°");
        }
    }
}
