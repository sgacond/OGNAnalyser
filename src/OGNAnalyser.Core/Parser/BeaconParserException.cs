using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OGNAnalyser.Core.Parser
{
    public class BeaconParserException : Exception
    {
        public string StringPartParsing { get; private set; }

        public BeaconParserException(string message, string stringPartParsing) : base(message)
        {
            StringPartParsing = stringPartParsing;
        }

        public BeaconParserException(string message, string stringPartParsing, Exception innerException) : base(message, innerException)
        {
            StringPartParsing = stringPartParsing;
        }
    }
}
