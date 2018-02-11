using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OGNAnalyser.Client;
using System;

namespace OGNAnalyser.Core
{
    public class OGNAnalyserFactory
    {
        private readonly APRSClientFactory _aprsClientFactory;
        private readonly ILoggerFactory _loggerFactory;

        public OGNAnalyserFactory(APRSClientFactory aprsClientFactory, ILoggerFactory loggerFactory)
        {
            _aprsClientFactory = aprsClientFactory;
            _loggerFactory = loggerFactory;
        }

        public IOGNAnalyser CreateOGNAnalyser(Action<OGNAnalyserSettings> configure)
        {
            var settings = new OGNAnalyserSettings();
            configure(settings);

            if (!settings.Complete)
                throw new InvalidOperationException("OGN Analyser not configured! Please check configuration delegate.");

            var aprsClient = _aprsClientFactory.Create(settings.OgnServer, settings.OgnPort, settings.OgnUsername, settings.Filter.CenterLatDegrees, settings.Filter.CenterLonDegrees, settings.Filter.RadiusKm);
            var logger = _loggerFactory.CreateLogger($"{nameof(OGNAnalyser)}_{settings.Filter.CenterLatDegrees}_{settings.Filter.CenterLonDegrees}");

            return new OGNAnalyser(aprsClient, settings, logger);
        }
    }
}
