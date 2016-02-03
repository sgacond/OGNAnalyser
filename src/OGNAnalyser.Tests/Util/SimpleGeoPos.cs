using OGNAnalyser.Client.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OGNAnalyser.Tests.Util
{
    internal class SimpleGeoPos : IGeographicPosition
    {
        public double PositionLatDegrees { get; set; }
        public double PositionLonDegrees { get; set; }
        public int PositionAltitudeMeters { get; set; }
    }
}
