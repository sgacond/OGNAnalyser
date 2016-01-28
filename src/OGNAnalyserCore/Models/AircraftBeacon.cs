using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OGNAnalyserCore.Models
{
    public class AircraftBeacon : Beacon
    {
        public override BeaconType BeaconType { get { return BeaconType.Aircraft; } }

        internal override void Parse(string receivedLine)
        {
            throw new NotImplementedException();
        }
    }
}
