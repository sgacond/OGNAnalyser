using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OGNAnalyser.Tests.IGCConverter
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string fileName = "in.igc";
            if (args.Any())
                fileName = args[0];

            Console.WriteLine($"File {fileName}");

            // parse igc samples to coordinates and altitude ogn stuff..

            Console.ReadLine();
        }
    }
}
