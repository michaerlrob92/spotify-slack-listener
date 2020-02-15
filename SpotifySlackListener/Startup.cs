using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SpotifySlackListener.Infrastructure;
using SpotifySlackListener.Infrastructure.BackgroundServices;
using SpotifySlackListener.Infrastructure.Options;
using SpotifySlackListener.Infrastructure.Services;

namespace SpotifySlackListener
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        } 

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));

            services.AddHttpClient();

            services.Configure<SpotifyOptions>(Configuration.GetSection("Spotify"));
            services.Configure<SlackOptions>(Configuration.GetSection("Slack"));
            services.Configure<StatusBackgroundServiceOptions>(Configuration.GetSection("StatusBackgroundService"));
            
            services.AddRazorPages();
            services.AddServerSideBlazor();

            services.AddTransient<SpotifyService>();
            services.AddTransient<SlackService>();
            services.AddTransient<UserService>();
            services.AddProtectedBrowserStorage();

            services.AddHostedService<StatusBackgroundService>();

            services.AddHttpsRedirection(options => { options.HttpsPort = 443; });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
                app.UseHttpsRedirection();

                var rewriteOptions = new RewriteOptions()
                    .AddRedirectToWwwPermanent();
                app.UseRewriter(rewriteOptions);
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}