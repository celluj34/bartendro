using Bartendro.Common.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bartendro.Common.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterCommon(this IServiceCollection services)
        {
            return services.AddSingleton<IDateTimeService, DateTimeService>();
        }

        public static IServiceCollection ConfigureSettings<T>(this IServiceCollection serviceCollection, IConfiguration configuration) where T : class
        {
            return serviceCollection.Configure<T>(options => configuration.GetSection(typeof(T).Name).Bind(options));
        }
    }
}