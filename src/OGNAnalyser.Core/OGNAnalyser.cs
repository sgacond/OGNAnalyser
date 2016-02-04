using System;
using OGNAnalyser.Client;
using OGNAnalyser.Core.Analysis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace OGNAnalyser.Core
{
    public class OGNAnalyser : IDisposable
    {
        private ILogger log;
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

        public static void ConfigureServices(IServiceCollection sp)
        {
            sp.AddSingleton<APRSClient>();
            sp.AddSingleton<AircraftTrackAnalyser>();
        }

        public OGNAnalyser(APRSClient client, AircraftTrackAnalyser analyser, ILoggerFactory logFactory)
        {
            log = logFactory.CreateLogger<OGNAnalyser>();
            this.client = client;
            this.analyser = analyser;
        }

        public void Run()
        {
            client.Configure("glidern1.glidernet.org", 14580, "sgtest", 47.170869f, 9.039742f, 150);
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
}
