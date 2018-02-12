using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using OGNAnalyser.Client;
using OGNAnalyser.Core.Analysis;
using Microsoft.Extensions.Logging.Abstractions;
using OGNAnalyser.Client.Models;

namespace OGNAnalyser.Core
{
    internal class OGNAnalyser : IOGNAnalyser
    {
        private ILogger _log;
        private OGNAnalyserSettings _settings;
        private IAPRSClient _client;
        private AircraftTrackAnalyser _analyser;

        public event Action<IDictionary<uint, AircraftBeaconSpeedAndTrack>> AnalysisIntervalElapsed
        {
            add { _analyser.AnalysisIntervalElapsed += value; }
            remove { _analyser.AnalysisIntervalElapsed -= value; }
        }

        public event Action<string, AircraftTrackEvent> EventDetected
        {
            add { _analyser.EventDetected += value; }
            remove { _analyser.EventDetected -= value; }
        }

        public OGNAnalyser(IAPRSClient client, OGNAnalyserSettings settings, ILogger log)
        {
            _log = log ?? NullLogger.Instance;
            _client = client;
            _analyser = new AircraftTrackAnalyser();
            _settings = settings;
        }

        public void Run()
        {
            _client.Run();

            // push to analyser
            _client.AircraftBeaconReceived += b => _analyser.AddAircraftBeacon(b);
            
            // loggersa akjsdh askjh kasjdhaksjd 
            _client.ReceiverBeaconReceived += b => _log.LogTrace("Receiver: {0}, {1}, {2}, {3}, {4}, {5}", b.BeaconSender, b.PositionTimeUTC, b.PositionLatDegrees, b.PositionLonDegrees, b.PositionAltitudeMeters, b.SystemInfo);
            _client.AircraftBeaconReceived += b => _log.LogTrace("AIRCRAFT: {0}, {1}, {2}, {3}, {4}, {5}, {6}", b.AircraftId, b.PositionTimeUTC, b.PositionLatDegrees, b.PositionLonDegrees, b.PositionAltitudeMeters, b.ClimbRateMetersPerSecond, b.RotationRateHalfTurnPerTwoMins);

            _log.LogInformation("OGN Analyser started.");
        }

        public void Dispose()
        {
            if (_client != null)
                _client.Dispose();

            if (_analyser != null)
                _analyser.Dispose();

            _log.LogInformation("OGN Analyser stopped.");
        }

        public void SubscribeAirfieldForMovementEvents(string airfieldKey, IGeographicPosition airfieldPosition, Action<AircraftTrackEvent> eventDetectedCallback = null)
            => _analyser.SubscribeAirfieldForMovmentEvents(airfieldKey, airfieldPosition, eventDetectedCallback);
    }
}
