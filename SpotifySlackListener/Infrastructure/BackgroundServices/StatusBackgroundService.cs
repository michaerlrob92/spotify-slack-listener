using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SpotifySlackListener.Infrastructure.Entities;
using SpotifySlackListener.Infrastructure.Extensions;
using SpotifySlackListener.Infrastructure.Options;
using SpotifySlackListener.Infrastructure.Services;

namespace SpotifySlackListener.Infrastructure.BackgroundServices
{
    public class StatusBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        private readonly ILogger<StatusBackgroundService> _logger;

        private readonly StatusBackgroundServiceOptions _serviceOptions;

        public StatusBackgroundService(IServiceProvider serviceProvider, ILogger<StatusBackgroundService> logger, IOptions<StatusBackgroundServiceOptions> options)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _serviceOptions = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogDebug($"Starting {nameof(StatusBackgroundService)}...");

            stoppingToken.Register(() => _logger.LogDebug($"Stopping {nameof(StatusBackgroundService)}."));

            while (!stoppingToken.IsCancellationRequested)
            {
                await DoWork();
                await Task.Delay(_serviceOptions.UpdatePoll, stoppingToken);
            }
        }

        private async Task DoWork()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                await using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var spotifyService = scope.ServiceProvider.GetRequiredService<SpotifyService>();
                var slackService = scope.ServiceProvider.GetRequiredService<SlackService>();
                
                // Loop through all of the users in database and update their currently playing song
                var users = context.Users.OrderBy(u => u.LastUpdated);
                await foreach (var chunk in users.QueryInChunksOfAsync(100))
                {
                    foreach (var user in chunk)
                    {
                        try
                        {
                            var player = await spotifyService.GetUserPlayer(user.SpotifyAccessToken);
                            if (player == null)
                            {
                                await UpdateSpotifyUserAccessToken(user, spotifyService, context);
                                // TODO: I know we don't update here, but next poll we do so not sure what best practice is here
                                continue;
                            }
                            
                            var statusText = await slackService.UpdateUser(user.SlackAccessToken, user.CurrentStatus, player);
                            if (statusText != user.CurrentStatus)
                            {
                                user.CurrentStatus = statusText;
                                await context.SaveChangesAsync();
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error occurred while updating user {SpotifyAccessToken} {SlackAccessToken}", user.SpotifyAccessToken, user.SlackAccessToken);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while handling update status loop.");
            }
        }

        private static async Task UpdateSpotifyUserAccessToken(User user, SpotifyService spotifyService, ApplicationDbContext context)
        {
            var token = await spotifyService.GetAccessToken(user.SpotifyRefreshToken, SpotifyGrantType.RefreshToken);
            if (string.IsNullOrWhiteSpace(token.Error))
            {
                user.SpotifyAccessToken = token.AccessToken;
                user.SpotifyRefreshToken = token.RefreshToken;
            }
            else
            {
                // Invalid tokens, we remove from database
                context.Users.Remove(user);
            }
            await context.SaveChangesAsync();
        }
    }
}