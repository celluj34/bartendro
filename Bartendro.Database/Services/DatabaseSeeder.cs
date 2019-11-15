using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bartendro.Common.Services;
using Bartendro.Database.Entities;
using Microsoft.Extensions.Logging;

namespace Bartendro.Database.Services
{
    public interface IDatabaseSeeder
    {
        Task SeedAsync();
    }

    internal class DatabaseSeeder : IDatabaseSeeder
    {
        private readonly IDatabaseContext _context;
        private readonly IDateTimeService _dateTimeService;
        private readonly ILogger<DatabaseSeeder> _logger;

        public DatabaseSeeder(IDatabaseContext context, IDateTimeService dateTimeService, ILogger<DatabaseSeeder> logger)
        {
            _context = context;
            _dateTimeService = dateTimeService;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            try
            {
                var recipes = _context.Set<Recipe>();

                if(recipes.Any())
                {
                    return;
                }

                recipes.AddRange(GetDefaultRecipes());

                await _context.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "An error occurred seeding the database.");

                throw;
            }
        }

        private IEnumerable<Recipe> GetDefaultRecipes()
        {
            var now = _dateTimeService.Now();

            return new List<Recipe>
            {
                new Recipe
                {
                    Id = Guid.NewGuid(),
                    Title = "Default Recipe",
                    DateCreated = now,
                    DateModified = now
                },
                new Recipe
                {
                    Id = Guid.NewGuid(),
                    Title = "Default Recipe 2",
                    DateCreated = now,
                    DateModified = now
                }
            };
        }
    }
}