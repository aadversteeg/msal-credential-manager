using Core.Infrastructure.ConsoleApp.Models;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Core.Infrastructure.ConsoleApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // Load configuration from appsettings.json
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddUserSecrets<Program>(optional: true)
                .Build();

            // Get Azure AD settings from configuration
            var clientId = configuration["AzureAd:ClientId"];
            var tenantId = configuration["AzureAd:TenantId"];
            var authority = configuration["AzureAd:Authority"];
            var scopes = configuration.GetSection("AzureAd:Scopes")
                .GetChildren()
                .Select(x => x.Value)
                .ToArray();

            if (string.IsNullOrEmpty(clientId) || clientId == "YOUR_CLIENT_ID")
            {
                Console.WriteLine("Please update the appsettings.json file with your Azure AD app registration details");
                return;
            }

            try
            {
                // Initialize credential manager
                var credentialManager = new MsalCredentialManager(clientId, tenantId, authority, scopes);
                
                // Acquire token with popup
                var authResult = await credentialManager.GetAccessTokenAsync();
                
                // Create an anonymous object with the token info
                var tokenInfo = new
                {
                    authResult.Account.Username,
                    authResult.AccessToken,
                    authResult.ExpiresOn,
                    authResult.Scopes
                };

                // Serialize to JSON with formatting and output
                var options = new JsonSerializerOptions { WriteIndented = true };
                string jsonResult = JsonSerializer.Serialize(tokenInfo, options);
                Console.WriteLine(jsonResult);
            }
            catch (Exception ex)
            {
                // Output error as JSON
                var errorInfo = new
                {
                    Error = true,
                    ex.Message
                };
                
                var options = new JsonSerializerOptions { WriteIndented = true };
                string jsonError = JsonSerializer.Serialize(errorInfo, options);
                Console.WriteLine(jsonError);
            }
        }
    }
}