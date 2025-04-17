# MSAL Credential Manager

A lightweight API for acquiring Microsoft Graph API access tokens using Microsoft Authentication Library (MSAL) with persistent token caching.

## Features

- Authenticates users with Microsoft Graph using interactive browser login
- Caches tokens to minimize authentication prompts
- Supports multiple client applications in a single instance
- Securely stores and manages refresh tokens
- Requires minimal configuration

## How It Works

The MSAL Credential Manager provides a simple REST API for acquiring tokens. When a client requests a token:

1. The API tries to find a cached token for the requested client and scope
2. If a valid token exists, it's returned immediately
3. If the token is expired but a refresh token exists, it's refreshed silently
4. If interactive login is required, a browser window opens for the user to authenticate
5. The new token is cached for future use and returned to the client

## API Endpoint

### POST /api/token

Authenticate with Microsoft Graph and get an access token.

#### Request:
```json
{
  "clientId": "YOUR_CLIENT_ID",
  "tenantId": "YOUR_TENANT_ID",
  "authority": "https://login.microsoftonline.com/YOUR_TENANT_ID",
  "scopes": [
    "https://graph.microsoft.com/User.Read",
    "https://graph.microsoft.com/Mail.Read"
  ]
}
```

- `clientId` and `tenantId` are required
- `authority` defaults to `https://login.microsoftonline.com/{tenantId}` if not provided
- `scopes` defaults to `["https://graph.microsoft.com/User.Read"]` if not provided

#### Response:
```json
{
  "username": "user@example.com",
  "accessToken": "eyJ0eXAi...",
  "expiresOn": "2023-04-17T12:00:00Z",
  "scopes": [
    "https://graph.microsoft.com/User.Read",
    "https://graph.microsoft.com/Mail.Read"
  ]
}
```

## Getting Started

1. Clone this repository
2. Build the solution: `dotnet build src/src.sln`
3. Run the API: `dotnet run --project src/Core.Infrastructure.WebApi/Core.Infrastructure.WebApi.csproj`
4. Access the Swagger UI at https://localhost:5001/swagger

## Requirements

- .NET 9.0 or higher
- Azure AD App Registration with appropriate permissions