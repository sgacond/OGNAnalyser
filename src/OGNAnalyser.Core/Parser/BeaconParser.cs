using OGNAnalyser.Core.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace OGNAnalyser.Core.Parser
{
    public class BeaconParser
    {
        public static Beacon ParseBeacon(string receivedLine)
        {
            try
            {
                Beacon beacon = null;

                if (string.IsNullOrWhiteSpace(receivedLine))
                    throw new BeaconParserException("Received line empty!", receivedLine);

                receivedLine = receivedLine.Trim();

                if (!receivedLine.StartsWith("#"))
                {
                    // ,-separated parts: sender[,proto],type,receiver:Concrete
                    //     use last part for conrete data and 2nd last part for beaconType
                    int headerSepeartorIndex = receivedLine.IndexOf(':');
                    string header = receivedLine.Substring(0, headerSepeartorIndex);
                    string body = receivedLine.Substring(headerSepeartorIndex + 1);

                    var headerParts = header.Split(',');
                    string beaconType = headerParts[headerParts.Length - 2];

                    switch (beaconType)
                    {
                        case "qAS":
                            beacon = new AircraftBeacon();
                            parseAircraftBeaconBody((AircraftBeacon)beacon, body);
                            break;

                        case "qAC":
                            beacon = new ReceiverBeacon();
                            parseReceiverBeaconBody((ReceiverBeacon)beacon, body);
                            break;

                        default:
                            throw new BeaconParserException($"Unknown package type {beaconType}", receivedLine);
                    }

                    beacon.BeaconSender = headerParts[0];
                    beacon.BeaconReceiver = headerParts[headerParts.Length - 1];
                }
                else
                {
                    beacon = new InformationalBeacon();
                    parseInformationalBeaconBody((InformationalBeacon)beacon, receivedLine);
                    beacon.BeaconSender = "system";
                    beacon.BeaconReceiver = "system";
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

        // sample: /074548h5111.32N/00102.04W'086/007/A=000607 id0ADDE626 -019fpm +0.0rot 5.5dB 3e -4.3kHzs
        private static void parseAircraftBeaconBody(AircraftBeacon beacon, string body)
        {
            parseGeographicPositionAndDateTime(beacon, body);

            try
            {

            }
            catch (Exception e)
            {
                throw new BeaconParserException("Error while parsing system info from aircraft beacon packet.", body, e);
            }
        }

        // sample: /074555h5212.73NI00007.80E&/A=000066 CPU:4.0 RAM:242.7/458.8MB NTP:0.8ms/-28.6ppm +56.2C RF:+38+2.4ppm/+1.7dB
        private static void parseReceiverBeaconBody(ReceiverBeacon beacon, string body)
        {
            parseGeographicPositionAndDateTime(beacon, body);

            try
            {
                beacon.SystemInfo = body.Substring(body.IndexOf(' ') + 1);
            }
            catch(Exception e)
            {
                throw new BeaconParserException("Error while parsing system info from receiver beacon packet.", body, e);
            }
        }

        // sample: # aprsc 2.0.14-g28c5a6a 29 Jun 2014 07:46:15 GMT GLIDERN1 37.187.40.234:14580
        private static void parseInformationalBeaconBody(InformationalBeacon beacon, string body)
        {
            beacon.InformationalData = body;
        }

        // sample: /074548h5111.32N/00102.04W'086/007/A=000607
        private static void parseGeographicPositionAndDateTime(IGeographicPositionAndDateTime beacon, string body)
        {
            try
            {
                // --> 074548h5111.32N/00102.04W'086/007/A=000607
                string positionPart = body.Substring(0, body.IndexOf(' ')).TrimStart('/');


                // local time (074548h)
                if (positionPart[6] != 'h')
                    throw new BeaconParserException("Non-Local times not supported.", positionPart);

                string localTimeSub = positionPart.TrimStart('/').Substring(0, 6);
                beacon.PositionLocalTime = DateTime.ParseExact($"{DateTime.Now:yyyyMMdd} {localTimeSub}", "yyyyMMdd HHmmss", CultureInfo.InvariantCulture);

                // lat (5111.32N)
                beacon.PositionLatDegrees = (float) Math.Round(float.Parse(positionPart.Substring(7, 7)) / 100f, 4);
                if (positionPart[14] == 'S')
                    beacon.PositionLatDegrees *= -1;

                // lon (00102.04W)
                beacon.PositionLonDegrees = (float) Math.Round(float.Parse(positionPart.Substring(16, 8)) / 100f, 4);
                if (positionPart[24] == 'W')
                    beacon.PositionLonDegrees *= -1;

                //Altitude
                beacon.PositionAltitudeMeters = (int) Math.Round(int.Parse(positionPart.Substring(positionPart.IndexOf("/A=") + 3)) / 3.28084f);

            }
            catch (Exception e)
            {
                throw new BeaconParserException("Error while parsing geographical position.", body, e);
            }
        }
    }
}
