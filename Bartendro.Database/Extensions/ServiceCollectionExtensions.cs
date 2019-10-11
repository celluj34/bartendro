using Bartendro.Database.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Bartendro.Database.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterDatabase(this IServiceCollection serviceCollection)
        {
            return serviceCollection.AddTransient<IReader, Reader>().AddScoped<IDatabaseContext, DatabaseContext>();
        }
    }
}