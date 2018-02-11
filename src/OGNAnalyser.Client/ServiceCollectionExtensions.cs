using Microsoft.Extensions.DependencyInjection;

namespace OGNAnalyser.Client
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAPRSClientServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<APRSClientFactory>();

            return serviceCollection;
        }
    }
}
