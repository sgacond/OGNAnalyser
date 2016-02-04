using System;
using OGNAnalyser.Client;
using OGNAnalyser.Core.Analysis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace OGNAnalyser.Core
{
    public class OGNAnalyser : IDisposable
    {
        private ILogger log;
        private APRSClient client;
        private AircraftTrackAnalyser analyser;

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

            // console loggers
            client.AircraftBeaconReceived += b => log.LogInformation($"AIRCRAFT: {b.AircraftId}, {b.PositionTimeUTC}, {b.PositionLatDegrees}, {b.PositionLonDegrees}, {b.PositionAltitudeMeters}, {b.ClimbRateMetersPerSecond}, {b.RotationRateHalfTurnPerTwoMins}");
            client.ReceiverBeaconReceived += b => log.LogInformation($"Receiver: {b.BeaconSender}, {b.PositionTimeUTC}, {b.PositionLatDegrees}, {b.PositionLonDegrees}, {b.PositionAltitudeMeters}, {b.SystemInfo}");

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
