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
        }

        private static void RegisterValidators(this IServiceCollection serviceCollection)
        {
            const string validatorsNamespace = "Bartendro.Database.Validators";
            var abstractValidatorType = typeof(AbstractValidator<>);

            typeof(ServiceCollectionExtensions).Assembly.GetTypes()
                                               .Where(type => type.Namespace == validatorsNamespace)
                                               .Select(type => new
                                               {
                                                   BaseClass = type.BaseType,
                                                   Implementation = type
                                               })
                                               .Where(type => type.BaseClass.IsGenericType)
                                               .Where(type => type.BaseClass.GetGenericTypeDefinition() == abstractValidatorType)
                                               .ToList()
                                               .ForEach(reg => serviceCollection.AddSingleton(reg.BaseClass, reg.Implementation));
        }
    }
}