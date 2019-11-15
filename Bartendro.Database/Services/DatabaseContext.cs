using System;
using System.Threading.Tasks;
using Bartendro.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bartendro.Database.Services
{
    public interface IDatabaseMigrator
    {
        Task MigrateAsync();
    }

    internal interface IDatabaseContext
    {
        DbSet<T> Set<T>() where T : class;
        Task SaveChangesAsync();
    }

    internal class DatabaseContext : DbContext, IDatabaseContext, IDatabaseMigrator
    {
        private readonly ILogger<DatabaseContext> _logger;

        public DatabaseContext(ILogger<DatabaseContext> logger)
        {
            _logger = logger;
        }

        public DbSet<Recipe> Recipes {get;set;}
        public DbSet<Ingredient> Ingredients {get;set;}

        DbSet<T> IDatabaseContext.Set<T>()
        {
            return Set<T>();
        }

        async Task IDatabaseContext.SaveChangesAsync()
        {
            await SaveChangesAsync();
        }

        public async Task MigrateAsync()
        {
            try
            {
                await Database.MigrateAsync();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "An error occurred migrating the database.");

                throw;
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=Bartendro.db");
        }
    }
}