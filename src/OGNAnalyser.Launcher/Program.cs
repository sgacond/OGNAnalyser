using Microsoft.Extensions.PlatformAbstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Microsoft.Extensions.Logging;

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
            
            // use microsofts built in simple DI container.
            var services = new ServiceCollection();
            configureServices(services);
            var sp = services.BuildServiceProvider();

            // disposable pattern to stop on read-line.
            using (var analyser = sp.GetService<Core.OGNAnalyser>())
            {
                // debug view
                analyser.AnalysisIntervalElapsed += lastBeacons =>
                {
                    Console.Clear();
                    Console.WriteLine("-- Analysis - Aircraft: -----------------------------------------------------------");
                    var nowUtc = DateTime.Now.ToUniversalTime();
                    foreach (var b in lastBeacons)
                        Console.WriteLine($"\t{b.Key:X} {b.Value.AircraftType}\t: ({Math.Round(nowUtc.Subtract(b.Value.PositionTimeUTC).TotalSeconds, 1)}s ago) {b.Value.GroundSpeedMs}ms ({Math.Round(b.Value.GroundSpeedMs * 3.6f, 1)}km/h) - {b.Value.TrackDegrees}°");
                    Console.WriteLine("-----------------------------------------------------------------------------------");
                };

                analyser.Run();
                Console.ReadLine();
            }
        }

        private static void configureServices(IServiceCollection services)
        {
            services.AddTransient<ILoggerFactory>(isp => new LoggerFactory().AddSerilog(Log.Logger));
            services.AddSingleton<Core.OGNAnalyser>();
            Core.OGNAnalyser.ConfigureServices(services);
        }
    }
}
