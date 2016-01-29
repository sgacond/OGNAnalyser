using OGNAnalyser.Core.Parser;
using OGNAnalyser.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        // aircraft
        [InlineData("ICA4B43C0>APRS,qAS,SoloStdby:/075618h4656.87N/00711.99EX331/118/A=004946 !W67! id0D4B43C0 -039fpm +0.0rot 8.0dB 0e +1.1kHz gps2x3")]
        [InlineData("FLRDDA617>APRS,qAS,Rigi:/075620h4700.13N/00819.58EX332/129/A=002460 !W87! id0EDDA617 +238fpm +0.1rot 10.0dB 1e -1.2kHz gps3x4")]
        [InlineData("FLRDDA617>APRS,qAS,Rigi:/075623h4700.23N/00819.51EX332/128/A=002476 !W23! id0EDDA617 +317fpm +0.1rot 11.5dB 0e -1.6kHz gps3x4")]
        [InlineData("ICA4B43C0>APRS,qAS,LSTBSE:/075624h4657.04N/00711.85EX331/119/A=004943 !W85! id0D4B43C0 -039fpm +0.0rot 9.8dB 0e +2.3kHz gps2x3")]
        [InlineData("FLRDF0A52>APRS,qAS,LSTB:/075625h4658.70N/00707.78Ez180/000/A=001404 !W34! id02DF0A52 -058fpm +0.0rot 54.5dB 0e +31.6kHz gps9x15 s6.00 h43")]

        public void ParserDoesNotCrash(string receivedLine)
        {
            // Assert.DoesNotThrow not available in xunit.
            // test fails anyway if it throws: https://github.com/xunit/xunit/issues/188
            var beacon = BeaconParser.ParseBeacon(receivedLine);
            Assert.IsAssignableFrom<Beacon>(beacon);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("FLRDDE626>APRS,qXXYY,EGHL:/074548h5111.32N/00102.04W'086/007/A=000607 id0ADDE626 -019fpm +0.0rot 5.5dB 3e -4.3kHzs")] // invalid beacon type
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
            Assert.IsAssignableFrom<InformationalBeacon>(beacon);
            Assert.True(((InformationalBeacon)beacon).InformationalData == receivedLine);
        }
        

        [Fact]
        public void ReceiverParsedOk()
        {
            var beacon = BeaconParser.ParseBeacon("AbtwilSG>APRS,TCPIP*,qAC,GLIDERN2:/075629h4725.20NI00918.58E&000/000/A=002234 v0.2.4.ARM CPU:0.5 RAM:747.9/970.9MB NTP:0.2ms/-3.0ppm +35.8C RF:+0.46dB");
            Assert.IsAssignableFrom<ReceiverBeacon>(beacon);
            var recBecon = (ReceiverBeacon)beacon;

            Assert.Equal("GLIDERN2", recBecon.RegistredNetwork);
            Assert.Equal(47.2520f, recBecon.PositionLatDegrees); //4725.20 1/100th Minutes in degrees
            Assert.Equal(9.1858f, recBecon.PositionLonDegrees); //918.58 1/100th Minutes in degrees
            Assert.Equal(681, recBecon.PositionAltitudeMeters); // 2234 feet in meters

            var diffMillis = recBecon.PositionLocalTime.Subtract(DateTime.Now.Date.AddHours(7).AddMinutes(56).AddSeconds(29)).TotalMilliseconds;
            Assert.True(diffMillis < 1000);

            Assert.Equal("v0.2.4.ARM CPU:0.5 RAM:747.9/970.9MB NTP:0.2ms/-3.0ppm +35.8C RF:+0.46dB", recBecon.SystemInfo);
        }

        [Fact]
        public void ReceiverParsesLatNorthSouthOK()
        {
            // see: ----------------------------------------------------------------------------------------------> N
            var recBecon1 = (ReceiverBeacon)BeaconParser.ParseBeacon("SOMETHING,TCPIP*,qAC,SOMETHING:/000000h4725.20NI00918.58E&000/000/A=000001 v0.2.4.ARM CPU:0.5 RAM:747.9/970.9MB NTP:0.2ms/-3.0ppm +35.8C RF:+0.46dB");
            Assert.Equal(47.2520f, recBecon1.PositionLatDegrees);
            // see: ----------------------------------------------------------------------------------------------> S
            var recBecon2 = (ReceiverBeacon)BeaconParser.ParseBeacon("SOMETHING,TCPIP*,qAC,SOMETHING:/000000h4725.20SI00918.58E&000/000/A=000001 v0.2.4.ARM CPU:0.5 RAM:747.9/970.9MB NTP:0.2ms/-3.0ppm +35.8C RF:+0.46dB");
            Assert.Equal(-47.2520f, recBecon2.PositionLatDegrees);
        }

        [Fact]
        public void ReceiverParsesLonEeastWestOK()
        {
            // see: --------------------------------------------------------------------------------------------------------> E
            var recBecon1 = (ReceiverBeacon)BeaconParser.ParseBeacon("SOMETHING,TCPIP*,qAC,SOMETHING:/000000h4725.20NI00918.58E&000/000/A=000001 v0.2.4.ARM CPU:0.5 RAM:747.9/970.9MB NTP:0.2ms/-3.0ppm +35.8C RF:+0.46dB");
            Assert.Equal(9.1858f, recBecon1.PositionLonDegrees);
            // see: --------------------------------------------------------------------------------------------------------> W
            var recBecon2 = (ReceiverBeacon)BeaconParser.ParseBeacon("SOMETHING,TCPIP*,qAC,SOMETHING:/000000h4725.20NI00918.58W&000/000/A=000001 v0.2.4.ARM CPU:0.5 RAM:747.9/970.9MB NTP:0.2ms/-3.0ppm +35.8C RF:+0.46dB");
            Assert.Equal(-9.1858f, recBecon2.PositionLonDegrees);
        }

    }
}
