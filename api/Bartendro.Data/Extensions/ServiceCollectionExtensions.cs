using System.Linq;
using Bartendro.Data.Commands;
using Bartendro.Data.Services;
using Bartendro.Data.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Bartendro.Data.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterDatabase(this IServiceCollection serviceCollection)
        {
            return serviceCollection.RegisterServices().RegisterValidators();
        }

        private static IServiceCollection RegisterServices(this IServiceCollection serviceCollection)
        {
            return serviceCollection.AddTransient<IReader, Reader>()
                                    .AddTransient(typeof(CreateCommand<>))
                                    .AddTransient(typeof(UpdateCommand<>))
                                    .AddTransient(typeof(DeleteCommand<>))
                                    .AddTransient<ICommandFactory, CommandFactory>()
                                    .AddScoped<IDatabaseContext, DatabaseContext>()
                                    .AddScoped<IDatabaseMigrator, DatabaseMigrator>()
                                    .AddScoped<IDatabaseSeeder, DatabaseSeeder>();
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