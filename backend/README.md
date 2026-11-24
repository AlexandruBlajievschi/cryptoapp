## Backend development

Requires .NET 11 SDK. Run these commands from the repository root.

Local PostgreSQL is configured separately using the root `docker-compose.yml`.
See [database setup and persistence checks](../docs/Local_PostgreSQL.md).
The API is not connected to PostgreSQL yet, and existing API tests do not require
Docker. EF Core, the connection string and migrations are a subsequent task.

### Run the API

```powershell
dotnet run --project backend/backend.csproj --launch-profile http
```

Market endpoint: http://localhost:5123/api/market/coins

### Run backend tests

```powershell
dotnet test backend.Test/backend.Test.csproj
```

Integration tests start the API in a test host; you do not need
to run the API separately.

### Code organization

`Features/Markets` groups application code by market use case:

| Location | Responsibility |
|---|---|
| `MarketController.cs` | Incoming HTTP requests and responses |
| `MarketService.cs` | Fetching, mapping and filtering workflow |
| `GetCoins/` | Listing request and pagination validator |
| `GetCoinById/` | Single-coin request and ID validator |
| `Contracts/` | Application response DTOs |
| `Mapping/` | Provider-to-DTO conversion |
| `Exceptions/` | Market-provider failure types |

The shared `Controllers/BaseApiController.cs` supplies API conventions.
`ExceptionHandling/GlobalExceptionHandler.cs` converts uncaught exceptions into
HTTP responses. Provider HTTP clients and their JSON models live under
`Infrastructure/ExternalApis/CoinMarketCap`; they are separate from API DTOs.
Namespaces follow the folders, including the provider client namespace.

The test project keeps isolated class tests under `backend.Test/Unit`, HTTP
test-host checks under `backend.Test/Integration`, and shared fake/slow HTTP
handlers under `backend.Test/TestDoubles/Http`. Feature and infrastructure test
folders mirror their backend counterparts. The endpoint test's private
unexpected-failure handler stays nested because only that test class uses it.

Manual requests are grouped in `docs/api` at the repository root:
[Market.http](../docs/api/Market.http) and the optional
[Postman collection](../docs/api/CryptoGuide.postman_collection.json).

### Market integration (CG-005)

With the API running, send `GET http://localhost:5123/api/market/coins`.
This endpoint requests the top 50 coins in USD from
CoinMarketCap's [keyless public API](https://coinmarketcap.com/api/documentation/pro-api-reference/keyless-public-api/).
No API key is required. A manual request needs internet access and consumes
provider quota; avoid repeatedly refreshing it.

The response contains a `data` array of application-owned `CoinDto` objects,
mapped from provider models. Pagination and fetching a single coin by ID are
supported.

### Controller and service separation

`MarketController` in `Features/Markets` handles HTTP and returns the existing
`{"data":[...]}` response. It inherits `[ApiController]` and
`[Route("api/[controller]")]` from the shared abstract `BaseApiController`.
The route token uses the concrete class name without the `Controller` suffix,
Actions each declare one `[HttpGet]` route: `coins` for the list and
`coins/{id:int}` for details. Combined with the inherited controller prefix,
the endpoints are `/api/market/coins` and `/api/market/coins/{id}`.
ASP.NET Core route matching is case-insensitive. Renaming a controller changes
its inherited route prefix.

`MarketService` fetches provider listings, maps them into DTOs,
and filters incomplete coins. The controller receives the concrete service
through constructor injection. `CoinMarketCapClient` owns outgoing HTTP requests
and deserialization. The global exception handler still maps failures to
Problem Details responses.

`Program.cs` registers controllers using `AddControllers`, registers the scoped
market service, and maps controller routes using `MapControllers`. The request
flow is controller -> service -> provider client -> CoinMarketCap, with the
existing cancellation token passed through each layer. FluentValidation checks
pagination and coin-ID requests before the service is called. The backlog is
unchanged. Routes currently follow the inherited `/api/[controller]` convention.

### Pagination and validation (CG-007)

`GET /api/market/coins?page=2&limit=10` requests the second page of ten provider records.
Omitted parameters default to page 1 and limit 50. `Page` must be positive and
`Limit` must be between 1 and 50. The provider offset is calculated as
`start = (page - 1) * limit + 1`, so page 2 with limit 10 sends start 11.

`GetMarketCoinsRequestValidator` is registered as a concrete scoped service and
called explicitly using `ValidateAsync` in the controller. Invalid requests
return HTTP 400 Problem Details with errors grouped by property, without calling
the provider. Nonnumeric query values are rejected by ASP.NET Core model binding.

