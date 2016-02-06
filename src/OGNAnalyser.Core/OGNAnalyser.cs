using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using OGNAnalyser.Client;
using OGNAnalyser.Core.Analysis;

namespace OGNAnalyser.Core
{
    public class OGNAnalyser : IDisposable
    {
        private ILogger log;
        private OGNClientSettings settings;
        private APRSClient client;
        private AircraftTrackAnalyser analyser;

        public event Action<IDictionary<uint, AircraftBeaconSpeedAndTrack>> AnalysisIntervalElapsed
        {
            add { analyser.AnalysisIntervalElapsed += value; }
            remove { analyser.AnalysisIntervalElapsed -= value; }
        }

        public event Action<string, AircraftTrackEvent> EventDetected
        {
            add { analyser.EventDetected += value; }
            remove { analyser.EventDetected -= value; }
        }

        public OGNAnalyser(APRSClient client, AircraftTrackAnalyser analyser, OGNClientSettings settings, ILoggerFactory logFactory)
        {
            log = logFactory.CreateLogger<OGNAnalyser>();
            this.client = client;
            this.analyser = analyser;
            this.settings = settings;
        }

        public void Run()
        {
            // load from config provider
            client.Configure(settings.OgnServer, settings.OgnPort, settings.OgnUsername, settings.Filter.CenterLatDegrees, settings.Filter.CenterLonDegrees, settings.Filter.RadiusKm);
            
            client.Run();


            // push to analyser
            client.AircraftBeaconReceived += b => analyser.AddAircraftBeacon(b);

            // loggers
            client.AircraftBeaconReceived += b => log.LogVerbose("AIRCRAFT: {0}, {1}, {2}, {3}, {4}, {5}, {6}", b.AircraftId, b.PositionTimeUTC, b.PositionLatDegrees, b.PositionLonDegrees, b.PositionAltitudeMeters, b.ClimbRateMetersPerSecond, b.RotationRateHalfTurnPerTwoMins);
            client.ReceiverBeaconReceived += b => log.LogVerbose("Receiver: {0}, {1}, {2}, {3}, {4}, {5}", b.BeaconSender, b.PositionTimeUTC, b.PositionLatDegrees, b.PositionLonDegrees, b.PositionAltitudeMeters, b.SystemInfo);

            log.LogInformation("OGN Analyser started.");
        }

        public void Dispose()
        {
            if (client != null)
                client.Dispose();

            if (analyser != null)
                analyser.Dispose();

            log.LogInformation("OGN Analyser stopped.");
        }
    }

    public static class OGNAnalyserStartupExtensions
    {
        public static IServiceCollection AddOgnAnalyserServices(this IServiceCollection sp, OGNClientSettings settings)
        {
            sp.AddSingleton<APRSClient>();
            sp.AddSingleton<AircraftTrackAnalyser>();
            sp.AddSingleton<OGNClientSettings>(r => settings);

            // neuer kommentar.

            return sp;
        }
    }
}
