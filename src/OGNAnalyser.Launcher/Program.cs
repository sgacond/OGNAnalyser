using Microsoft.Extensions.PlatformAbstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Microsoft.Extensions.Logging;
using OGNAnalyser.Core;
using OGNAnalyser.Core.Analysis;
using System.Timers;
using Microsoft.Extensions.Configuration;

namespace OGNAnalyser.Launcher
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // serilog provider configuration
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.ColoredConsole()
                .CreateLogger();

            // logger factory for program.cs - startup case
            var log = new LoggerFactory().AddSerilog().CreateLogger(typeof(Program).FullName);
            log.LogInformation("Starting from console launcher... press enter to stop.");

            // load config
            var config = new ConfigurationBuilder()
                            .AddJsonFile("analyserconf.json")
                            .Build();

            // use microsofts built in simple DI container.
            var services = new ServiceCollection();
            configureServices(services, config);
            var sp = services.BuildServiceProvider();

            IDictionary<uint, AircraftBeaconSpeedAndTrack> beaconDisplayBuffer = null;
            var events = new Dictionary<string, List<AircraftTrackEvent>>();

            // disposable pattern to stop on read-line.
            using (var analyser = sp.GetService<Core.OGNAnalyser>())
            {
                attachConsoleDisplay(beaconDisplayBuffer, events, analyser);
                Console.ReadLine();
            }
        }

        private static void attachConsoleDisplay(IDictionary<uint, AircraftBeaconSpeedAndTrack> beaconDisplayBuffer, Dictionary<string, List<AircraftTrackEvent>> events, Core.OGNAnalyser analyser)
        {
            analyser.AnalysisIntervalElapsed += lastBeacons => { beaconDisplayBuffer = lastBeacons; };
            analyser.EventDetected += (airfieldKey, evt) =>
            {
                if (!events.ContainsKey(airfieldKey))
                    events.Add(airfieldKey, new List<AircraftTrackEvent>());
                if (!events[airfieldKey].Contains(evt, new AircraftTrackEventComparer()))
                    events[airfieldKey].Add(evt);
            };

            analyser.Run();

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

        private static void configureServices(IServiceCollection services, IConfigurationRoot config)
        {
            services.AddTransient<ILoggerFactory>(isp => new LoggerFactory().AddSerilog(Log.Logger));
            services.AddSingleton<Core.OGNAnalyser>();
            services.AddOgnAnalyserServices(config.Get<OGNClientSettings>("client"));
        }
    }
}
