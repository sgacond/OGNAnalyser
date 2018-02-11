using Microsoft.Extensions.Logging;
using OGNAnalyser.Client.Models;
using OGNAnalyser.Client.Parser;
using OGNAnalyser.Client.Util;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace OGNAnalyser.Client
{
    internal class APRSClient : IAPRSClient
    {
        private const string swInfo = "test";
        private const string swVersion = "0.2";
        private const double keepAlivePingMillis = 240000;
        private const int bufferSize = 256;
        private const string expectedLineEnding = "\r\n";
        
        private readonly byte[] buffer = new byte[bufferSize];
        private readonly ConcurrentQueue<string> recQueue = new ConcurrentQueue<string>();

        private ILogger _log;
        private Socket _socket;
        private string _server;
        private int _port;
        private string _username;
        private string _password;
        private float _filterLat;
        private float _filterLon;
        private float _filterRadius;
        private string _halfReceivedLine = null;

        public event Action<Beacon> BeaconReceived;
        public event Action<AircraftBeacon> AircraftBeaconReceived;
        public event Action<ReceiverBeacon> ReceiverBeaconReceived;

        private Timer _keepAlivePingTimer = null;
        private volatile bool _shouldBeRunning = false;
        private Thread _workerThread = null;

        public APRSClient(ILogger logger, string server, int port, string username, float filterLat, float filterLon, float filterRadius, string password = "-1")
        {
            if (_socket != null)
                Dispose();

            this._server = server;
            this._port = port;
            this._username = username;
            this._password = password;
            this._filterLat = filterLat;
            this._filterLon = filterLon;
            this._filterRadius = filterRadius;

            _log = logger;
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            _log.LogInformation("APRS Client configured: {0}:{1} ({2}) - filter: lat {3} / lon {4} / radius {5}", server, port, username, filterLat, filterLon, filterRadius);
        }

        public void Run()
        {
            _socket.Connect(_server, _port);

            // Begin receiving the data from the remote device.
            _socket.BeginReceive(buffer, 0, bufferSize, 0, new AsyncCallback(ReceivedCallback), null);
            _socket.Send(Encoding.ASCII.GetBytes($"user {_username} pass {_password} vers {swInfo} {swVersion} filter r/{_filterLat:#.######}/{_filterLon:#.######}/{_filterRadius:#}\n"));

            _keepAlivePingTimer = new Timer((s) => {

                if (_socket != null)
                    _socket.Send(Encoding.ASCII.GetBytes($"# {swInfo} - {swVersion}"));

            }, null, TimeSpan.FromMilliseconds(keepAlivePingMillis), TimeSpan.FromMilliseconds(keepAlivePingMillis));

            _shouldBeRunning = true;
            _workerThread = new Thread(WorkerLoop);
            _workerThread.Start();

            _log.LogInformation("APRS Client started: keep alive ping time: {0}ms", keepAlivePingMillis);
        }

        public void Dispose()
        {
            if (_socket != null && _socket.Connected)
                _socket.Close();

            _keepAlivePingTimer?.Dispose();

            _socket = null;

            _shouldBeRunning = false;
            _workerThread?.Join();

            _log.LogInformation("APRS Client stopped.");
        }

        private void ReceivedCallback(IAsyncResult ar)
        {
            if (_socket == null || !_socket.Connected)
                return;

            // Read data from the remote device.
            var bytesRead = _socket.EndReceive(ar);
            if (bytesRead > 0)
            {
                // There might be more data, so store the data received so far.
                var data = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                if (_halfReceivedLine != null)
                {
                    data = _halfReceivedLine + data;
                    _halfReceivedLine = null;
                }

                string lastLine = expectedLineEnding;
                foreach(var line in data.SplitKeepDelim(expectedLineEnding))
                {
                    lastLine = line;
                    if (line.EndsWith(expectedLineEnding))
                        recQueue.Enqueue(line);
                }

                if(!lastLine.EndsWith(expectedLineEnding))
                    _halfReceivedLine = lastLine;
            }

            _socket.BeginReceive(buffer, 0, bufferSize, 0, new AsyncCallback(ReceivedCallback), null);
        }

        private void WorkerLoop()
        {
            var buf = new StringBuilder();

            while(_shouldBeRunning)
            {
                if (!recQueue.TryDequeue(out string dequeued))
                    continue;

                var lines = dequeued.SplitKeepDelim(expectedLineEnding);

                foreach (var line in lines)
                {
                    try
                    {
                        var beacon = BeaconParser.ParseBeacon(line.TrimEnd('\r', '\n'));

                        BeaconReceived?.Invoke(beacon);

                        if (beacon.BeaconType == BeaconType.Aircraft && AircraftBeaconReceived != null)
                            AircraftBeaconReceived((AircraftBeacon)beacon);

                        else if (beacon.BeaconType == BeaconType.Receiver && BeaconReceived != null)
                            ReceiverBeaconReceived((ReceiverBeacon)beacon);
                    }
                    catch (BeaconParserException e)
                    {
                        _log.LogWarning("Problem matching APRS line: {0}", e.StringPartParsing);
                    }
                }
            }
        }
    }
}
