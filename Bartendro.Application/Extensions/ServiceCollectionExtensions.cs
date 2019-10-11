using System;
using Bartendro.Application.Orchestrators;
using Bartendro.Database.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Bartendro.Application.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterApplication(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IWeatherForecastOrchestrator, WeatherForecastOrchestrator>();
            serviceCollection.AddSingleton(new Random());

            return serviceCollection.RegisterDatabase();
        }
    }
}