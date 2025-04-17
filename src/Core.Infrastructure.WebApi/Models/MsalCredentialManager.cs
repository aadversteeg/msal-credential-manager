using Microsoft.Identity.Client;
using Core.Infrastructure.WebApi.Services;

namespace Core.Infrastructure.WebApi.Models
{
    public class MsalCredentialManager
    {
        private readonly IPublicClientApplication _msalClient;
        private readonly string[] _scopes;

        public MsalCredentialManager(TokenCacheService tokenCacheService, string clientId, string tenantId, string authority, string[] scopes)
        {
            _scopes = scopes;

            // Get a cached client application from the token cache service
            _msalClient = tokenCacheService.GetClientApplication(clientId, tenantId, authority);
        }

        public async Task<AuthenticationResult> AcquireTokenInteractiveAsync()
        {
            try
            {
                // Try to get token silently first
                var accounts = await _msalClient.GetAccountsAsync();
                if (accounts.Any())
                {
                    try
                    {
                        return await _msalClient.AcquireTokenSilent(_scopes, accounts.FirstOrDefault())
                            .ExecuteAsync();
                    }
                    catch (MsalUiRequiredException)
                    {
                        // Silent token acquisition failed, fall back to interactive
                    }
                }

                // Interactive authentication with system browser
                // Use a compact system browser popup
                return await _msalClient.AcquireTokenInteractive(_scopes)
                    .WithPrompt(Prompt.SelectAccount)
                    .ExecuteAsync();
            }
            catch
            {
                // No console output here, just rethrow
                throw;
            }
        }

        public async Task<AuthenticationResult> GetAccessTokenAsync()
        {
            return await AcquireTokenInteractiveAsync();
        }

        public async Task SignOutAsync()
        {
            var accounts = await _msalClient.GetAccountsAsync();
            
            foreach (var account in accounts)
            {
                await _msalClient.RemoveAsync(account);
            }
        }
    }
}