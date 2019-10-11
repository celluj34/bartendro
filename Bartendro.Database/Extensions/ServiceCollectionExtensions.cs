using Microsoft.Extensions.DependencyInjection;

namespace Bartendro.Database.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterDatabase(this IServiceCollection serviceCollection)
        {
            return serviceCollection;
        }
    }
}