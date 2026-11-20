using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace backend.Test;

public class MarketEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public MarketEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Theory]
    [InlineData("success", 200, null)]
    [InlineData("http", 502, "Market data provider is unavailable.")]
    [InlineData("json", 502, "Market data provider is unavailable.")]
    [InlineData("timeout", 504, "Market data provider timed out.")]
    [InlineData("unexpected", 500, "An unexpected error occurred.")]
    public async Task Market_ReturnsExpectedResponse(
        string scenario, int expectedStatus, string? expectedTitle)
    {
        using var factory = _factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
                services.AddHttpClient<CoinMarketCapClient>(client =>
                    client.Timeout = TimeSpan.FromMilliseconds(100))
                    .ConfigurePrimaryHttpMessageHandler(() => scenario switch
                    {
                        "http" => new FakeHttpMessageHandler(
                            HttpStatusCode.ServiceUnavailable, "private provider detail"),
                        "json" => new FakeHttpMessageHandler(HttpStatusCode.OK, "not valid json"),
                        "timeout" => new SlowHttpMessageHandler(),
                        "unexpected" => new UnexpectedFailureHandler(),
                        _ => new FakeHttpMessageHandler(HttpStatusCode.OK,
                            """{"data":[{"id":1,"name":"Bitcoin","symbol":"BTC","cmc_rank":1,"quote":[{"symbol":"EUR","price":55000,"market_cap":1100000000000,"volume_24h":40000000000,"percent_change_24h":1.5,"last_updated":"2026-11-20T15:00:00Z"},{"symbol":"USD","price":60000.50,"market_cap":1200000000000,"volume_24h":53000000000.25,"percent_change_24h":-6.12,"last_updated":"2026-11-20T18:41:03.017+02:00"}]}]}""")
                    })));

        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });
        using var response = await client.GetAsync("/api/Market");

        Assert.Equal(expectedStatus, (int)response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);

        if (expectedStatus == 200)
        {
            var coin = Assert.Single(json.RootElement.GetProperty("data").EnumerateArray());
            Assert.Equal(1, coin.GetProperty("id").GetInt32());
            Assert.Equal("Bitcoin", coin.GetProperty("name").GetString());
            Assert.Equal("BTC", coin.GetProperty("symbol").GetString());
            Assert.Equal(60000.50m, coin.GetProperty("priceUsd").GetDecimal());
            Assert.Equal(1, coin.GetProperty("rank").GetInt32());
            Assert.Equal(1200000000000m, coin.GetProperty("marketCapUsd").GetDecimal());
            Assert.Equal(53000000000.25m, coin.GetProperty("volume24hUsd").GetDecimal());
            Assert.Equal(-6.12m, coin.GetProperty("change24hPercent").GetDecimal());
            var lastUpdated = coin.GetProperty("lastUpdatedUtc").GetDateTimeOffset();
            Assert.Equal(new DateTimeOffset(2026, 11, 20, 16, 41, 3, 17, TimeSpan.Zero), lastUpdated);
            Assert.Equal(TimeSpan.Zero, lastUpdated.Offset);
            Assert.False(coin.TryGetProperty("quote", out _));
        }
        else
        {
            Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
            Assert.Equal(expectedStatus, json.RootElement.GetProperty("status").GetInt32());
            Assert.Equal(expectedTitle, json.RootElement.GetProperty("title").GetString());
            Assert.DoesNotContain("private provider detail", body);
            Assert.False(json.RootElement.TryGetProperty("exception", out _));
            Assert.False(json.RootElement.TryGetProperty("detail", out _));
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Market_OmitsIncompleteCoins(bool includeCompleteCoin)
    {
        // This coin is complete except for the missing USD price.
        var coins = new List<object>
        {
            new
            {
                id = 1027,
                name = "Ethereum",
                symbol = "ETH",
                cmc_rank = 2,
                quote = new[]
                {
                    new
                    {
                        symbol = "USD",
                        market_cap = 200000000000m,
                        volume_24h = 10000000000m,
                        percent_change_24h = -2m,
                        last_updated = "2026-11-20T16:41:03Z"
                    }
                }
            }
        };

        if (includeCompleteCoin)
        {
            coins.Add(new
            {
                id = 1,
                name = "Bitcoin",
                symbol = "BTC",
                cmc_rank = 1,
                quote = new[]
                {
                    new
                    {
                        symbol = "USD",
                        price = 60000.50m,
                        market_cap = 1200000000000m,
                        volume_24h = 53000000000.25m,
                        percent_change_24h = -6.12m,
                        last_updated = "2026-11-20T16:41:03Z"
                    }
                }
            });
        }

        var providerJson = JsonSerializer.Serialize(new { data = coins });
        using var factory = _factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
                services.AddHttpClient<CoinMarketCapClient>()
                    .ConfigurePrimaryHttpMessageHandler(() =>
                        new FakeHttpMessageHandler(HttpStatusCode.OK, providerJson))));
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });
        using var response = await client.GetAsync("/api/Market");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var data = json.RootElement.GetProperty("data");
        Assert.Equal(JsonValueKind.Array, data.ValueKind);

        if (includeCompleteCoin)
        {
            var coin = Assert.Single(data.EnumerateArray());
            Assert.Equal(JsonValueKind.Object, coin.ValueKind);
            Assert.Equal(1, coin.GetProperty("id").GetInt32());
            Assert.Equal("BTC", coin.GetProperty("symbol").GetString());
            Assert.Equal(60000.50m, coin.GetProperty("priceUsd").GetDecimal());
        }
        else
        {
            Assert.Empty(data.EnumerateArray());
        }
    }

    private sealed class UnexpectedFailureHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            throw new InvalidOperationException("private provider detail");
        }
    }
}
