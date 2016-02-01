using Microsoft.Extensions.PlatformAbstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using NLog.Config;

namespace OGNAnalyser.Launcher
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Starting");
            using (var app = new OGNAnalyserCore.OGNAnalyser())
                Console.ReadLine();
        }
    }
}
