using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SpotifySlackListener.Infrastructure.Models;
using SpotifySlackListener.Infrastructure.Options;

namespace SpotifySlackListener.Infrastructure.Services
{
    public class SlackService
    {
        private readonly SlackOptions _options;

        private readonly IHttpClientFactory _httpClientFactory;

        private readonly ILogger<SlackService> _logger;

        public SlackService(IOptions<SlackOptions> options, IHttpClientFactory httpClientFactory, ILogger<SlackService> logger)
        {
            _options = options.Value;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<SlackTokenResponse> GetAccessToken(string token)
        {
            var formData = new Dictionary<string, string>
            {
                {"client_id", _options.ClientId},
                {"client_secret", _options.ClientSecret},
                {"code", token},
                {"redirect_uri", _options.RedirectUri}
            };

            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsync(_options.TokenUri, new FormUrlEncodedContent(formData));

            var responseData = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<SlackTokenResponse>(responseData);
        }

        public async Task<(bool success, string status, string error)> UpdateUser(string userSlackAccessToken, string currentStatus, SpotifyPlayerResponse player)
        {
            var profileData = new SlackUpdateStatusModel();

            if (player.IsPlaying && !player.Device.IsPrivateSession)
            {
                var statusText = player.GetStatusText();
                profileData.Profile.StatusText = statusText;
                profileData.Profile.StatusEmoji = SlackEmoji.GetEmojiForStatus(statusText);
            }

            if (currentStatus != profileData.Profile.StatusText)
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userSlackAccessToken);

                var response = await client.PostAsync(_options.ProfileUri, new StringContent(JsonSerializer.Serialize(profileData), Encoding.UTF8, "application/json"));
                var responseData = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(responseData))
                {
                    return (false, currentStatus, SlackErrorType.InvalidResponse);
                }

                var content = JsonSerializer.Deserialize<SlackUpdateStatusResponse>(responseData);
                if (!content.Ok)
                {
                    _logger.LogError("Error occurred while updating user status: {Error}", content.Error);
                    return (false, currentStatus, content.Error);
                }
            }

            return (true, profileData.Profile.StatusText, string.Empty);
        }
    }
}
