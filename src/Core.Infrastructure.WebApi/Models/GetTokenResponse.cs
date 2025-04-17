namespace Core.Infrastructure.WebApi.Models
{
    public class GetTokenResponse
    {
        public string Username { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
        public DateTimeOffset ExpiresOn { get; set; }
        public string[] Scopes { get; set; } = Array.Empty<string>();
    }
}
