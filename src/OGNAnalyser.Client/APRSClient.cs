using OGNAnalyser.Client.Models;
using OGNAnalyser.Client.Parser;
using System;
using System.Net.Sockets;
using System.Text;
using System.Timers;
using static System.FormattableString;

namespace OGNAnalyser.Client
{
    public class APRSClient : IDisposable
    {
        private const string swInfo = "test";
        private const string swVersion = "0.1";
        private const double keepAlivePingMillis = 240000;
        private const int bufferSize = 256;

        public readonly byte[] buffer = new byte[bufferSize];

        private Socket socket;
        private string server;
        private int port;
        private string username;
        private string password;
        private float filterLat;
        private float filterLon;
        private float filterRadius;

        public event Action<Beacon> BeaconReceived;
        public event Action<AircraftBeacon> AircraftBeaconReceived;
        public event Action<ReceiverBeacon> ReceiverBeaconReceived;

        private Timer keepAlivePingTimer = null;

        public APRSClient(string server, int port, string username, float filterLat, float filterLon, float filterRadius, string password = "-1")
        {
            this.server = server;
            this.port = port;
            this.username = username;
            this.password = password;
            this.filterLat = filterLat;
            this.filterLon = filterLon;
            this.filterRadius = filterRadius;

            if (socket != null)
                Dispose();
            
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(server, port);

            // Begin receiving the data from the remote device.
            socket.BeginReceive(buffer, 0, bufferSize, 0, new AsyncCallback(receivedCallback), null);
            socket.Send(Encoding.ASCII.GetBytes(Invariant($"user {username} pass {password} vers {swInfo} {swVersion} filter r/{filterLat:#.######}/{filterLon:#.######}/{filterRadius:#}\n")));

            keepAlivePingTimer = new Timer(keepAlivePingMillis);
            keepAlivePingTimer.Elapsed += (s, e) => 
            {
                if (socket != null)
                    socket.Send(Encoding.ASCII.GetBytes($"# {swInfo} - {swVersion}"));
            };
            keepAlivePingTimer.Start();
        }

        public void Dispose()
        {
            if (socket != null && socket.Connected)
                socket.Close();

            if (keepAlivePingTimer != null)
                keepAlivePingTimer.Stop();

            socket = null;
        }

        private void receivedCallback(IAsyncResult ar)
        {
            if (!socket.Connected)
                return;

            // Read data from the remote device.
            var bytesRead = socket.EndReceive(ar);
            socket.BeginReceive(buffer, 0, bufferSize, 0, new AsyncCallback(receivedCallback), null);

            if (bytesRead > 0)
            {
                // There might be more data, so store the data received so far.
                var data = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                try
                {
                    var beacon = BeaconParser.ParseBeacon(data);

                    if (BeaconReceived != null)
                        BeaconReceived(beacon);

                    if (beacon.BeaconType == BeaconType.Aircraft && AircraftBeaconReceived != null)
                        AircraftBeaconReceived((AircraftBeacon)beacon);

                    else if (beacon.BeaconType == BeaconType.Receiver && BeaconReceived != null)
                        ReceiverBeaconReceived((ReceiverBeacon)beacon);
                }
                catch(BeaconParserException e)
                {
                    Console.Error.WriteLine($"Problem matching line: {e.StringPartParsing}");
                }
            }
        }


        //public event EventHandler<PacketInfoEventArgs> PacketReceived;
    }
}
