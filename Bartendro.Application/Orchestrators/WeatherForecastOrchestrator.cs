using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bartendro.Application.Models.Weather;
using Bartendro.Database.Entities;
using Bartendro.Database.Services;
using Microsoft.EntityFrameworkCore;

namespace Bartendro.Application.Orchestrators
{
    public interface IWeatherForecastOrchestrator
    {
        Task<List<RecipeModel>> GetForecastAsync(DateTime startDate);
    }

    internal class WeatherForecastOrchestrator : IWeatherForecastOrchestrator
    {
        private readonly IReader _reader;

        public WeatherForecastOrchestrator(IReader reader)
        {
            _reader = reader;
        }

        public async Task<List<RecipeModel>> GetForecastAsync(DateTime startDate)
        {
            return await _reader.Query<Recipe>()
                                .OrderBy(x => x.Title)
                                .Take(5)
                                .Select(x => new RecipeModel
                                {
                                    Title = x.Title
                                })
                                .ToListAsync();
        }
    }
}