using OGNAnalyser.Client.Models;
using OGNAnalyser.Client.Parser;
using OGNAnalyser.Core.Analysis;
using OGNAnalyser.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace OGNAnalyser.Tests.Analyser
{
    public class TrackAnalysisTests
    {
        [Theory]
        [InlineData("Data\\54oz5of2_Landing.igc")]
        public void IgcTest1(string path)
        {
            var beacons = IGCAircraftBeaconReader.ReadFromIGCFile(path, 0x0A000001); // simulate fifo collection
            var beaconsBuffer = new CircularFifoBuffer<AircraftBeaconSpeedAndTrack>(beacons.Count() - 4); // gice the circular buffer a chance to fill the internal buffer
            foreach (var beacon in beacons)
                beaconsBuffer.Enqueue(new AircraftBeaconSpeedAndTrack(beacon));
            
            var myTestPoints = beaconsBuffer.Select(b => new { b.Beacon.PositionLatDegrees, b.Beacon.PositionLonDegrees, b.Beacon.PositionAltitudeMeters, b.Beacon.ClimbRateMetersPerSecond, b.GroundSpeedMs, b.TrackDegrees }).ToList();
            throw new NotImplementedException("this shouldn't pass... just a test-test.");
        }

        //[Fact]
        //public void TrackAnalysisGroundspeedOKSet1()
        //{
        //    string fakeTime = $"{DateTime.Now:HHmm}";
        //    int curSecs = DateTime.Now.Second;

        //    var beaconSet1 = new Client.Models.AircraftBeacon[]
        //    {
        //        (Client.Models.AircraftBeacon) BeaconParser.ParseBeacon($"FLRDDA617>APRS,qAS,Rigi:/{fakeTime}{curSecs-18:00}h4700.13N/00819.59EX332/129/A=002460 !W86! id0EDDA617 +238fpm +0.1rot 10.0dB 1e -1.2kHz gps3x4"),
        //        (Client.Models.AircraftBeacon) BeaconParser.ParseBeacon($"FLRDDA617>APRS,qAS,Rigi:/{fakeTime}{curSecs-16:00}h4700.13N/00819.59EX332/129/A=002460 !W85! id0EDDA617 +238fpm +0.1rot 10.0dB 1e -1.2kHz gps3x4"),
        //        (Client.Models.AircraftBeacon) BeaconParser.ParseBeacon($"FLRDDA617>APRS,qAS,Rigi:/{fakeTime}{curSecs-14:00}h4700.13N/00819.59EX332/129/A=002460 !W84! id0EDDA617 +238fpm +0.1rot 10.0dB 1e -1.2kHz gps3x4"),
        //        (Client.Models.AircraftBeacon) BeaconParser.ParseBeacon($"FLRDDA617>APRS,qAS,Rigi:/{fakeTime}{curSecs-12:00}h4700.13N/00819.59EX332/129/A=002460 !W83! id0EDDA617 +238fpm +0.1rot 10.0dB 1e -1.2kHz gps3x4"),
        //        (Client.Models.AircraftBeacon) BeaconParser.ParseBeacon($"FLRDDA617>APRS,qAS,Rigi:/{fakeTime}{curSecs-10:00}h4700.13N/00819.59EX332/129/A=002460 !W82! id0EDDA617 +238fpm +0.1rot 10.0dB 1e -1.2kHz gps3x4"),
        //        (Client.Models.AircraftBeacon) BeaconParser.ParseBeacon($"FLRDDA617>APRS,qAS,Rigi:/{fakeTime}{curSecs-8:00}h4700.13N/00819.59EX332/129/A=002460 !W81! id0EDDA617 +238fpm +0.1rot 10.0dB 1e -1.2kHz gps3x4"),
        //        (Client.Models.AircraftBeacon) BeaconParser.ParseBeacon($"FLRDDA617>APRS,qAS,Rigi:/{fakeTime}{curSecs-6:00}h4700.13N/00819.59EX332/129/A=002460 !W80! id0EDDA617 +238fpm +0.1rot 10.0dB 1e -1.2kHz gps3x4"),
        //        (Client.Models.AircraftBeacon) BeaconParser.ParseBeacon($"FLRDDA617>APRS,qAS,Rigi:/{fakeTime}{curSecs-4:00}h4700.13N/00819.58EX332/129/A=002460 !W89! id0EDDA617 +238fpm +0.1rot 10.0dB 1e -1.2kHz gps3x4"),
        //        (Client.Models.AircraftBeacon) BeaconParser.ParseBeacon($"FLRDDA617>APRS,qAS,Rigi:/{fakeTime}{curSecs-2:00}h4700.13N/00819.58EX332/129/A=002460 !W88! id0EDDA617 +238fpm +0.1rot 10.0dB 1e -1.2kHz gps3x4"),
        //        (Client.Models.AircraftBeacon) BeaconParser.ParseBeacon($"FLRDDA617>APRS,qAS,Rigi:/{fakeTime}{curSecs:00}h4700.13N/00819.58EX332/129/A=002460 !W87! id0EDDA617 +238fpm +0.1rot 10.0dB 1e -1.2kHz gps3x4")
        //    };

        //    trackAnalysisGroundspeedOK(beaconSet1, 47.11f);
        //}

        //private void trackAnalysisGroundspeedOK(Client.Models.AircraftBeacon[] beaconSet, float lastGroundspeedExpected)
        //{
        //    var buf = new CircularFifoBuffer<Client.Models.AircraftBeacon>(10);
        //    foreach (var beacon in beaconSet)
        //        buf.Enqueue(beacon);

        //    buf.AnalyseSpeedAndTrack(TimeSpan.FromSeconds(60));
        //    Assert.Equal(lastGroundspeedExpected, analysis.EndSpeedMs);
        //}
    }
}
