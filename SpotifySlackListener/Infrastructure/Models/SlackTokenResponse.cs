using System.Text.Json.Serialization;

namespace SpotifySlackListener.Infrastructure.Models
{
    public class SlackTokenResponse
    {
        [JsonPropertyName("ok")]
        public bool Ok { get; set; }
        
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }
        
        [JsonPropertyName("scope")]
        public string Scope { get; set; }
        
        [JsonPropertyName("user_id")]
        public string UserId { get; set; }
        
        [JsonPropertyName("team_id")]
        public string TeamId { get; set; }
        
        [JsonPropertyName("team_name")]
        public string TeamName { get; set; }
        
        [JsonPropertyName("error")]
        public string Error { get; set; }
    }
}