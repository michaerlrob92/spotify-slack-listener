using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SpotifySlackListener.Infrastructure.Extensions
{
    public static class IHostExtensions
    {
        public static IHost MigrateDbContext<TContext>(this IHost host,
            Action<TContext, IServiceProvider> seedAction = null)
            where TContext : DbContext
        {
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<TContext>>();
            var context = services.GetRequiredService<TContext>();
            
            try
            {
                context.Database.Migrate();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while migrating database");
            }

            return host;
        }
    }
}