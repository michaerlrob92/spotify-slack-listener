using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SpotifySlackListener.Infrastructure.Models;
using SpotifySlackListener.Infrastructure.Options;

namespace SpotifySlackListener.Infrastructure.Services
{
    public class SpotifyService
    {
        private readonly SpotifyOptions _options;

        private readonly IHttpClientFactory _httpClientFactory;

        public SpotifyService(IOptions<SpotifyOptions> options, IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _options = options.Value;
        }

        public async Task<SpotifyTokenResponse> GetAccessTokenFromCode(string code)
        {
            var formData = new Dictionary<string, string>
            {
                {"client_id", _options.ClientId},
                {"client_secret", _options.ClientSecret},
                {"grant_type", SpotifyGrantType.Code},
                {"redirect_uri", _options.RedirectUri},
                {"code", code}
            };

            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsync(_options.TokenUri, new FormUrlEncodedContent(formData));

            var responseData = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<SpotifyTokenResponse>(responseData);
        }

        public async Task<SpotifyTokenResponse> GetAccessTokenFromRefreshToken(string token)
        {
            var formData = new Dictionary<string, string>
            {
                {"grant_type", SpotifyGrantType.RefreshToken},
                {"refresh_token", token}
            };

            var client = _httpClientFactory.CreateClient();
            var authorizationHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_options.ClientId}:{_options.ClientSecret}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authorizationHeader);

            var response = await client.PostAsync(_options.TokenUri, new FormUrlEncodedContent(formData));

            var responseData = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<SpotifyTokenResponse>(responseData);
        }

        public async Task<SpotifyUserProfileResponse> GetUserProfile(string accessToken)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"{_options.ProfileUri}?access_token={accessToken}");
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return null;
            }

            var responseData = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(responseData))
            {
                return null;
            }

            return JsonSerializer.Deserialize<SpotifyUserProfileResponse>(responseData);
        }

        public async Task<SpotifyPlayerResponse> GetUserPlayer(string accessToken)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"{_options.PlayerUri}?access_token={accessToken}");
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return null;
            }

            var responseData = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<SpotifyPlayerResponse>(responseData);
        }
    }
}
