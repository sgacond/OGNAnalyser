using OGNAnalyser.Client.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OGNAnalyser.Core.Util
{
    public static class GeoExtensions
    {
        public static double DistanceToGeoPositionMeters(this IGeographicPosition pos1, IGeographicPosition pos2)
        {
            double rlat1 = Math.PI * pos1.PositionLatDegrees / 180;
            double rlat2 = Math.PI * pos2.PositionLatDegrees / 180;
            double theta = pos1.PositionLonDegrees - pos2.PositionLonDegrees;
            double rtheta = Math.PI * theta / 180;
            double dist = Math.Sin(rlat1) * Math.Sin(rlat2) + Math.Cos(rlat1) * Math.Cos(rlat2) * Math.Cos(rtheta);
            return Math.Acos(dist) * 180 / Math.PI * 60 * 1.1515 * 1609.344d;
        }
    }
}
