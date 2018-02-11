using System;
using OGNAnalyser.Client.Models;

namespace OGNAnalyser.Client
{
    public interface IAPRSClient : IDisposable
    {
        event Action<AircraftBeacon> AircraftBeaconReceived;
        event Action<Beacon> BeaconReceived;
        event Action<ReceiverBeacon> ReceiverBeaconReceived;

        void Dispose();
        void Run();
    }
}