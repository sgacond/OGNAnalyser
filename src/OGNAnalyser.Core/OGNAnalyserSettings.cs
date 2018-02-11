using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OGNAnalyser.Core
{
    public class OGNAnalyserSettings
    {
        public string OgnServer { get; set; }
        public string OgnUsername { get; set; }
        public int OgnPort { get; set; }
        public OGNAnalyserFilterSettings Filter { get; set; }

        public bool Complete =>
            !string.IsNullOrEmpty(OgnServer) && !string.IsNullOrEmpty(OgnUsername) && OgnPort > 0 && Filter != null;
    }

    public class OGNAnalyserFilterSettings
    {
        public float CenterLatDegrees { get; set; }
        public float CenterLonDegrees { get; set; }
        public int RadiusKm { get; set; }
    }
}
