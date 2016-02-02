using OGNAnalyser.Client.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OGNAnalyser.Core.Util
{
    public static class GeoExtensions
    {
        // ref was: http://www.movable-type.co.uk/scripts/latlong.html

        private const double meanEarthRadius = 6371d;

        private static double deg2rad(double deg) => (deg * Math.PI / 180d);
        private static double rad2deg(double rad) => (rad / Math.PI * 180d);

        public static int DistanceToGeoPositionMeters(this IGeographicPosition pos1, IGeographicPosition pos2)
        {
            var dLat = deg2rad(pos2.PositionLatDegrees - pos1.PositionLatDegrees);
            var dLon = deg2rad(pos2.PositionLonDegrees - pos1.PositionLonDegrees);
            var lat1 = deg2rad(pos1.PositionLatDegrees);
            var lat2 = deg2rad(pos2.PositionLatDegrees);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(lat1) * Math.Cos(lat2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return (int) Math.Round(meanEarthRadius * c * 1000);
        }

        public static float InitialBearingToGeoPositionDegrees(this IGeographicPosition pos1, IGeographicPosition pos2)
        {
            var lat1 = deg2rad(pos1.PositionLatDegrees); // φ1
            var lat2 = deg2rad(pos2.PositionLatDegrees); // φ2
            var lon1 = deg2rad(pos1.PositionLonDegrees); // λ1
            var lon2 = deg2rad(pos2.PositionLonDegrees); // λ2
            var dLat = deg2rad(pos2.PositionLatDegrees - pos1.PositionLatDegrees); // Δφ
            var dLon = deg2rad(pos2.PositionLonDegrees - pos1.PositionLonDegrees); // Δλ

            var y = Math.Sin(dLon) * Math.Cos(lat2);
            var x = Math.Cos(lat1) * Math.Sin(lat2) - Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(dLon);
            var bearing = rad2deg(Math.Atan2(y, x));

            if (bearing < 0d)
                bearing += 360d;

            return (float) Math.Round(bearing, 1);
        }
    }
}
