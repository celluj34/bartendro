using System.Linq;
using Bartendro.Database.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Bartendro.Application.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void RegisterApplication(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddOrchestratorsAndServices();
            serviceCollection.RegisterDatabase();
        }

        private static void AddOrchestratorsAndServices(this IServiceCollection serviceCollection)
        {
            var namespaces = new[]
            {
                "Bartendro.Application.Orchestrators",
                "Bartendro.Application.Services"
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
        }
    }
}