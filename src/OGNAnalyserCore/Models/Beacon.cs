using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OGNAnalyserCore.Models
{
    public enum BeaconType
    {
        Aircraft,
        Receiver,
        Informational
    }

    /// <summary>
    /// Abstract APRS Beacon message. Use static ParseBeacon to parse a APRS Message.
    /// </summary>
    public abstract class Beacon
    {
        public abstract BeaconType BeaconType { get; }

        public DateTime ParsedDateTime { get; private set; }

        public string Sender { get; private set; }

        protected abstract void parseConcretePart(string concretePart);

        internal static Beacon ParseBeacon(string receivedLine)
        {
            Beacon beacon = null;

            receivedLine = receivedLine.Trim();

            if (!receivedLine.StartsWith("#"))
            {
                // , - separated parts: sender,proto,Type,Concrete
                var topLevelParts = receivedLine.Split(',');

                switch(topLevelParts[2])
                {
                    case "qAS":
                        beacon = new AircraftBeacon();
                        break;

                    case "qAC":
                        beacon = new ReceiverBeacon();
                        break;
                }

                beacon.Sender = topLevelParts[0];
                beacon.parseConcretePart(topLevelParts[3]);
            }
            else
            {
                beacon = new InformationalBeacon();
                beacon.Sender = "system";
                beacon.parseConcretePart(receivedLine);
            }

            beacon.ParsedDateTime = DateTime.Now;
            return beacon;
        }
    }
}
