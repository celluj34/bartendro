using System.Threading.Tasks;
using Bartendro.Data.Services;
using BlazorApp2.Components;
using BlazorApp2.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BlazorApp2
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            if (builder.Environment.IsDevelopment())
            {
                builder.Configuration.AddUserSecrets<Program>();
            }

            ConfigureServices(builder.Services, builder.Configuration, builder.Environment);

            var app = builder.Build();

            ConfigureApp(app, builder.Environment);

            
            await SetupDatabaseAsync(app);

            await app.RunAsync();
        }

        private static void ConfigureServices(IServiceCollection services, ConfigurationManager configuration, IHostEnvironment hostEnvironment)
        {
            // Add services to the container.
            services.AddRazorComponents().AddInteractiveServerComponents();

            services.AddServices(configuration, hostEnvironment);
        }

        private static void ConfigureApp(WebApplication app, IHostEnvironment hostEnvironment)
        {
            // Configure the HTTP request pipeline.
            if (!hostEnvironment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");

                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseAntiforgery();

            app.MapStaticAssets();
            app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
        }

        private static async Task SetupDatabaseAsync(WebApplication host)
        {
            using var scope = host.Services.CreateScope();

            var databaseMigrator = scope.ServiceProvider.GetRequiredService<IDatabaseMigrator>();
            await databaseMigrator.MigrateAsync();

            var databaseSeeder = scope.ServiceProvider.GetRequiredService<IDatabaseSeeder>();
            await databaseSeeder.SeedAsync();
        }
    }
}