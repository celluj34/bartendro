using System;
using System.Threading.Tasks;
using Bartendro.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bartendro.Database.Services
{
    internal interface IDatabaseContext
    {
        DbSet<T> Set<T>() where T : Entity;
        Task SaveChangesAsync();
        Task MigrateAsync();
        Task<T> FindByIdAndVersionAsync<T>(Guid id, byte[] version) where T : Entity;
    }

    internal class DatabaseContext : DbContext, IDatabaseContext
    {
        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }

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
            await Database.MigrateAsync();
        }

        public async Task<T> FindByIdAndVersionAsync<T>(Guid id, byte[] version) where T : Entity
        {
            var entity = await Set<T>().FindAsync(id);

            if(entity == null)
            {
                throw new InvalidOperationException($"A(n) '{typeof(T).Name}' with id '{id}' was not found.");
            }

            if(entity.Version != version)
            {
                throw new DbUpdateConcurrencyException();
            }

            return entity;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=Bartendro.db");
        }
    }
}