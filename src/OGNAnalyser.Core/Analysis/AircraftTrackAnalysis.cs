using OGNAnalyser.Client.Models;
using OGNAnalyser.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace OGNAnalyser.Core.Analysis
{
    /// <summary>
    /// Analysis of aircraft tracks. Runs an internal analysis timer to get the latest unanalysed beacons and adds
    /// track and speed information.
    /// </summary>
    public class AircraftTrackAnalyser : IDisposable
    {
        private const int aircraftBuffersCapacity = 60;
        private const int maxAircraftTrackAnalysisSecs = 180;
        private const double bufferAnalysisTimerIntervalMillis = 2000;

        private Dictionary<uint, CircularFifoBuffer<AircraftBeaconSpeedAndTrack>> aircraftBuffer = new Dictionary<uint, CircularFifoBuffer<AircraftBeaconSpeedAndTrack>>();
        private Timer bufferAnalysisTimer = new Timer(bufferAnalysisTimerIntervalMillis);
        private Dictionary<string, AirfieldSubscription> airfieldSubscriptions = new Dictionary<string, AirfieldSubscription>();

        public event Action<IDictionary<uint, AircraftBeaconSpeedAndTrack>> AnalysisIntervalElapsed;
        public event Action<string, AircraftTrackEvent> EventDetected;
        
        /// <summary>
        /// Initializes and starts the analysis interval.
        /// </summary>
        public AircraftTrackAnalyser()
        {
            bufferAnalysisTimer.Elapsed += analysisTimerElapsed;
            bufferAnalysisTimer.Start();
        }

        /// <summary>
        /// Call when new aircraft beacon detected.
        /// </summary>
        /// <param name="beacon">detected beacon</param>
        public void AddAircraftBeacon(AircraftBeacon beacon)
        {
            if (!aircraftBuffer.ContainsKey(beacon.AircraftAddress))
                aircraftBuffer.Add(beacon.AircraftAddress, new CircularFifoBuffer<AircraftBeaconSpeedAndTrack>(aircraftBuffersCapacity));

            aircraftBuffer[beacon.AircraftAddress].Enqueue(new AircraftBeaconSpeedAndTrack(beacon));
        }

        /// <summary>
        /// Returns already analysed path for further analysis.
        /// </summary>
        /// <param name="aircraftId">parsed aircraft id to identify beacons</param>
        /// <returns></returns>
        public IEnumerable<AircraftBeaconSpeedAndTrack> GetAircraftAnalysedPath(uint aircraftId)
        {
            return aircraftBuffer[aircraftId].SkipWhile(a => !a.Analysed);
        }

        /// <summary>
        /// Subscribe an observation point (lat / lon / elevation) and attach a callback for detected events.
        /// </summary>
        /// <param name="airfieldKey">a definable key</param>
        /// <param name="airfieldPosition">position and elevation of the observation point (usually an airfield)</param>
        /// <param name="eventDetectedCallback">callback on event detected</param>
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

            if (AnalysisIntervalElapsed != null)
                AnalysisIntervalElapsed(aircraftBuffer.ToDictionary(ab => ab.Key, ab => ab.Value.First()));
        }
        /// <summary>
        /// Disposes the analysis timer.
        /// </summary>
        public void Dispose()
        {
            if(bufferAnalysisTimer != null)
                bufferAnalysisTimer.Dispose();
        }
    }
}
