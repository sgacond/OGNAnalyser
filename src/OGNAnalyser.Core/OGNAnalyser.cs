using System;
using OGNAnalyser.Client;
using OGNAnalyser.Core.Analysis;

namespace OGNAnalyserCore
{
    public class OGNAnalyser : IDisposable
    {
        private APRSClient client;
        private AircraftTrackAnalyser analyser;

        public OGNAnalyser()
        {
            client = new APRSClient("glidern1.glidernet.org", 14580, "sgtest", 47.170869f, 9.039742f, 150);
            analyser = new AircraftTrackAnalyser();

            // push to analyser
            client.AircraftBeaconReceived += b => analyser.AddAircraftBeacon(b);

            // console loggers
            client.AircraftBeaconReceived += b => Console.WriteLine($"AIRCRAFT: {b.AircraftId}, {b.PositionLatDegrees}, {b.PositionLonDegrees}, {b.PositionAltitudeMeters}, {b.ClimbRateMetersPerSecond}, {b.RotationRateHalfTurnPerTwoMins}");
            client.ReceiverBeaconReceived += b => Console.WriteLine($"Receiver: {b.BeaconSender}, {b.PositionLatDegrees}, {b.PositionLonDegrees}, {b.PositionAltitudeMeters}, {b.SystemInfo}");
        }

        public void Dispose()
        {
            if (client != null)
                client.Dispose();
        }
    }
}
