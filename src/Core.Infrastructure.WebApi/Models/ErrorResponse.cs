namespace Core.Infrastructure.WebApi.Models
{
    public class ErrorResponse
    {
        public bool Error { get; set; } = true;
        public string Message { get; set; } = string.Empty;
    }
}