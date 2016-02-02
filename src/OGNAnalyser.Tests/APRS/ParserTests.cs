using OGNAnalyser.Client.Parser;
using OGNAnalyser.Client.Models;
using System;
using Xunit;

namespace OGNAnalyser.Tests.APRS
{
    public class ParserTests
    {
        [Theory]
        // info
        [InlineData("# aprsc 2.0.18-ge7666c5")]
        [InlineData("# logresp sgtest unverified, server GLIDERN1")]
        [InlineData("# aprsc 2.0.14-g28c5a6a 29 Jun 2014 07:46:15 GMT GLIDERN1 37.187.40.234:14580")]
        [InlineData("# aprsc 2.0.18-ge7666c5 29 Jan 2016 07:57:04 GMT GLIDERN1 37.187.40.234:14580")]
        // receivers
        [InlineData("AbtwilSG>APRS,TCPIP*,qAC,GLIDERN2:/075629h4725.20NI00918.58E&000/000/A=002234 v0.2.4.ARM CPU:0.5 RAM:747.9/970.9MB NTP:0.2ms/-3.0ppm +35.8C RF:+0.46dB")]
        [InlineData("Niederurn>APRS,TCPIP*,qAC,GLIDERN1:/075634h4707.48NI00903.33E&000/000/A=002821 v0.2.4.ARM CPU:0.7 RAM:774.5/972.2MB NTP:0.6ms/-8.0ppm +32.6C RF:+0.77dB")]
        [InlineData("EDNE>APRS,TCPIP*,qAC,GLIDERN2:/075642h4820.54NI00954.83E&000/000/A=001588 v0.2.4.RPI-GPU CPU:1.0 RAM:194.7/455.7MB NTP:12.1ms/-9.3ppm +48.7C RF:+2.46dB")]
        [InlineData("BachtelN>APRS,TCPIP*,qAC,GIGA01:/125043h4716.85NI00852.79E&/A=002486 v0.2.2 CPU:0.9 RAM:273.0/456.5MB NTP:0.6ms/-11.6ppm +40.1C RF:+13.71dB")]
        // aircraft
        [InlineData("ICA4B43C0>APRS,qAS,SoloStdby:/075618h4656.87N/00711.99EX331/118/A=004946 !W67! id0D4B43C0 -039fpm +0.0rot 8.0dB 0e +1.1kHz gps2x3")]
        [InlineData("FLRDDA617>APRS,qAS,Rigi:/075620h4700.13N/00819.58EX332/129/A=002460 !W87! id0EDDA617 +238fpm +0.1rot 10.0dB 1e -1.2kHz gps3x4")]
        [InlineData("FLRDDA617>APRS,qAS,Rigi:/075623h4700.23N/00819.51EX332/128/A=002476 !W23! id0EDDA617 +317fpm +0.1rot 11.5dB 0e -1.6kHz gps3x4")]
        [InlineData("ICA4B43C0>APRS,qAS,LSTBSE:/075624h4657.04N/00711.85EX331/119/A=004943 !W85! id0D4B43C0 -039fpm +0.0rot 9.8dB 0e +2.3kHz gps2x3")]
        [InlineData("FLRDF0A52>APRS,qAS,LSTB:/075625h4658.70N/00707.78Ez180/000/A=001404 !W34! id02DF0A52 -058fpm +0.0rot 54.5dB 0e +31.6kHz gps9x15 s6.00 h43")]
        [InlineData("FLRDD0525>APRS,qAS,Rigi:/125048h4658.64N/00844.88E'282/067/A=002732 !W12! id06DD0525 +1307fpm +0.1rot 7.2dB 0e +6.3kHz gps3x4")]
        [InlineData("FLRDD0525>APRS,qAS,Rigi:/125144h4659.38N/00843.74E'027/076/A=004097 !W65! id06DD0525 +1663fpm +0.2rot 8.8dB 0e +6.0kHz gps4x5")]
        [InlineData("FLRDD0525>APRS,qAS,LSZKWest:/130334h4719.58N/00842.57E'315/100/A=002916 id06DD0525 -118fpm +0.0rot 15.5dB 0e -12.0kHz gps3x3")]
        public void ParserDoesNotCrash(string receivedLine)
        {
            // Assert.DoesNotThrow not available in xunit.
            // test fails anyway if it throws: https://github.com/xunit/xunit/issues/188
            var beacon = BeaconParser.ParseBeacon(receivedLine);
            Assert.IsAssignableFrom<Beacon>(beacon);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)] //-----> qXXYY (invalid beacon type)
        [InlineData("FLRDDE626>APRS,qXXYY,EGHL:/074548h5111.32N/00102.04W'086/007/A=000607 id0ADDE626 -019fpm +0.0rot 5.5dB 3e -4.3kHzs")]
        public void ParserDoesCrashOnInvalidLine(string receivedLine)
        {
            // Assert.DoesNotThrow not available in xunit.
            // test fails anyway if it throws: https://github.com/xunit/xunit/issues/188
            Assert.Throws<BeaconParserException>(() => BeaconParser.ParseBeacon(receivedLine));
        }

