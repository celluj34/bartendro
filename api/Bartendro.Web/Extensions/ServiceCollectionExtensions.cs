using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Bartendro.Web.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterServices(this IServiceCollection serviceCollection)
        {
            var namespaces = new[]
            {
                "Bartendro.Web.Services"
            };

            typeof(ServiceCollectionExtensions).Assembly.GetTypes()
                                               .Where(type => type.IsClass)
                                               .Where(type => namespaces.Contains(type.Namespace))
                                               .Where(type => type.GetInterfaces().Length == 1)
                                               .Where(type => type.IsNested == false)
                                               .Select(type => new
                                               {
                                                   Interface = type.GetInterfaces().Single(),
                                                   Implementation = type
                                               })
                                               .ToList()
                                               .ForEach(reg => serviceCollection.AddTransient(reg.Interface, reg.Implementation));

            return serviceCollection;
        }
    }
}