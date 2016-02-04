using Microsoft.Extensions.Logging;
using OGNAnalyser.Client.Models;
using OGNAnalyser.Client.Parser;
using OGNAnalyser.Client.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        private const string expectedLineEnding = "\r\n";

        public readonly byte[] buffer = new byte[bufferSize];

        private ILogger log;
        private Socket socket;
        private string server;
        private int port;
        private string username;
        private string password;
        private float filterLat;
        private float filterLon;
        private float filterRadius;
        private string halfReceivedLine = null;
        private object receiverLock = new object();

        public event Action<Beacon> BeaconReceived;
        public event Action<AircraftBeacon> AircraftBeaconReceived;
        public event Action<ReceiverBeacon> ReceiverBeaconReceived;

        private Timer keepAlivePingTimer = null;

        public APRSClient(ILoggerFactory loggerFactory)
        {
            if (socket != null)
                Dispose();

            log = loggerFactory.CreateLogger<APRSClient>();
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Configure(string server, int port, string username, float filterLat, float filterLon, float filterRadius, string password = "-1")
        {
            this.server = server;
            this.port = port;
            this.username = username;
            this.password = password;
            this.filterLat = filterLat;
            this.filterLon = filterLon;
            this.filterRadius = filterRadius;

            log.LogInformation("APRS Client configured: {0}:{1} ({2}) - filter: lat {3} / lon {4} / radius {5}", server, port, username, filterLat, filterLon, filterRadius);
        }

        public void Run()
        {
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

            log.LogInformation("APRS Client started: keep alive ping time: {0}ms", keepAlivePingMillis);
        }

        public void Dispose()
        {
            if (socket != null && socket.Connected)
                socket.Close();

            if (keepAlivePingTimer != null)
                keepAlivePingTimer.Stop();

            socket = null;

            log.LogInformation("APRS Client stopped.");
        }

        private void receivedCallback(IAsyncResult ar)
        {
            if (socket == null || !socket.Connected)
                return;

            lock (receiverLock)
            {
                // Read data from the remote device.
                var bytesRead = socket.EndReceive(ar);
                socket.BeginReceive(buffer, 0, bufferSize, 0, new AsyncCallback(receivedCallback), null);

                if (bytesRead > 0)
                {
                    // There might be more data, so store the data received so far.
                    var data = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                    var lines = data.SplitKeepDelim(expectedLineEnding).ToArray();

                    if (halfReceivedLine != null && lines.Any())
                    {
                        lines[0] = halfReceivedLine + lines[0];
                        halfReceivedLine = null;
                    }

                    foreach (var line in lines)
                    {
                        if (line.EndsWith(expectedLineEnding))
                        {
                            try
                            {
                                var beacon = BeaconParser.ParseBeacon(line.TrimEnd('\r', '\n'));

                                if (BeaconReceived != null)
                                    BeaconReceived(beacon);

                                if (beacon.BeaconType == BeaconType.Aircraft && AircraftBeaconReceived != null)
                                    AircraftBeaconReceived((AircraftBeacon)beacon);

                                else if (beacon.BeaconType == BeaconType.Receiver && BeaconReceived != null)
                                    ReceiverBeaconReceived((ReceiverBeacon)beacon);
                            }
                            catch (BeaconParserException e)
                            {
                                log.LogWarning("Problem matching APRS line: {0}", e.StringPartParsing);
                            }
                        }
                        else
                            halfReceivedLine = line;
                    }
                }
            }
        }
    }
}
