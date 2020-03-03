using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

                // We only need to retrieve the current song for a spotify id once, group by spotify id
                var users = context.Users.OrderBy(u => u.Id);

                await foreach (var chunk in users.QueryInChunksOfAsync(100))
                {
                    foreach (var group in chunk.GroupBy(u => u.SpotifyId, u => u))
                    {
                        var firstUser = group.First();

                        try
                        {
                            var player = await spotifyService.GetUserPlayer(firstUser.SpotifyAccessToken);

                            if (player == null)
                            {
                                // Get first available refresh token
                                var refreshToken = firstUser.SpotifyRefreshToken ?? group
                                    .FirstOrDefault(u => !string.IsNullOrWhiteSpace(u.SpotifyRefreshToken))?.SpotifyRefreshToken;

                                // Update all users with their new refresh/access tokens
                                await UpdateSpotifyUserAccessToken(group, refreshToken, spotifyService, context);

                                continue;
                            }

                            // Update all users with their new status
                            foreach (var user in group)
                            {
                                var (success, statusText, error) = await slackService.UpdateUser(user.SlackAccessToken, user.CurrentStatus, player);

                                if (success && statusText != user.CurrentStatus)
                                {
                                    user.CurrentStatus = statusText;
                                }
                                else if (error == SlackErrorType.InvalidAuth)
                                {
                                    context.Users.Remove(user);
                                }
                            }

                            await context.SaveChangesAsync();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error occurred while updating user {SpotifyId}", firstUser.SpotifyId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while handling update status loop.");
            }
        }

        private static async Task UpdateSpotifyUserAccessToken(IEnumerable<User> users, string refreshToken, SpotifyService spotifyService, ApplicationDbContext context)
        {
            var token = await spotifyService.GetAccessTokenFromRefreshToken(refreshToken);
            if (string.IsNullOrWhiteSpace(token.Error))
            {
                foreach (var user in users)
                {
                    user.SpotifyAccessToken = token.AccessToken;

                    if (string.IsNullOrWhiteSpace(token.RefreshToken) == false)
                    {
                        user.SpotifyRefreshToken = token.RefreshToken;
                    }

                    user.LastUpdated = DateTime.UtcNow;
                }
            }
            else
            {
                // Invalid tokens, we remove from database
                context.Users.RemoveRange(users);
            }

            await context.SaveChangesAsync();
        }
    }
}
