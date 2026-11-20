## Backend development

Requires .NET 11 SDK. Run these commands from the repository root.

### Run the API

```powershell
dotnet run --project backend/backend.csproj --launch-profile http
```

Market endpoint: http://localhost:5123/api/Market

The previous `/api/health` endpoint and its tests have been removed.

### Run backend tests

```powershell
dotnet test backend.Test/backend.Test.csproj
```

Integration tests start the API in a test host; you do not need
to run the API separately.

### Market integration (CG-005)

With the API running, send `GET http://localhost:5123/api/Market`.
This endpoint requests the top 50 coins in USD from
CoinMarketCap's [keyless public API](https://coinmarketcap.com/api/documentation/pro-api-reference/keyless-public-api/).
No API key is required. A manual request needs internet access and consumes
provider quota; avoid repeatedly refreshing it.

The response contains a `data` array of application-owned `CoinDto` objects,
mapped from provider models. Pagination and coin-detail endpoints remain
outstanding CG-007 work.

### Controller and service separation

`MarketController` in `Features/Markets` handles HTTP and returns the existing
`{"data":[...]}` response. It inherits `[ApiController]` and
`[Route("api/[controller]")]` from the shared abstract `BaseApiController`.
The route token uses the concrete class name without the `Controller` suffix,
so `MarketController` maps to `/api/Market`. Actions declare their HTTP methods
with attributes such as `[HttpGet]`. Renaming a controller changes its route.

`MarketService` fetches provider listings, maps them into DTOs,
and filters incomplete coins. The controller receives the concrete service
through constructor injection. `CoinMarketCapClient` owns outgoing HTTP requests
and deserialization. The global exception handler still maps failures to
Problem Details responses.

`Program.cs` registers controllers using `AddControllers`, registers the scoped
market service, and maps controller routes using `MapControllers`. The request
flow is controller -> service -> provider client -> CoinMarketCap, with the
existing cancellation token passed through each layer. FluentValidation will
be introduced when request models need validation. The backlog is unchanged;
pagination and coin-detail endpoints are still outstanding CG-007 work.

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

### Market DTO mapping (CG-006)

Market feature code lives in `Features/Markets`; provider response models stay
in `Infrastructure/ExternalApis/CoinMarketCap/Models`. `CoinMapper` selects the
quote whose `symbol` is `USD` from the provider's `quote` array and maps it to
these JSON fields:

| Field | Source |
|---|---|
| `id` | Coin `id` |
| `name` | Coin `name` |
| `symbol` | Cryptocurrency symbol, such as `BTC` |
| `rank` | Coin `cmc_rank` |
| `priceUsd` | USD quote `price` |
| `marketCapUsd` | USD quote `market_cap` |
| `volume24hUsd` | USD quote `volume_24h` |
| `change24hPercent` | USD quote `percent_change_24h` |
| `lastUpdatedUtc` | USD quote `last_updated`, normalized to UTC |

Prices, market cap, volume and percentage change use `decimal`. Timestamps use
`DateTimeOffset` and are serialized as ISO 8601 with a UTC offset. The timestamp
is the provider's quote update time, not the time our backend fetched it.

**Chosen filtering rule:** returned DTOs have required, non-nullable name,
symbol, rank, price, market cap, volume, percentage change and update timestamp.
Provider models allow missing values so one incomplete coin does not prevent
other complete coins from being returned. The mapper skips a coin if its name
or symbol is blank, its rank is missing, its USD quote is missing, or any of
the required quote values is missing. Negative prices are rejected; zero
prices and negative percentage changes are allowed.

Incomplete records are omitted, never replaced with null list entries or
invented zero values. A successful provider response with no complete coins
returns HTTP 200 with `{"data":[]}`. HTTP failures, malformed JSON and timeouts
remain errors with the statuses documented above.

This strict output contract is the owner's chosen implementation decision;
the product backlog is unchanged. Mapper tests cover field selection, missing
data, negative/zero prices and UTC conversion. Endpoint tests verify the DTO
contract, mixed complete/incomplete results and all-incomplete results without
live provider requests.

### Local secrets

No secrets are required for the keyless market endpoint.

When database or external-provider credentials are introduced,
store them using ASP.NET Core user-secrets. Never commit actual
credentials to configuration files.

User-secrets is intended for local development and is not encrypted.
