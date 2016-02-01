using OGNAnalyser.Core.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OGNAnalyser.Core.Parser
{
    public static class BeaconParser
    {
        public static readonly Regex MatcherAPRSBaseRegex = new Regex(@"(.+?)>APRS,(TCPIP\*,)?(q[A-U].),(.+?):|(\d{6})+h(\d{4}\.\d{2})(N|S).(\d{5}\.\d{2})(E|W).((\d{3})|(\d{73}))?(\/[0-9]{3}\/)?A=(\d{6}).*?");
        public static readonly Regex MatcherAircraftBodyRegex = new Regex(@"(?:\s\!W)([0-9]{2})(?:\!)(?:\sid)([a-fA-F0-9]{8})(?:\s)([+-][0-9]{3}fpm)(?:\s)([+-][0-9]\.[0-9]rot)(?:\s.*)");
        public static readonly Regex MatcherReceiverBodyRegex = new Regex(@"(?: )(.)*");

        public static Beacon ParseBeacon(string receivedLine)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(receivedLine))
                    throw new BeaconParserException("Received line empty!", receivedLine);

                Beacon beacon;

                receivedLine = receivedLine.Trim();

                if (!receivedLine.StartsWith("#"))
                {
                    var aprsMatches = MatcherAPRSBaseRegex.Matches(receivedLine);

                    if (aprsMatches.Count != 2)
                        throw new BeaconParserException("APRS Base matcher failed.", receivedLine);
                    
                    string beaconType = aprsMatches[0].Groups[3].Value;

                    switch (beaconType)
                    {
                        case "qAS":
                            beacon = new AircraftBeacon();
                            break;

                        case "qAC":
                            beacon = new ReceiverBeacon();
                            break;

                        default:
                            throw new BeaconParserException($"Unknown package type {beaconType}", receivedLine);
                    }

                    ((IBaseAPRSBeacon)beacon).parseAPRSBaseData(aprsMatches[0].Groups);
                    ((IGeographicPositionAndDateTime)beacon).parseCoords(aprsMatches[1].Groups);
                    ((ConcreteBeacon)beacon).parseOgnConcreteBeacon(receivedLine);
                }
                else
                {
                    var comment = new InformationalComment();
                    comment.InformationalData = receivedLine;
                    beacon = comment;
                }

                beacon.ParsedDateTime = DateTimeOffset.Now;
                return beacon;
            }
            catch(BeaconParserException e)
            {
                throw e;
            }
            catch(Exception e)
            {
                throw new BeaconParserException("Unknown exception while parsing beacon.", receivedLine, e);
            }
        }

        private static void parseAPRSBaseData(this IBaseAPRSBeacon beacon, GroupCollection aprsBaseHeaderMatchGroup)
        {
            beacon.BeaconSender = aprsBaseHeaderMatchGroup[1].Value;
            beacon.BeaconReceiver = aprsBaseHeaderMatchGroup[4].Value;
        }

        private static void parseCoords(this IGeographicPositionAndDateTime position, GroupCollection aprsBaseCoordsMatchGroup)
        {
            try
            {
                position.PositionLocalTime = DateTime.ParseExact($"{DateTime.Now:yyyyMMdd} {aprsBaseCoordsMatchGroup[5].Value}", "yyyyMMdd HHmmss", CultureInfo.InvariantCulture);

                // lat (5111.32N)
                position.PositionLatDegrees = (float)Math.Round(float.Parse(aprsBaseCoordsMatchGroup[6].Value) / 100f, 4);
                if (aprsBaseCoordsMatchGroup[7].Value == "S")
                    position.PositionLatDegrees *= -1;

                // lon (00102.04W)
                position.PositionLonDegrees = (float)Math.Round(float.Parse(aprsBaseCoordsMatchGroup[8].Value) / 100f, 4);
                if (aprsBaseCoordsMatchGroup[9].Value == "W")
                    position.PositionLonDegrees *= -1;

                // Altitude
                position.PositionAltitudeMeters = (int)Math.Round(int.Parse(aprsBaseCoordsMatchGroup[14].Value) / 3.28084f);
            }
            catch (Exception e)
            {
                throw new BeaconParserException("Error while parsing geographical position.", aprsBaseCoordsMatchGroup[0].Value, e);
            }
        }

        public static void parseOgnConcreteBeacon(this ConcreteBeacon beacon, string receivedLine)
        {
            switch(beacon.BeaconType)
            {
                case BeaconType.Aircraft:
                    ((AircraftBeacon)beacon).parseOgnAircraftData(receivedLine);
                    return;

                case BeaconType.Receiver:
                    ((ReceiverBeacon)beacon).parseOgnReceiverData(receivedLine);
                    return;
            }

            throw new BeaconParserException("Unknown beacon type for concrete beacon parser", receivedLine);
        }

        // sample: /064314h4658.69N/00707.77Ez000/000/A=001374 !W96! id02DF0A52 +000fpm +0.0rot 54.2dB 0e -5.0kHz gps11x18
        private static void parseOgnAircraftData(this AircraftBeacon beacon, string receivedLine)
        {
            try
            {
                var match = MatcherAircraftBodyRegex.Match(receivedLine);

                // coords extension
                string coordsExt = match.Groups[1].Value.Trim();
                beacon.PositionLatDegrees += float.Parse(coordsExt[0].ToString()) / 100000f;
                beacon.PositionLonDegrees += float.Parse(coordsExt[1].ToString()) / 100000f;

                // aircraft id
                ulong acftId = ulong.Parse(match.Groups[2].Value.Trim(), NumberStyles.HexNumber);

                // rotation
                string rot = match.Groups[3].Value.Trim();

                // info
                string info = match.Groups[4].Value.Trim();


                //string ognPart = body.Substring(body.IndexOf(' ') + 1);

                //var coordsExtensionPart = Regex.Match(body, "(?:\\!W)(.)(.)(?=\\!)").Value;
                //var addressPart = Regex.Match(body, "(?:id)([a-fA-F0-9]){8}").Value;
                //var climbRatePart = Regex.Match(body, "(\\+|\\-)(\\d+)fpm").Value;
                //var rotationPart = Regex.Match(body, "(\\+|\\-)(\\d+\\.\\d+)rot").Value;

                // signalStrength
                // errorCount

                // hearId?
                // freq offset?

            }
            catch (Exception e)
            {
                throw new BeaconParserException("Error while parsing system info from aircraft beacon packet.", receivedLine, e);
            }
        }

        // sample: /074555h5212.73NI00007.80E&/A=000066 CPU:4.0 RAM:242.7/458.8MB NTP:0.8ms/-28.6ppm +56.2C RF:+38+2.4ppm/+1.7dB
        private static void parseOgnReceiverData(this ReceiverBeacon beacon, string receivedLine)
        {
            beacon.SystemInfo = MatcherReceiverBodyRegex.Matches(receivedLine)[0].Value.Trim();
        }

        
        private static void parseReceiverBeaconBody(ReceiverBeacon beacon, string body)
        {
            try
            {
                //beacon.SystemInfo = body.Substring(body.IndexOf(' ') + 1);
            }
            catch (Exception e)
            {
                throw new BeaconParserException("Error while parsing system info from receiver beacon packet.", body, e);
            }
        }
    }
}
