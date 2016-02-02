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
        [Fact]
        public void TrackAnalysisGroundspeedOKSet1()
        {
            string fakeTime = $"{DateTime.Now:HHmm}";
            int curSecs = DateTime.Now.Second;

            var beaconSet1 = new AircraftBeacon[]
            {
                (AircraftBeacon) BeaconParser.ParseBeacon($"FLRDDA617>APRS,qAS,Rigi:/{fakeTime}{curSecs-18:00}h4700.13N/00819.59EX332/129/A=002460 !W86! id0EDDA617 +238fpm +0.1rot 10.0dB 1e -1.2kHz gps3x4"),
                (AircraftBeacon) BeaconParser.ParseBeacon($"FLRDDA617>APRS,qAS,Rigi:/{fakeTime}{curSecs-16:00}h4700.13N/00819.59EX332/129/A=002460 !W85! id0EDDA617 +238fpm +0.1rot 10.0dB 1e -1.2kHz gps3x4"),
                (AircraftBeacon) BeaconParser.ParseBeacon($"FLRDDA617>APRS,qAS,Rigi:/{fakeTime}{curSecs-14:00}h4700.13N/00819.59EX332/129/A=002460 !W84! id0EDDA617 +238fpm +0.1rot 10.0dB 1e -1.2kHz gps3x4"),
                (AircraftBeacon) BeaconParser.ParseBeacon($"FLRDDA617>APRS,qAS,Rigi:/{fakeTime}{curSecs-12:00}h4700.13N/00819.59EX332/129/A=002460 !W83! id0EDDA617 +238fpm +0.1rot 10.0dB 1e -1.2kHz gps3x4"),
                (AircraftBeacon) BeaconParser.ParseBeacon($"FLRDDA617>APRS,qAS,Rigi:/{fakeTime}{curSecs-10:00}h4700.13N/00819.59EX332/129/A=002460 !W82! id0EDDA617 +238fpm +0.1rot 10.0dB 1e -1.2kHz gps3x4"),
                (AircraftBeacon) BeaconParser.ParseBeacon($"FLRDDA617>APRS,qAS,Rigi:/{fakeTime}{curSecs-8:00}h4700.13N/00819.59EX332/129/A=002460 !W81! id0EDDA617 +238fpm +0.1rot 10.0dB 1e -1.2kHz gps3x4"),
                (AircraftBeacon) BeaconParser.ParseBeacon($"FLRDDA617>APRS,qAS,Rigi:/{fakeTime}{curSecs-6:00}h4700.13N/00819.59EX332/129/A=002460 !W80! id0EDDA617 +238fpm +0.1rot 10.0dB 1e -1.2kHz gps3x4"),
                (AircraftBeacon) BeaconParser.ParseBeacon($"FLRDDA617>APRS,qAS,Rigi:/{fakeTime}{curSecs-4:00}h4700.13N/00819.58EX332/129/A=002460 !W89! id0EDDA617 +238fpm +0.1rot 10.0dB 1e -1.2kHz gps3x4"),
                (AircraftBeacon) BeaconParser.ParseBeacon($"FLRDDA617>APRS,qAS,Rigi:/{fakeTime}{curSecs-2:00}h4700.13N/00819.58EX332/129/A=002460 !W88! id0EDDA617 +238fpm +0.1rot 10.0dB 1e -1.2kHz gps3x4"),
                (AircraftBeacon) BeaconParser.ParseBeacon($"FLRDDA617>APRS,qAS,Rigi:/{fakeTime}{curSecs:00}h4700.13N/00819.58EX332/129/A=002460 !W87! id0EDDA617 +238fpm +0.1rot 10.0dB 1e -1.2kHz gps3x4")
            };

            trackAnalysisGroundspeedOK(beaconSet1, 47.11f);
        }

        private void trackAnalysisGroundspeedOK(AircraftBeacon[] beaconSet, float lastGroundspeedExpected)
        {
            var buf = new CircularFifoBuffer<AircraftBeacon>(10);
            foreach (var beacon in beaconSet)
                buf.Enqueue(beacon);

            var analysis = buf.AnalyseTrack(TimeSpan.FromSeconds(60));
            Assert.Equal(lastGroundspeedExpected, analysis.EndSpeedMs);
        }
    }
}
