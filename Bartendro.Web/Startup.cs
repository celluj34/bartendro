using Bartendro.Common.Extensions;
using Bartendro.Database.Extensions;
using Bartendro.Web.Extensions;
using Blazor.Extensions.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PeterLeslieMorris.Blazor.Validation;
using ServiceCollectionExtensions = Bartendro.Database.Extensions.ServiceCollectionExtensions;

namespace Bartendro.Web
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddStorage()
                    .AddFormValidation(config => config.AddDataAnnotationsValidation().AddFluentValidation(typeof(ServiceCollectionExtensions).Assembly))
                    .RegisterServices()
                    .RegisterDatabase()
                    .RegisterCommon();
        }

        public void Configure(IApplicationBuilder app, IHostEnvironment env)
        {
            if(env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}