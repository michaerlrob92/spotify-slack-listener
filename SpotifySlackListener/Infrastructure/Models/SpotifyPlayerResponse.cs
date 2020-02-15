using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace SpotifySlackListener.Infrastructure.Models
{
    public class SpotifyPlayerResponse
    {
        [JsonPropertyName("device")]
        public SpotifyDeviceResponse Device { get; set; }
        
        [JsonPropertyName("progress_ms")]
        public int? ProgressMs { get; set; }
        
        [JsonPropertyName("is_playing")]
        public bool IsPlaying { get; set; }
        
        [JsonPropertyName("item")]
        public SpotifyTrackItemResponse Track { get; set; }
        
        [JsonPropertyName("repeat_state")]
        public string RepeatState { get; set; }
        
        [JsonPropertyName("shuffle_state")]
        public bool ShuffleState { get; set; }
        
        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; }
        
        [JsonPropertyName("currently_playing_type")]
        public string PlayingType { get; set; }

        public string GetStatusText()
        {
            var artists = string.Empty;
            
            if (Track.Artists?.Any() == true)
            {
                artists = " by " + string.Join(", ", Track.Artists.Select(a => a.Name));
            }

            var statusText = $"{Track.Name}{artists}";
            if (statusText.Length > 100)
            {
                statusText = statusText.Substring(0, 100);
            }

            return statusText;
        }
    }

    public class SpotifyDeviceResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("is_active")]
        public bool IsActive { get; set; }
        
        [JsonPropertyName("is_private_session")]
        public bool IsPrivateSession { get; set; }
        
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }
        
        [JsonPropertyName("volume_percent")]
        public int VolumePercent { get; set; }
    }

    public class SpotifyTrackItemResponse
    {
        [JsonPropertyName("album")]
        public SpotifyAlbumResponse Album { get; set; }
        
        [JsonPropertyName("artists")]
        public List<SpotifyArtistResponse> Artists { get; set; }
        
        [JsonPropertyName("duration_ms")]
        public int DurationMs { get; set; }
        
        [JsonPropertyName("explicit")]
        public bool Explicit { get; set; }
        
        [JsonPropertyName("id")]
        public string Id { get; set; }
        
        [JsonPropertyName("name")]
        public string Name { get; set; }
        
        [JsonPropertyName("popularity")]
        public int Popularity { get; set; }
        
        [JsonPropertyName("uri")]
        public string Uri { get; set; }
        
        [JsonPropertyName("is_local")]
        public bool IsLocal { get; set; }
    }

    public class SpotifyArtistResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        
        [JsonPropertyName("name")]
        public string Name { get; set; }
        
        [JsonPropertyName("uri")]
        public string Uri { get; set; }
    }

    public class SpotifyAlbumResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        
        [JsonPropertyName("name")]
        public string Name { get; set; }
        
        [JsonPropertyName("type")]
        public string Type { get; set; }
        
        [JsonPropertyName("uri")]
        public string Uri { get; set; }
    }
}