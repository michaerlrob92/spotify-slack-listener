using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SpotifySlackListener.Infrastructure.Entities;
using SpotifySlackListener.Infrastructure.Models;

namespace SpotifySlackListener.Infrastructure.Services
{
    public class UserService
    {
        private readonly ApplicationDbContext _context;

        private readonly SpotifyService _spotifyService;

        private readonly SlackService _slackService;

        public UserService(ApplicationDbContext context, SpotifyService spotifyService, SlackService slackService)
        {
            _context = context;
            _spotifyService = spotifyService;
            _slackService = slackService;
        }

        public async Task<(bool status, string error)> AddOrUpdateUser(string spotifyAccess, string slackAccess)
        {
            if (string.IsNullOrWhiteSpace(spotifyAccess) || string.IsNullOrWhiteSpace(slackAccess))
            {
                return (false, "Your spotify access token or slack access token is invalid, please re-connect.");
            }

            var spotifyToken = JsonSerializer.Deserialize<SpotifyTokenResponse>(spotifyAccess);
            var slackToken = JsonSerializer.Deserialize<SlackTokenResponse>(slackAccess);

            // Make sure we have access tokens for spotify & slack
            if (string.IsNullOrWhiteSpace(spotifyToken.AccessToken) || string.IsNullOrWhiteSpace(slackToken.AccessToken))
            {
                return (false, "Your spotify access token or slack access token is invalid, please re-connect.");
            }

            // If we match an existing user, don't worry about it.
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.SpotifyAccessToken == spotifyToken.AccessToken && u.SlackAccessToken == slackToken.AccessToken);

            if (user == null)
            {
                // Get the user profile from spotify
                var spotifyUser = await _spotifyService.GetUserProfile(spotifyToken.AccessToken);
                if (spotifyUser == null)
                {
                    return (false, "Your spotify access token is invalid, please re-connect.");
                }

                // Match the user to their spotify id and slack access token, spotify access tokens change, slack don't
                user = await _context.Users.FirstOrDefaultAsync(u =>
                    u.SpotifyId == spotifyUser.Id && u.SlackAccessToken == slackToken.AccessToken);

                if (user == null)
                {
                    user = new User
                    {
                        Id = Guid.NewGuid().ToString(),
                        SpotifyId = spotifyUser.Id,
                        SpotifyAccessToken = spotifyToken.AccessToken,
                        SpotifyRefreshToken = spotifyToken.RefreshToken,
                        SlackId = slackToken.UserId,
                        SlackAccessToken = slackToken.AccessToken,
                        SlackTeamId = slackToken.TeamId,
                        Created = DateTime.UtcNow,
                        LastUpdated = DateTime.UtcNow
                    };

                    _context.Users.Add(user);
                }
                else
                {
                    user.SpotifyId = spotifyUser.Id;
                    user.SpotifyAccessToken = spotifyToken.AccessToken;
                    user.SpotifyRefreshToken = spotifyToken.RefreshToken;
                    user.SlackId = slackToken.UserId;
                    user.SlackTeamId = slackToken.TeamId;
                    user.SlackAccessToken = slackToken.AccessToken;
                    user.LastUpdated = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
            }

            return (true, string.Empty);
        }
    }
}
