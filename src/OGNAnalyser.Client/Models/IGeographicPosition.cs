using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OGNAnalyser.Client.Models
{
    public interface IGeographicPosition
    {
        double PositionLatDegrees { get; set; }
        double PositionLonDegrees { get; set; }
        int PositionAltitudeMeters { get; set; }
    }

    public interface IGeographicPositionAndDateTime : IGeographicPosition
    {
        DateTime PositionTimeUTC { get; set; }
    }
}
