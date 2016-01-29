using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OGNAnalyser.Core.Models
{
    public interface IGeographicPosition
    {
        float PositionLatDegrees { get; set; }
        float PositionLonDegrees { get; set; }
        int PositionAltitudeMeters { get; set; }
    }

    public interface IGeographicPositionAndDateTime : IGeographicPosition
    {
        DateTime PositionLocalTime { get; set; }
    }
}
