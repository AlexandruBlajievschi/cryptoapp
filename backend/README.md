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

The response contains a `data` array of application-owned `CoinDto` objects,
mapped from provider models. The route remains temporary; the public market
endpoints and pagination are implemented in CG-007.

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

No secrets are required for the health or keyless market endpoints.

When database or external-provider credentials are introduced,
store them using ASP.NET Core user-secrets. Never commit actual
credentials to configuration files.

User-secrets is intended for local development and is not encrypted.
