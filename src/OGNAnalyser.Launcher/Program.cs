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
            log.LogInformation("Starting from console launcher...");


            var services = new ServiceCollection();
            configureServices(services);
            var sp = services.BuildServiceProvider();
            
            var analyser = sp.GetService<Core.OGNAnalyser>();
            analyser.Run();

            Console.ReadLine();
        }

        private static void configureServices(IServiceCollection services)
        {
            services.AddTransient<ILoggerFactory>(isp => new LoggerFactory().AddSerilog());
            services.AddSingleton<Core.OGNAnalyser>();
            Core.OGNAnalyser.ConfigureServices(services);
        }
    }
}
