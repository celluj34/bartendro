using System.Threading.Tasks;
using Bartendro.Database.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Bartendro.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            await MigrateAndSeedDatabase(host);

            await host.RunAsync();
        }

        private static async Task MigrateAndSeedDatabase(IHost host)
        {
            using(var scope = host.Services.CreateScope())
            {
                var databaseMigrator = scope.ServiceProvider.GetRequiredService<IDatabaseMigrator>();
                await databaseMigrator.MigrateAsync();

                var databaseSeeder = scope.ServiceProvider.GetRequiredService<IDatabaseSeeder>();
                await databaseSeeder.SeedAsync();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                       .ConfigureWebHostDefaults(webBuilder =>
                       {
                           webBuilder.UseStartup<Startup>();
                       });
        }
    }
}