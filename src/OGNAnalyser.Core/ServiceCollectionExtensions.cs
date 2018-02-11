using Microsoft.Extensions.DependencyInjection;
using OGNAnalyser.Client;

namespace OGNAnalyser.Core
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOGNAnalyserServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddAPRSClientServices();
            serviceCollection.AddTransient<OGNAnalyserFactory>();

            return serviceCollection;
        }
    }
}
