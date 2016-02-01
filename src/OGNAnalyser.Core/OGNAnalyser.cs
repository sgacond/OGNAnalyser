using System;
using OGNAnalyser.Client;
using System.Collections.Generic;
using OGNAnalyser.Core.Util;
using OGNAnalyser.Client.Models;
using System.Timers;

namespace OGNAnalyserCore
{
    public class OGNAnalyser : IDisposable
    {
        private const int aircraftBuffersCapacity = 100;
        private const double bufferAnalysisTimerIntervalMillis = 5000;

        private APRSClient client;
        private Dictionary<ulong, CircularBuffer<AircraftBeacon>> aircraftBuffer = new Dictionary<ulong, CircularBuffer<AircraftBeacon>>();
        private Timer bufferAnalysisTimer = new Timer(bufferAnalysisTimerIntervalMillis);

        public OGNAnalyser()
        {
            client = new APRSClient("glidern1.glidernet.org", 14580, "sgtest", 47.170869f, 9.039742f, 150);

            // console loggers
            client.AircraftBeaconReceived += b => Console.WriteLine($"AIRCRAFT: {b.AircraftId}, {b.PositionLatDegrees}, {b.PositionLonDegrees}, {b.PositionAltitudeMeters}, {b.ClimbRateMetersPerSecond}, {b.RotationRateHalfTurnPerTwoMins}");
            client.ReceiverBeaconReceived += b => Console.WriteLine($"Receiver: {b.BeaconSender}, {b.PositionLatDegrees}, {b.PositionLonDegrees}, {b.PositionAltitudeMeters}, {b.SystemInfo}");

            // buffer pusher
            client.AircraftBeaconReceived += b => 
            {
                if (!aircraftBuffer.ContainsKey(b.AircraftId))
                    aircraftBuffer.Add(b.AircraftId, new CircularBuffer<AircraftBeacon>(aircraftBuffersCapacity));

                aircraftBuffer[b.AircraftId].Enqueue(b);
            };

            bufferAnalysisTimer.Elapsed += (s, e) => 
            {
                Console.WriteLine("Analysis - Aircraft:");
                foreach(var kvp in aircraftBuffer)
                    Console.WriteLine($"\t{kvp.Key}: {kvp.Value.Length}");
            };
            bufferAnalysisTimer.Start();
        }

        public void Dispose()
        {
            if (client != null)
                client.Dispose();
        }
    }
}
