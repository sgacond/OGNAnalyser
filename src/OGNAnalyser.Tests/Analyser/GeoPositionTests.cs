using OGNAnalyser.Client.Models;
using OGNAnalyser.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace OGNAnalyser.Tests.Analyser
{
    public class GeoPositionTests
    {
        private class SimpleGeoPos : IGeographicPosition
        {
            public double PositionLatDegrees { get; set; }
            public double PositionLonDegrees { get; set; }
            public int PositionAltitudeMeters { get; set; }
        }

        [Theory]
        [InlineData(0d, 0d, 0d, 0d, 0d)]
        [InlineData(47.1717d, 9.0394d, 47.3764d, 8.7575d, 31151d)] // schaenis / fehraltorf
        [InlineData(47.1717d, 9.0394d, 51.4633d, 9.0494d, 477205d)] // schaenis / dehausen diemels (north)
        [InlineData(47.1717d, 9.0394d, 47.1858d, 14.1264d, 384414d)] // schaenis / Schoeder (east)
        [InlineData(47.1717d, 9.0394d, 45.4495d, 8.9986d, 191526d)] // schaenis / cisiliano ul (south)
        [InlineData(47.1717d, 9.0394d, 47.1939d, 2.0653d, 526898d)] // schaenis / vierzon mereau (west)
        public void DistanceToGeoPositionMatches(double lat1, double lon1, double lat2, double lon2, double expectedDistanceMeters)
        {
            var geoPos1 = new SimpleGeoPos { PositionLatDegrees = lat1, PositionLonDegrees = lon1 };
            var geoPos2 = new SimpleGeoPos { PositionLatDegrees = lat2, PositionLonDegrees = lon2 };
            Assert.Equal(expectedDistanceMeters, geoPos1.DistanceToGeoPositionMeters(geoPos2));
        }

        [Theory]
        [InlineData(0d, 0d, 0d, 0d, 0d)]
        [InlineData(47.1717d, 9.0394d, 47.3764d, 8.7575d, 317f)] // schaenis / fehraltorf
        [InlineData(47.1717d, 9.0394d, 51.4633d, 9.0494d, 0.1f)] // schaenis / dehausen diemels (north)
        [InlineData(47.1717d, 9.0394d, 47.1858d, 14.1264d, 87.9f)] // schaenis / Schoeder (east)
        [InlineData(47.1717d, 9.0394d, 45.4495d, 8.9986d, 181f)] // schaenis / cisiliano ul (south)
        [InlineData(47.1717d, 9.0394d, 47.1939d, 2.0653d, 272.8f)] // schaenis / vierzon mereau (west)
        public void InitialBearingToGeoPositionMatches(double lat1, double lon1, double lat2, double lon2, float expectedInitialBearingDegrees)
        {
            var geoPos1 = new SimpleGeoPos { PositionLatDegrees = lat1, PositionLonDegrees = lon1 };
            var geoPos2 = new SimpleGeoPos { PositionLatDegrees = lat2, PositionLonDegrees = lon2 };
            Assert.Equal(expectedInitialBearingDegrees, geoPos1.InitialBearingToGeoPositionDegrees(geoPos2));
        }
    }
}
