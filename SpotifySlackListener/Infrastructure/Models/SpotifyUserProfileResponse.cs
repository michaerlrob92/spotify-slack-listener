using System.Text.Json.Serialization;

namespace SpotifySlackListener.Infrastructure.Models
{
    public class SpotifyUserProfileResponse
    {
        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; }
        
        [JsonPropertyName("href")]
        public string Href { get; set; }
        
        [JsonPropertyName("id")]
        public string Id { get; set; }
        
        [JsonPropertyName("uri")]
        public string Uri { get; set; }
    }
}