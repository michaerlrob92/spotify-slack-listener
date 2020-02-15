using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using SpotifySlackListener.Infrastructure;
using SpotifySlackListener.Infrastructure.Extensions;

namespace SpotifySlackListener
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args)
                .Build()
                .MigrateDbContext<ApplicationDbContext>()
                .Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}