        [Theory]
        [InlineData("# aprsc 2.0.18-ge7666c5")]
        [InlineData("# logresp sgtest unverified, server GLIDERN1")]
        [InlineData("# aprsc 2.0.14-g28c5a6a 29 Jun 2014 07:46:15 GMT GLIDERN1 37.187.40.234:14580")]
        [InlineData("# aprsc 2.0.18-ge7666c5 29 Jan 2016 07:57:04 GMT GLIDERN1 37.187.40.234:14580")]
        [InlineData("# just something")]
        public void InformationalLineStoresLine(string receivedLine)
        {
            var beacon = BeaconParser.ParseBeacon(receivedLine);
            Assert.IsAssignableFrom<InformationalComment>(beacon);
            Assert.True(((InformationalComment)beacon).InformationalData == receivedLine);
        }

        [Fact]
        public void ReceiverParsed()
        {
            var beacon = BeaconParser.ParseBeacon("AbtwilSG>APRS,TCPIP*,qAC,GLIDERN2:/075629h4725.20NI00918.58E&000/000/A=002234 v0.2.4.ARM CPU:0.5 RAM:747.9/970.9MB NTP:0.2ms/-3.0ppm +35.8C RF:+0.46dB");
            Assert.IsAssignableFrom<ReceiverBeacon>(beacon);
            var recBecon = (ReceiverBeacon)beacon;

            Assert.Equal("GLIDERN2", recBecon.RegistredNetwork);
            Assert.Equal(47.42d, recBecon.PositionLatDegrees); //47° 25.20 Minutes in degrees
            Assert.Equal(9.30966667d, recBecon.PositionLonDegrees);  // 9° 18.58 Minutes in degrees
            Assert.Equal(681, recBecon.PositionAltitudeMeters); // 2234 feet in meters

            var diffMillis = recBecon.PositionTimeUtc.Subtract(DateTime.Now.Date.AddHours(7).AddMinutes(56).AddSeconds(29)).TotalMilliseconds;
            Assert.True(diffMillis < 1000);

            Assert.Equal("v0.2.4.ARM CPU:0.5 RAM:747.9/970.9MB NTP:0.2ms/-3.0ppm +35.8C RF:+0.46dB", recBecon.SystemInfo);
        }

        [Fact]
        public void ReceiverParsesLatNorthSouth()
        {
            // see: ----------------------------------------------------------------------------------------------> N
            var recBecon1 = (ReceiverBeacon)BeaconParser.ParseBeacon("SOMETH>APRS,TCPIP*,qAC,SOMETHING:/000000h4725.20NI00918.58E&000/000/A=000001 v0.2.4.ARM CPU:0.5 RAM:747.9/970.9MB NTP:0.2ms/-3.0ppm +35.8C RF:+0.46dB");
            Assert.Equal(47.42d, recBecon1.PositionLatDegrees);
            // see: ----------------------------------------------------------------------------------------------> S
            var recBecon2 = (ReceiverBeacon)BeaconParser.ParseBeacon("SOMETH>APRS,TCPIP*,qAC,SOMETHING:/000000h4725.20SI00918.58E&000/000/A=000001 v0.2.4.ARM CPU:0.5 RAM:747.9/970.9MB NTP:0.2ms/-3.0ppm +35.8C RF:+0.46dB");
            Assert.Equal(-47.42d, recBecon2.PositionLatDegrees);
        }

        [Fact]
        public void ReceiverParsesLonEeastWest()
        {
            // see: --------------------------------------------------------------------------------------------------------> E
            var recBecon1 = (ReceiverBeacon)BeaconParser.ParseBeacon("SOMETH>APRS,TCPIP*,qAC,SOMETHING:/000000h4725.20NI00918.58E&000/000/A=000001 v0.2.4.ARM CPU:0.5 RAM:747.9/970.9MB NTP:0.2ms/-3.0ppm +35.8C RF:+0.46dB");
            Assert.Equal(9.30966667d, recBecon1.PositionLonDegrees);
            // see: --------------------------------------------------------------------------------------------------------> W
            var recBecon2 = (ReceiverBeacon)BeaconParser.ParseBeacon("SOMETH>APRS,TCPIP*,qAC,SOMETHING:/000000h4725.20NI00918.58W&000/000/A=000001 v0.2.4.ARM CPU:0.5 RAM:747.9/970.9MB NTP:0.2ms/-3.0ppm +35.8C RF:+0.46dB");
            Assert.Equal(-9.30966667d, recBecon2.PositionLonDegrees);
        }

