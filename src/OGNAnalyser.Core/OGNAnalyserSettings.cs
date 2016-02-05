using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OGNAnalyser.Core
{

    // das muss wohl ein interface werden, das rumerbt... damit die client settings bei den client settings sind und die analyser settings bei denen.
    // collections für flugplätze und so.

    public class OGNClientSettings
    {
        public string OgnServer { get; set; }
        public string OgnUsername { get; set; }
        public int OgnPort { get; set; }
        public OGNClientFiterSettings Filter { get; set; }
    }

    public class OGNClientFiterSettings
    {
        public float CenterLatDegrees { get; set; }
        public float CenterLonDegrees { get; set; }
        public int RadiusKm { get; set; }
    }
}
