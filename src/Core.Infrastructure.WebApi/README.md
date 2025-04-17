# MSAL Credential Manager API

This API provides authentication services for Microsoft Graph using Microsoft Authentication Library (MSAL) with persistent token caching.

## Token Caching

The API implements file-based token caching to persist authentication tokens between requests:

- Tokens are cached based on client ID and tenant ID combinations
- Cache files are stored in the `token_cache` directory
- Multiple different clients and tenants can be used without interference
- Interactive login is only required when tokens expire or new scopes are requested

## Endpoint

### POST /api/token
Authenticates a user with Microsoft Graph using the settings provided in the request body and returns an access token.

#### Request Body
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

All fields except `clientId` and `tenantId` are optional:
- If `authority` is not provided, it will be constructed as `https://login.microsoftonline.com/{tenantId}`
- If `scopes` is not provided, it will default to `["https://graph.microsoft.com/User.Read"]`

#### Response
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

## Setup

1. Run the application:
   ```
   dotnet run
   ```

2. Access the Swagger UI:
   https://localhost:5001/swagger

## Using the API

```bash
curl -X POST https://localhost:5001/api/token \
  -H "Content-Type: application/json" \
  -d '{
    "clientId": "YOUR_CLIENT_ID",
    "tenantId": "YOUR_TENANT_ID",
    "scopes": ["https://graph.microsoft.com/User.Read"]
  }'
```

The browser will open for authentication. After successful authentication, the API will return the token information as JSON.