using OGNAnalyser.Client.Models;
using OGNAnalyser.Core.Analysis;
using System;
using System.Collections.Generic;

namespace OGNAnalyser.Core
{
    public interface IOGNAnalyser : IDisposable
    {
        event Action<IDictionary<uint, AircraftBeaconSpeedAndTrack>> AnalysisIntervalElapsed;
        event Action<string, AircraftTrackEvent> EventDetected;
        void SubscribeAirfieldForMovementEvents(string airfieldKey, IGeographicPosition airfieldPosition, Action<AircraftTrackEvent> eventDetectedCallback = null);
        void Run();
    }
}