        [Fact]
        public void AircraftParser()
        {
            var beacon = BeaconParser.ParseBeacon("FLRDF0A52>APRS,qAS,LSTB:/064314h4658.69N/00707.77Ez000/000/A=000840 !W96! id02DF0A52 +110fpm +1.2rot 54.2dB 1e -5.1kHz gps11x18");
            Assert.IsAssignableFrom<AircraftBeacon>(beacon);
            var acftBecon = (AircraftBeacon)beacon;

            Assert.Equal(46.97831667d, acftBecon.PositionLatDegrees); // 46° 58.69 Minutes in degrees + .009 minutes (!W96!)
            Assert.Equal(7.1296d, acftBecon.PositionLonDegrees);  //  7° 07.77 Minutes in degrees + .006 minutes (!W96!)
            Assert.True(0x02DF0A52 == acftBecon.AircraftId); // id02DF0A52
            Assert.Equal(256, acftBecon.PositionAltitudeMeters);  // 840 feet in meters
            Assert.Equal(0.56f, acftBecon.ClimbRateMetersPerSecond); // 110fpm in m/s
            Assert.Equal(1.2f, acftBecon.RotationRateHalfTurnPerTwoMins); // +1.2rot
            Assert.Equal(54.2f, acftBecon.SignalNoiseRatioDb); // 54.2dB
            Assert.Equal(1, acftBecon.TransmissionErrorsCorrected); // 1e
            Assert.Equal(-5.1f, acftBecon.CenterFrequencyOffsetKhz); // -5.1kHz
            Assert.Equal(11, acftBecon.GpsSatellitesVisible); // gps11x18
            Assert.Equal(18, acftBecon.GpsSatelliteChannelsAvailable); // gps11x18
        }

        [Fact]
        public void AircraftParserNegativeRotationRate()
        {
            var beacon = BeaconParser.ParseBeacon("FLRDF0A52>APRS,qAS,LSTB:/064314h4658.69N/00707.77Ez000/000/A=000840 !W96! id02DF0A52 +110fpm -1.2rot 54.2dB 0e -5.0kHz gps11x18");
            Assert.IsAssignableFrom<AircraftBeacon>(beacon);
            var acftBecon = (AircraftBeacon)beacon;

            Assert.Equal(-1.2f, acftBecon.RotationRateHalfTurnPerTwoMins); // -1.2rot
        }

        [Fact]
        public void AircraftParserNegativeClimbRate()
        {
            var beacon = BeaconParser.ParseBeacon("FLRDF0A52>APRS,qAS,LSTB:/064314h4658.69N/00707.77Ez000/000/A=000840 !W96! id02DF0A52 -110fpm -1.2rot 54.2dB 0e -5.0kHz gps11x18");
            Assert.IsAssignableFrom<AircraftBeacon>(beacon);
            var acftBecon = (AircraftBeacon)beacon;

            Assert.Equal(-0.56f, acftBecon.ClimbRateMetersPerSecond); // -110fpm in m/s
        }

        [Theory]
        [InlineData(0x0A000001, AircraftBeacon.AddressTypes.Flarm, AircraftBeacon.AircraftTypes.TowPlane, 0x000001, false, false)]
        [InlineData(0x8A000001, AircraftBeacon.AddressTypes.Flarm, AircraftBeacon.AircraftTypes.TowPlane, 0x000001, true, false)] // stealth
        [InlineData(0xCA000001, AircraftBeacon.AddressTypes.Flarm, AircraftBeacon.AircraftTypes.TowPlane, 0x000001, true, true)] // stealth & no track
        [InlineData(0x4A000001, AircraftBeacon.AddressTypes.Flarm, AircraftBeacon.AircraftTypes.TowPlane, 0x000001, false, true)] // no track
        [InlineData(0x26000001, AircraftBeacon.AddressTypes.Flarm, AircraftBeacon.AircraftTypes.PoweredJet, 0x000001, false, false)] // jet
        [InlineData(0x06000001, AircraftBeacon.AddressTypes.Flarm, AircraftBeacon.AircraftTypes.Glider, 0x000001, false, false)] // glider
        [InlineData(0x07000002, AircraftBeacon.AddressTypes.OGN, AircraftBeacon.AircraftTypes.Glider, 0x000002, false, false)] // ogn address
        [InlineData(0x04000003, AircraftBeacon.AddressTypes.Random, AircraftBeacon.AircraftTypes.Glider, 0x000003, false, false)] // random address
        [InlineData(0x05000004, AircraftBeacon.AddressTypes.ICAO, AircraftBeacon.AircraftTypes.Glider, 0x000004, false, false)] // icao address
        [InlineData(0x05FFFFFF, AircraftBeacon.AddressTypes.ICAO, AircraftBeacon.AircraftTypes.Glider, 0xFFFFFF, false, false)] // icao full address
        [InlineData(0x05DDE626, AircraftBeacon.AddressTypes.ICAO, AircraftBeacon.AircraftTypes.Glider, 0xDDE626, false, false)] // icao other address
        public void AircraftParserAddressDecoder(ulong id, AircraftBeacon.AddressTypes expectedAddressType, AircraftBeacon.AircraftTypes expectedAircraftType, uint expectedAddressValue, bool expectedStealthFlag, bool expectedNoTrackFlag)
        {
            var beacon = new AircraftBeacon { AircraftId = id };
            Assert.Equal(expectedAddressType, beacon.AddressType);
            Assert.Equal(expectedAircraftType, beacon.AircraftType);
            Assert.Equal(expectedAddressValue, beacon.AircraftAddress);
            Assert.Equal(expectedStealthFlag, beacon.StealthMode);
            Assert.Equal(expectedNoTrackFlag, beacon.NoTrackingFlag);
        }
    }
}
