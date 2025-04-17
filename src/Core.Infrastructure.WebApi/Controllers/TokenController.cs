using Core.Infrastructure.WebApi.Models;
using Core.Infrastructure.WebApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;

namespace Core.Infrastructure.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TokenController : ControllerBase
    {
        private readonly ILogger<TokenController> _logger;
        private readonly IConfiguration _configuration;
        private readonly TokenCacheService _tokenCacheService;

        public TokenController(ILogger<TokenController> logger, 
                              IConfiguration configuration,
                              TokenCacheService tokenCacheService)
        {
            _logger = logger;
            _configuration = configuration;
            _tokenCacheService = tokenCacheService;
        }


        [HttpPost]
        [ProducesResponseType(typeof(GetTokenResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Post([FromBody] TokenRequest request)
        {
            _logger.LogInformation("Token request received via POST");
            
            try
            {
                // Validate request
                if (string.IsNullOrEmpty(request.ClientId))
                {
                    return BadRequest(new ErrorResponse { Message = "ClientId is required" });
                }
                
                if (string.IsNullOrEmpty(request.TenantId))
                {
                    return BadRequest(new ErrorResponse { Message = "TenantId is required" });
                }
                
                // Use provided authority or construct it from tenantId
                string authority = request.Authority ?? $"https://login.microsoftonline.com/{request.TenantId}";
                
                // Use provided scopes or default to User.Read
                string[] scopes = request.Scopes?.Length > 0 
                    ? request.Scopes 
                    : new[] { "https://graph.microsoft.com/User.Read" };
                
                // Initialize credential manager with token cache service and request parameters
                var credentialManager = new MsalCredentialManager(
                    _tokenCacheService,
                    request.ClientId,
                    request.TenantId,
                    authority,
                    scopes);
                
                // Acquire token with popup
                _logger.LogInformation("Initiating authentication with Microsoft Graph using provided credentials");
                var authResult = await credentialManager.GetAccessTokenAsync();
                _logger.LogInformation("Authentication successful for user: {Username}", authResult.Account.Username);
                
                // Create response
                var response = new GetTokenResponse
                {
                    Username = authResult.Account.Username,
                    AccessToken = authResult.AccessToken,
                    ExpiresOn = authResult.ExpiresOn,
                    Scopes = authResult.Scopes.ToArray()
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error acquiring token");
                
                return StatusCode(500, new ErrorResponse
                {
                    Message = ex.Message
                });
            }
        }
    }
}