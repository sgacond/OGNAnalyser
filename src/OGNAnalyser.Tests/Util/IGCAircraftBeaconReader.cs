using OGNAnalyser.Client.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OGNAnalyser.Tests
{
    public class IGCAircraftBeaconReader
    {
        private static readonly Regex bRecordMatchRegex = new Regex(@"B([0-9]{6})([0-9]{7}[NS])([0-9]{8}[EW])A([0-9]{5})([0-9]{5}).*");
        private static readonly char[] invertingChars = new char[] { 'S', 's', 'W', 'w' };

        public static IEnumerable<AircraftBeacon> ReadFromIGCFile(string path, ulong acftId)
        {
            var beacons = new List<AircraftBeacon>();
            int lagAlt = 0;
            DateTime lagDt = DateTime.MinValue;

            foreach (var line in File.ReadLines(path))
            {
                var match = bRecordMatchRegex.Match(line);
                if (!match.Success)
                    continue;

                DateTime bRecDateTime = DateTime.ParseExact($"19841111 {match.Groups[1]}", "yyyyMMdd HHmmss", CultureInfo.InvariantCulture);

                // take gps alt... without offset
                int alt = int.Parse(match.Groups[5].Value);

                beacons.Add(new AircraftBeacon
                {
                    AircraftId = acftId,
                    BeaconReceiver = "TESTIGC",
                    BeaconSender = "TESTIGC",
                    CenterFrequencyOffsetKhz = 0,
                    GpsSatellitesVisible = 18,
                    GpsSatelliteChannelsAvailable = 18,
                    TransmissionErrorsCorrected = 0,
                    SignalNoiseRatioDb = 54f,
                    RotationRateHalfTurnPerTwoMins = 0,
                    ParsedDateTime = bRecDateTime,
                    PositionTimeUTC = bRecDateTime,
                    PositionAltitudeMeters = alt,
                    PositionLatDegrees = igcFormatToDegrees(match.Groups[2].Value),
                    PositionLonDegrees = igcFormatToDegrees(match.Groups[3].Value),
                    ClimbRateMetersPerSecond = (float)Math.Round(lagAlt == 0 ? 0d : (((double)(alt - lagAlt)) / bRecDateTime.Subtract(lagDt).TotalSeconds), 2)
                });

                lagAlt = alt;
                lagDt = bRecDateTime;
            }

            var corr = DateTime.Now.ToUniversalTime().Subtract(beacons.Max(b => b.PositionTimeUTC));
            beacons.ForEach(b => b.PositionTimeUTC = b.PositionTimeUTC.Add(corr));
            beacons.ForEach(b => b.ParsedDateTime = b.ParsedDateTime.Add(corr));
            
            return beacons;
        }

        private static double igcFormatToDegrees(string input)
        {
            // 4711375N  -->  47° 11.375min
            // 00902591E --> 009° 02.591min 
            int degIdx = input.Length - 6; // 2 or 3
            int degrees = int.Parse(input.Substring(0, degIdx));
            int thouMinutes = int.Parse(input.Substring(degIdx, input.Length - degIdx - 1));
            return Math.Round((double)degrees + (double)thouMinutes / 60000d * (invertingChars.Contains(input.Last()) ? -1d : 1d), 6);
        }
    }
}
