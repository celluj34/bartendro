using System.Threading.Tasks;
using BlazorApp2.Components;
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

            await app.RunAsync();
        }

        private static void ConfigureServices(IServiceCollection services, ConfigurationManager configuration, IHostEnvironment hostEnvironment)
        {
            // Add services to the container.
            services.AddRazorComponents().AddInteractiveServerComponents();
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
    }
}