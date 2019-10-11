using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bartendro.Application.Models.Weather;
using Bartendro.Database.Entities;

namespace Bartendro.Application.Orchestrators
{
    public interface IWeatherForecastOrchestrator
    {
        Task<IEnumerable<WeatherForecastModel>> GetForecastAsync(DateTime startDate);
    }

    internal class WeatherForecastOrchestrator : IWeatherForecastOrchestrator
    {
        private static readonly string[] Summaries =
        {
            "Freezing",
            "Bracing",
            "Chilly",
            "Cool",
            "Mild",
            "Warm",
            "Balmy",
            "Hot",
            "Sweltering",
            "Scorching"
        };

        private readonly Random _random;

        public WeatherForecastOrchestrator(Random random)
        {
            _random = random;
        }

        public async Task<IEnumerable<WeatherForecastModel>> GetForecastAsync(DateTime startDate)
        {
            var weatherForecasts = Enumerable.Range(1, 5)
                                             .Select(index => new WeatherForecast
                                             {
                                                 Date = startDate.AddDays(index),
                                                 TemperatureC = _random.Next(-20, 55)
                                             });

            return weatherForecasts.Select(x => new WeatherForecastModel
            {
                Date = x.Date,
                TemperatureC = x.TemperatureC,
                Summary = Summaries[_random.Next(Summaries.Length)]
            });
        }
    }
}