namespace Core.Infrastructure.WebApi.Models
{
    public class TokenRequest
    {
        public string ClientId { get; set; } = string.Empty;
        public string TenantId { get; set; } = string.Empty;
        public string? Authority { get; set; }
        public string[]? Scopes { get; set; }
    }
}