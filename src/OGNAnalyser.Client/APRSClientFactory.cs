using Microsoft.Extensions.Logging;

namespace OGNAnalyser.Client
{
    public class APRSClientFactory
    {
        private ILoggerFactory _loggerFactory;

        public APRSClientFactory(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public IAPRSClient Create(string server, int port, string username, float filterLat, float filterLon, float filterRadius, string password = "-1")
        {
            var logger = _loggerFactory.CreateLogger($"{nameof(APRSClient)}_{filterLat}_{filterLon}");
            return new APRSClient(logger, server, port, username, filterLat, filterLon, filterRadius, password);
        }
    }
}
