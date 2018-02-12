using Microsoft.Extensions.PlatformAbstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OGNAnalyser.Core;
using OGNAnalyser.Core.Analysis;
using System.Timers;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace OGNAnalyser.Launcher
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // logger factory for program.cs - startup case
            var services = new ServiceCollection();

            // load config
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("analyserconf.json", optional: true, reloadOnChange: true)
                .Build();

            services.AddLogging(l => l.AddConsole().AddConfiguration(config.GetSection("Logging")));

            var OGNAnalyserSettings = new OGNAnalyserSettings();
            config.GetSection("client").Bind(OGNAnalyserSettings);

            services.AddOGNAnalyserServices();

            var sp = services.BuildServiceProvider();

            var log = sp.GetService<ILogger<Program>>();
            log.LogInformation("Starting from console launcher... press enter to stop.");

            IDictionary<uint, AircraftBeaconSpeedAndTrack> beaconDisplayBuffer = null;
            var events = new Dictionary<string, List<AircraftTrackEvent>>();

            var factory = sp.GetService<OGNAnalyserFactory>();
            var analyserConfigSection = config.GetSection("client1");
            using (var analyser = factory.CreateOGNAnalyser(settings => analyserConfigSection.GetSection("analyser").Bind(settings)))
            {
                AttachConsoleDisplay(beaconDisplayBuffer, events, analyser);

                foreach(var airfield in analyserConfigSection.GetSection("airfields").GetChildren())
                {
                    analyser.SubscribeAirfieldForMovementEvents(airfield.Key, new SimpleGeographicPosition
                    {
                        PositionLatDegrees = airfield.GetValue<double>("lat"),
                        PositionLonDegrees = airfield.GetValue<double>("lon"),
                        PositionAltitudeMeters = airfield.GetValue<int>("alt")
                    });
                }

                analyser.Run();

                Console.ReadLine();
            }
        }

        private static void AttachConsoleDisplay(IDictionary<uint, AircraftBeaconSpeedAndTrack> beaconDisplayBuffer, Dictionary<string, List<AircraftTrackEvent>> events, IOGNAnalyser analyser)
        {
            analyser.AnalysisIntervalElapsed += lastBeacons => { beaconDisplayBuffer = lastBeacons; };
            analyser.EventDetected += (airfieldKey, evt) =>
            {
                if (!events.ContainsKey(airfieldKey))
                    events.Add(airfieldKey, new List<AircraftTrackEvent>());
                if (!events[airfieldKey].Contains(evt, new AircraftTrackEventComparer()))
                    events[airfieldKey].Add(evt);
            };

            var displayTimer = new Timer(5000);
            displayTimer.Elapsed += (s, e) =>
            {
                if (beaconDisplayBuffer == null)
                    return;

                Console.Clear();

                Console.WriteLine("-- Analysis - Aircraft: -----------------------------------------------------------");
                var nowUtc = DateTime.Now.ToUniversalTime();
                foreach (var b in beaconDisplayBuffer)
                    Console.WriteLine($"\t{b.Key:X} {b.Value.AircraftType}\t: ({Math.Round(nowUtc.Subtract(b.Value.PositionTimeUTC).TotalSeconds, 1)}s ago) {b.Value.GroundSpeedMs}ms ({Math.Round(b.Value.GroundSpeedMs * 3.6f, 1)}km/h) - {b.Value.TrackDegrees}°");

                if(events.Any())
                {
                    Console.WriteLine("-- Last Events: -------------------------------------------------------------------");
                    foreach (var airfieldEvents in events)
                    {
                        Console.WriteLine(airfieldEvents.Key);
                        foreach (var evt in airfieldEvents.Value)
                            Console.WriteLine($"\t{evt.EventDateTimeUTC:HH:mm:ss}\t{evt.EventType}\t{evt.ReferenceBeacon.AircraftId:X}");
                    }
                }
                displayTimer.Start();

                Console.WriteLine("-----------------------------------------------------------------------------------");
            };
        }
    }
}
