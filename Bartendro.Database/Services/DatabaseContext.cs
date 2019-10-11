using System.Threading.Tasks;
using Bartendro.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bartendro.Database.Services
{
    internal interface IDatabaseContext
    {
        DbSet<T> Set<T>() where T : class;
        Task SaveChangesAsync();
    }

    internal class DatabaseContext : DbContext, IDatabaseContext
    {
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

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=Bartendro.db");
        }
    }
}