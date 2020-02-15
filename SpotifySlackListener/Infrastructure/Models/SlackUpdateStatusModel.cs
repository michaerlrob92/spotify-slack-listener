using System.Text.Json.Serialization;

namespace SpotifySlackListener.Infrastructure.Models
{
    public class SlackUpdateStatusModel
    {
        [JsonPropertyName("profile")]
        public SlackUpdateStatusProfileModel Profile { get; set; }

        public SlackUpdateStatusModel()
        {
            Profile = new SlackUpdateStatusProfileModel();
        }
    }

    public class SlackUpdateStatusProfileModel
    {
        [JsonPropertyName("status_text")]
        public string StatusText { get; set; }
        
        [JsonPropertyName("status_emoji")]
        public string StatusEmoji { get; set; }
        
        [JsonPropertyName("status_expiration")]
        public int StatusExpiration { get; set; }
    }
}