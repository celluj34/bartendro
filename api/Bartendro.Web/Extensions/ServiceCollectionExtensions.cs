using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BlazorApp2.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddServices(this IServiceCollection services, ConfigurationManager configuration, IHostEnvironment hostEnvironment)
        {
            return services.AddServices();
        }

        private static IServiceCollection AddServices(this IServiceCollection services)
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
                                               .ForEach(reg => services.AddTransient(reg.Interface, reg.Implementation));

            return services;
        }
    }
}