The validator uses `long` arithmetic to reject page/limit combinations whose
calculated start exceeds `int.MaxValue`. These are numeric safety limits, not
a guarantee that the provider contains records for every accepted page.
Incomplete coins are filtered within the requested provider page; the API does
not fetch extra pages to replace them, so fewer than `limit` coins may be returned.

Tests cover validator rules, query defaults, pagination forwarding, overflow
boundaries and rejection before any outbound request. Tests use fake responses.

### Single coin by ID

`GET /api/market/coins/1` returns one mapped coin under `data`, using its CMC ID rather
than its market rank. IDs identify coins; market ranks may change. The controller
binds `GetMarketCoinRequest` from the route and explicitly runs its validator.
Zero and negative IDs return HTTP 400 with an `Id` validation error before any
provider call. The `int` route constraint excludes noninteger paths.

The service calls the client's `/v3/cryptocurrency/quotes/latest` integration
with `id`, `convert=USD` and `skip_invalid=true`. The existing response models
and mapper are reused. A successful provider response with no matching ID
returns HTTP 404. An existing coin missing required quote fields returns 502,
since unavailable data does not establish that the coin does not exist.
Provider rate limits return 503; other HTTP/JSON failures return 502 and timeouts
return 504.

Focused endpoint tests cover ID versus rank, zero/negative IDs, an absent coin
and a provider failure, using fake responses rather than live API calls.

### CG-007 acceptance verification

The automated suite verifies listing responses, query defaults, pagination
boundaries, invalid requests making no provider call, detail lookup by ID,
404 for an empty successful lookup, 502 for provider failures or incomplete
detail data, and OpenAPI discovery of both backlog routes and their parameters.
The service's incomplete-data error has no artificial inner JSON parsing error;
HTTP and actual JSON exceptions are still preserved by the provider client.

Swagger UI is available at `/swagger` and its OpenAPI document at
`/openapi/v1.json` in Development. Import
[`docs/api/CryptoGuide.postman_collection.json`](../docs/api/CryptoGuide.postman_collection.json)
into Postman and run it against the current API for the explicit manual
acceptance check. The collection includes success, invalid page, invalid ID
and unknown-ID cases. Successful reads and the unknown-ID check call the live
provider; provider rate limits or outages can produce 502/503/504 instead of the expected
success or not-found statuses. Unknown-ID behavior relies on the provider's
`skip_invalid=true` handling and should be confirmed in this live check.

Postman execution has not been performed by the coding agent. Automated
verification does not constitute recorded manual Postman evidence; this final
acceptance step remains for the developer before marking CG-007 Done.

The typed HTTP client has a 10-second timeout and forwards request cancellation.
The client wraps HTTP/network and JSON failures in `MarketDataProviderException`,
and timeouts in `MarketDataProviderTimeoutException`, preserving the original
exception as `InnerException`. Caller cancellation is not wrapped as a timeout.

The global exception handler writes Problem Details JSON with these statuses:

| Failure | HTTP status |
|---|---|
| Provider rate limit (HTTP 429) | 503 Service Unavailable |
| Provider HTTP/network failure or malformed JSON | 502 Bad Gateway |
| Provider timeout | 504 Gateway Timeout |
| Unexpected application exception | 500 Internal Server Error |

Provider 429 responses are detected before `EnsureSuccessStatusCode` and become
`MarketDataProviderRateLimitException`. The global handler logs the upstream
429 and returns the fixed title "Market data is temporarily unavailable. Please
try again later." for both listing and detail endpoints. The raw provider error
body is not returned. A valid provider `Retry-After` header (seconds or an HTTP
date) is forwarded; missing or invalid values do not produce a retry header.
The API does not invent a reset time or automatically retry these requests.

Responses use fixed titles and do not expose exception messages or stack traces.
Client tests and market endpoint integration tests use fake HTTP handlers;
they do not call CoinMarketCap or consume its quota. They cover successful
deserialization, HTTP failure, malformed JSON and timeout. Endpoint tests also
check the generic 500 fallback and Problem Details responses.

### Market DTO mapping (CG-006)

`CoinDto` lives in `Features/Markets/Contracts` and `CoinMapper` in
`Features/Markets/Mapping`; provider response models stay in
`Infrastructure/ExternalApis/CoinMarketCap/Models`. `CoinMapper` selects the
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
