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
            
            if (string.IsNullOrWhiteSpace(spotifyToken.AccessToken) || string.IsNullOrWhiteSpace(slackToken.AccessToken))
            {
                return (false, "Your spotify access token or slack access token is invalid, please re-connect.");
            }

            // Check if we already have a user for this spotify token in the slack team
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.SpotifyAccessToken == spotifyToken.AccessToken && u.SlackTeamId == slackToken.TeamId);

            if (user == null)
            {
                // Get the user profile from spotify
                var spotifyUser = await _spotifyService.GetUserProfile(spotifyToken.AccessToken);
                if (spotifyUser == null)
                {
                    return (false, "Your spotify access token is invalid, please re-connect.");
                }
                
                // Check if we now have a user for the spotify id and team id
                user = await _context.Users.FirstOrDefaultAsync(u =>
                    u.SpotifyId == spotifyUser.Id && u.SlackTeamId == slackToken.TeamId);
                
                if (user == null)
                {
                    // We really don't have a user, let's create one for the spotify/slack combination
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
            }
            
            // Update access tokens, they might have changed?
            user.SpotifyAccessToken = spotifyToken.AccessToken;
            user.SpotifyRefreshToken = spotifyToken.RefreshToken;
            user.SlackAccessToken = slackToken.AccessToken;
            user.SlackTeamId = slackToken.TeamId;
            user.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return (true, string.Empty);
        }
    }
}