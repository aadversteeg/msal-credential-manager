using Microsoft.Identity.Client;

namespace Core.Infrastructure.WebApi.Services
{
    /// <summary>
    /// Service for persistent token caching
    /// </summary>
    public class TokenCacheService
    {
        private readonly Dictionary<string, IPublicClientApplication> _clientApplications = new();
        private readonly object _lock = new();
        private readonly string _cacheDirectory;
        private readonly ILogger<TokenCacheService> _logger;

        public TokenCacheService(IWebHostEnvironment environment, ILogger<TokenCacheService> logger)
        {
            _cacheDirectory = Path.Combine(environment.ContentRootPath, "token_cache");
            _logger = logger;

            // Ensure cache directory exists
            Directory.CreateDirectory(_cacheDirectory);
            _logger.LogInformation("Token cache directory initialized at: {CacheDirectory}", _cacheDirectory);
        }

        public IPublicClientApplication GetClientApplication(string clientId, string tenantId, string authority)
        {
            // Create a key for this client configuration
            string cacheKey = $"{clientId}_{tenantId}";
            
            // Check if we have a cached client application
            lock (_lock)
            {
                if (_clientApplications.TryGetValue(cacheKey, out var cachedApp))
                {
                    _logger.LogDebug("Using cached PublicClientApplication for {ClientId}", clientId);
                    return cachedApp;
                }
            }

            _logger.LogInformation("Creating new PublicClientApplication for {ClientId}", clientId);

            // Create a new client application
            var app = PublicClientApplicationBuilder
                .Create(clientId)
                .WithAuthority(authority)
                .WithRedirectUri("http://localhost")
                .Build();

            // Set up token cache with file persistence
            ConfigureTokenCache(app.UserTokenCache, cacheKey);

            // Cache the application
            lock (_lock)
            {
                _clientApplications[cacheKey] = app;
                return app;
            }
        }

        private void ConfigureTokenCache(ITokenCache cache, string cacheKey)
        {
            string cacheFilePath = Path.Combine(_cacheDirectory, $"{cacheKey}_cache.bin");
            _logger.LogDebug("Configuring token cache for {CacheKey} at {CacheFilePath}", cacheKey, cacheFilePath);

            // Set up cache serialization events
            cache.SetBeforeAccess(args =>
            {
                try
                {
                    if (File.Exists(cacheFilePath))
                    {
                        _logger.LogDebug("Loading token cache from file for {CacheKey}", cacheKey);
                        byte[] cacheData = File.ReadAllBytes(cacheFilePath);
                        args.TokenCache.DeserializeMsalV3(cacheData);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error loading token cache from file for {CacheKey}", cacheKey);
                }
            });

            cache.SetAfterAccess(args =>
            {
                try
                {
                    if (args.HasStateChanged)
                    {
                        _logger.LogDebug("Saving token cache to file for {CacheKey}", cacheKey);
                        byte[] cacheData = args.TokenCache.SerializeMsalV3();
                        File.WriteAllBytes(cacheFilePath, cacheData);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving token cache to file for {CacheKey}", cacheKey);
                }
            });
        }
    }
}