using System.Linq;
using Bartendro.Database.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Bartendro.Database.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void RegisterDatabase(this IServiceCollection serviceCollection)
        {
            serviceCollection.RegisterServices();
            serviceCollection.RegisterValidators();
        }

        private static void RegisterServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IReader, Reader>();
            serviceCollection.AddTransient(typeof(Command<>));
            serviceCollection.AddTransient<ICommandFactory, CommandFactory>();
            serviceCollection.AddScoped<IDatabaseContext, DatabaseContext>();
            serviceCollection.AddScoped<IDatabaseMigrator, DatabaseMigrator>();
            serviceCollection.AddScoped<IDatabaseSeeder, DatabaseSeeder>();
        }

        private static IServiceCollection RegisterValidators(this IServiceCollection serviceCollection)
        {
            var iValidatorType = typeof(IValidator<>);
            var dataAnnotationsValidatorType = typeof(DataAnnotationsValidator<>);
            var validatorsNamespace = dataAnnotationsValidatorType.Namespace;

            typeof(ServiceCollectionExtensions).Assembly.GetTypes()
                                               .Where(type => type.Namespace == validatorsNamespace)
                                               .Select(type => new
                                               {
                                                   type.BaseType,
                                                   Implementation = type
                                               })
                                               .Where(type => type.BaseType.IsGenericType)
                                               .Where(type => type.BaseType.GetGenericTypeDefinition() == dataAnnotationsValidatorType)
                                               .Select(type =>
                                               {
                                                   var baseType = type.BaseType.FindInterfaces((x, y) =>
                                                       x.IsGenericType && x.GetGenericTypeDefinition() == iValidatorType,
                                                   null)
                                                   .Single();

                                                   return new
                                                   {
                                                       BaseType = baseType,
                                                       type.Implementation
                                                   };
                                               })
                                               .ToList()
                                               .ForEach(reg => serviceCollection.AddSingleton(reg.BaseType, reg.Implementation));

            return serviceCollection;
        }
    }
}