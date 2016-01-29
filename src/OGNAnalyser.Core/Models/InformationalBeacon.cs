using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OGNAnalyser.Core.Models
{
    /// <summary>
    /// Informational beacon ("comment").
    /// </summary>
    public class InformationalBeacon : Beacon
    {
        public override BeaconType BeaconType { get { return BeaconType.Informational; } }

        public string InformationalData { get; internal set; }
    }
}
