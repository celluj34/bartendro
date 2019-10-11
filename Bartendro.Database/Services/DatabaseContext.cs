using Bartendro.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bartendro.Database.Services
{
    internal interface IDatabaseContext
    {
        DbSet<T> Set<T>() where T : class;
    }

    internal class DatabaseContext : DbContext, IDatabaseContext
    {
        public DbSet<Blog> Blogs {get;set;}

        DbSet<T> IDatabaseContext.Set<T>()
        {
            return Set<T>();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=Bartendro.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Map table names
            modelBuilder.Entity<Blog>().ToTable("Blogs", "test");
            modelBuilder.Entity<Blog>(entity =>
            {
                entity.HasKey(e => e.BlogId);
                entity.HasIndex(e => e.Title).IsUnique();
                entity.Property(e => e.DateTimeAdd).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}