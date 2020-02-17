using System.Text.Json.Serialization;

namespace SpotifySlackListener.Infrastructure.Models
{
    public class SlackUpdateStatusResponse
    {
        [JsonPropertyName("ok")]
        public bool Ok { get; set; }

        [JsonPropertyName("error")]
        public string Error { get; set; }
    }
}
