using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OGNAnalyserCore.Models
{
    /// <summary>
    /// Aircraft beacon. Parsed from APRS String.
    /// Sample: FLRDDE626>APRS,qAS,EGHL:/074548h5111.32N/00102.04W'086/007/A=000607 id0ADDE626 -019fpm +0.0rot 5.5dB 3e -4.3kHzs
    /// </summary>
    public class AircraftBeacon : Beacon
    {
        public override BeaconType BeaconType { get { return BeaconType.Aircraft; } }

        protected override void parseConcretePart(string concretePart)
        {

        }
    }
}
