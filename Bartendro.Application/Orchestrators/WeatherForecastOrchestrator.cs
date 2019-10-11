using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bartendro.Application.Models.Weather;
using Bartendro.Database.Entities;
using Bartendro.Database.Services;

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
        private readonly IReader _reader;

        public WeatherForecastOrchestrator(Random random, IReader reader)
        {
            _random = random;
            _reader = reader;
        }

        public async Task<IEnumerable<WeatherForecastModel>> GetForecastAsync(DateTime startDate)
        {
            var t = _reader.Query<Blog>();

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