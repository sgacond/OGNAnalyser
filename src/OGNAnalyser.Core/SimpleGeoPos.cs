using OGNAnalyser.Client.Models;

namespace OGNAnalyser.Core
{
    public class SimpleGeographicPosition : IGeographicPosition
    {
        public double PositionLatDegrees { get; set; }
        public double PositionLonDegrees { get; set; }
        public int PositionAltitudeMeters { get; set; }
    }
}
