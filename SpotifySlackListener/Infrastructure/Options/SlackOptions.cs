namespace SpotifySlackListener.Infrastructure.Options
{
    public class SlackOptions
    {
        public string AuthorizeUri { get; set; }
        
        public string TokenUri { get; set; }
        
        public string ProfileUri { get; set; }
        
        public string RedirectUri { get; set; }
        
        public string ClientId { get; set; }
        
        public string ClientSecret { get; set; }
        
        public string Scopes { get; set; }
    }
}