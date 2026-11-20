## Backend development

Requires .NET 11 SDK. Run these commands from the repository root.

### Run the API

```powershell
dotnet run --project backend/backend.csproj --launch-profile http
```

Health endpoint: http://localhost:5123/api/health

Expected response: HTTP 200 with `{"status":"healthy"}`.

### Run backend tests

```powershell
dotnet test backend.Test/backend.Test.csproj
```

Integration tests start the API in a test host; you do not need
to run the API separately.

### Market integration (CG-005)

With the API running, send `GET http://localhost:5123/api/dev/market`.
This temporary inspection endpoint requests the top 50 coins in USD from
CoinMarketCap's [keyless public API](https://coinmarketcap.com/api/documentation/pro-api-reference/keyless-public-api/).
No API key is required. A manual request needs internet access and consumes
provider quota; avoid repeatedly refreshing it.

The response currently contains a `data` array with each coin's `id`, `name`
and `symbol`. This is a provider model for development inspection; public
market DTOs and endpoints follow in CG-006 and CG-007.

The typed HTTP client has a 10-second timeout and forwards request cancellation.
The client wraps HTTP/network and JSON failures in `MarketDataProviderException`,
and timeouts in `MarketDataProviderTimeoutException`, preserving the original
exception as `InnerException`. Caller cancellation is not wrapped as a timeout.

The global exception handler writes Problem Details JSON with these statuses:

| Failure | HTTP status |
|---|---|
| Provider HTTP/network failure or malformed JSON | 502 Bad Gateway |
| Provider timeout | 504 Gateway Timeout |
| Unexpected application exception | 500 Internal Server Error |

Responses use fixed titles and do not expose exception messages or stack traces.
Client tests and market endpoint integration tests use fake HTTP handlers;
they do not call CoinMarketCap or consume its quota. They cover successful
deserialization, HTTP failure, malformed JSON and timeout. Endpoint tests also
check the generic 500 fallback and Problem Details responses.

### Local secrets

No secrets are required for the health or keyless market endpoints.

When database or external-provider credentials are introduced,
store them using ASP.NET Core user-secrets. Never commit actual
credentials to configuration files.

User-secrets is intended for local development and is not encrypted.
