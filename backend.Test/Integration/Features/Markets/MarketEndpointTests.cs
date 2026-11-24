using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using backend.Infrastructure.ExternalApis.CoinMarketCap;
using backend.Test.TestDoubles.Http;

namespace backend.Test.Integration.Features.Markets;

public class MarketEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    // Provider-shaped JSON, kept separate from the test steps for readability.
    private const string CompleteCoin = """
        {"id":1,"name":"Bitcoin","symbol":"BTC","cmc_rank":1,
         "quote":[
           {"symbol":"EUR","price":55000,"market_cap":1100000000000,
            "volume_24h":40000000000,"percent_change_24h":1.5,"last_updated":"2026-11-20T15:00:00Z"},
           {"symbol":"USD","price":60000.50,"market_cap":1200000000000,
            "volume_24h":53000000000.25,"percent_change_24h":-6.12,"last_updated":"2026-11-20T18:41:03.017+02:00"}]}
        """;

    // Only the USD price is missing.
    private const string IncompleteCoin = """
        {"id":1027,"name":"Ethereum","symbol":"ETH","cmc_rank":2,
         "quote":[{"symbol":"USD","market_cap":200000000000,"volume_24h":10000000000,
                   "percent_change_24h":-2,"last_updated":"2026-11-20T16:41:03Z"}]}
        """;

    public MarketEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetMarket_ReturnsMappedCoins()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, Listings(CompleteCoin));
        using var factory = CreateFactory(handler);
        using var client = CreateClient(factory);

        // Act
        using var response = await client.GetAsync("/api/market/coins");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var coin = Assert.Single(json.RootElement.GetProperty("data").EnumerateArray());
        Assert.Equal(1, coin.GetProperty("id").GetInt32());
        Assert.Equal("Bitcoin", coin.GetProperty("name").GetString());
        Assert.Equal("BTC", coin.GetProperty("symbol").GetString());
        Assert.Equal(60000.50m, coin.GetProperty("priceUsd").GetDecimal());
        Assert.Equal(1, coin.GetProperty("rank").GetInt32());
        Assert.Equal(1200000000000m, coin.GetProperty("marketCapUsd").GetDecimal());
        Assert.Equal(53000000000.25m, coin.GetProperty("volume24hUsd").GetDecimal());
        Assert.Equal(-6.12m, coin.GetProperty("change24hPercent").GetDecimal());
        var updated = coin.GetProperty("lastUpdatedUtc").GetDateTimeOffset();
        Assert.Equal(new DateTimeOffset(2026, 11, 20, 16, 41, 3, 17, TimeSpan.Zero), updated);
        Assert.Equal(TimeSpan.Zero, updated.Offset);
        Assert.False(coin.TryGetProperty("quote", out _));
        var query = QueryHelpers.ParseQuery(handler.LastRequestUri!.Query);
        Assert.Equal("1", query["start"].ToString());
        Assert.Equal("50", query["limit"].ToString());
    }

    [Theory]
    [InlineData(1, 1, 1)]
    [InlineData(1, 50, 1)]
    [InlineData(2, 10, 11)]
    [InlineData(101, 10, 1001)]
    [InlineData(42949673, 50, 2147483601)]
    public async Task GetMarket_PassesPaginationToProvider(int page, int limit, int expectedStart)
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, Listings());
        using var factory = CreateFactory(handler);
        using var client = CreateClient(factory);

        // Act
        using var response = await client.GetAsync($"/api/market/coins?page={page}&limit={limit}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(handler.LastRequestUri);
        var query = QueryHelpers.ParseQuery(handler.LastRequestUri.Query);
        Assert.Equal(expectedStart.ToString(), query["start"].ToString());
        Assert.Equal(limit.ToString(), query["limit"].ToString());
        Assert.Equal("USD", query["convert"].ToString());
    }

    [Theory]
    [InlineData("page=0&limit=10", "Page")]
    [InlineData("page=-1&limit=10", "Page")]
    [InlineData("page=1&limit=0", "Limit")]
    [InlineData("page=1&limit=-1", "Limit")]
    [InlineData("page=1&limit=51", "Limit")]
    [InlineData("page=abc&limit=10", "Page")]
    [InlineData("page=1&limit=abc", "Limit")]
    [InlineData("page=42949674&limit=50", "Page")]
    [InlineData("page=2147483647&limit=50", "Page")]
    public async Task GetMarket_Returns400WithoutCallingProviderForInvalidPagination(
        string query, string propertyName)
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, Listings());
        using var factory = CreateFactory(handler);
        using var client = CreateClient(factory);

        // Act
        using var response = await client.GetAsync($"/api/market/coins?{query}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(400, json.RootElement.GetProperty("status").GetInt32());
        var errors = json.RootElement.GetProperty("errors").GetProperty(propertyName);
        Assert.NotEmpty(errors.EnumerateArray());
        Assert.Null(handler.LastRequestUri);
    }

    [Fact]
    public async Task GetMarket_OmitsIncompleteCoins()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, Listings(IncompleteCoin, CompleteCoin));
        using var factory = CreateFactory(handler);
        using var client = CreateClient(factory);

        using var response = await client.GetAsync("/api/market/coins");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var coin = Assert.Single(json.RootElement.GetProperty("data").EnumerateArray());
        Assert.Equal(JsonValueKind.Object, coin.ValueKind);
        Assert.Equal(1, coin.GetProperty("id").GetInt32());
        Assert.Equal("BTC", coin.GetProperty("symbol").GetString());
        Assert.Equal(60000.50m, coin.GetProperty("priceUsd").GetDecimal());
    }

    [Fact]
    public async Task GetMarket_ReturnsEmptyArrayWhenAllCoinsAreIncomplete()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, Listings(IncompleteCoin));
        using var factory = CreateFactory(handler);
        using var client = CreateClient(factory);

        using var response = await client.GetAsync("/api/market/coins");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Empty(json.RootElement.GetProperty("data").EnumerateArray());
    }

    [Fact]
    public async Task GetMarket_Returns502WhenProviderIsUnavailable()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.ServiceUnavailable, "private provider detail");
        using var factory = CreateFactory(handler);
        using var client = CreateClient(factory);

        using var response = await client.GetAsync("/api/market/coins");

        await AssertProblemResponse(response, HttpStatusCode.BadGateway, "Market data provider is unavailable.");
    }

    [Fact]
    public async Task GetMarket_Returns502WhenProviderJsonIsInvalid()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, "not valid json");
        using var factory = CreateFactory(handler);
        using var client = CreateClient(factory);

        using var response = await client.GetAsync("/api/market/coins");

        await AssertProblemResponse(response, HttpStatusCode.BadGateway, "Market data provider is unavailable.");
    }

    [Fact]
    public async Task GetMarket_Returns504WhenProviderTimesOut()
    {
        using var factory = CreateFactory(new SlowHttpMessageHandler());
        using var client = CreateClient(factory);

        using var response = await client.GetAsync("/api/market/coins");

        await AssertProblemResponse(response, HttpStatusCode.GatewayTimeout, "Market data provider timed out.");
    }

    [Fact]
    public async Task GetMarket_Returns500WhenUnexpectedErrorOccurs()
    {
        using var factory = CreateFactory(new UnexpectedFailureHandler());
        using var client = CreateClient(factory);

        using var response = await client.GetAsync("/api/market/coins");

        await AssertProblemResponse(response, HttpStatusCode.InternalServerError, "An unexpected error occurred.");
    }

    [Fact]
    public async Task GetCoinById_ReturnsCoinUsingIdRatherThanRank()
    {
        // Arrange: this coin has ID 2 and a different market rank.
        var litecoin = CompleteCoin.Replace("\"id\":1", "\"id\":2")
            .Replace("Bitcoin", "Litecoin").Replace("BTC", "LTC")
            .Replace("\"cmc_rank\":1", "\"cmc_rank\":12");
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, Listings(litecoin));
        using var factory = CreateFactory(handler);
        using var client = CreateClient(factory);

        // Act
        using var response = await client.GetAsync("/api/market/coins/2");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var coin = json.RootElement.GetProperty("data");
        Assert.Equal(2, coin.GetProperty("id").GetInt32());
        Assert.Equal("LTC", coin.GetProperty("symbol").GetString());
        Assert.Equal(12, coin.GetProperty("rank").GetInt32());
        Assert.NotNull(handler.LastRequestUri);
        Assert.EndsWith("/cryptocurrency/quotes/latest", handler.LastRequestUri.AbsolutePath);
        var query = QueryHelpers.ParseQuery(handler.LastRequestUri.Query);
        Assert.Equal("2", query["id"].ToString());
        Assert.Equal("USD", query["convert"].ToString());
        Assert.Equal("true", query["skip_invalid"].ToString());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetCoinById_RejectsInvalidIdWithoutCallingProvider(int id)
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, Listings());
        using var factory = CreateFactory(handler);
        using var client = CreateClient(factory);

        using var response = await client.GetAsync($"/api/market/coins/{id}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(json.RootElement.GetProperty("errors").TryGetProperty("Id", out _));
        Assert.Null(handler.LastRequestUri);
    }

    [Fact]
    public async Task GetCoinById_Returns404WhenCoinIsAbsent()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, Listings());
        using var factory = CreateFactory(handler);
        using var client = CreateClient(factory);

        using var response = await client.GetAsync("/api/market/coins/99999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetCoinById_Returns502WhenProviderFails()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.ServiceUnavailable, "unavailable");
        using var factory = CreateFactory(handler);
        using var client = CreateClient(factory);

        using var response = await client.GetAsync("/api/market/coins/1");

        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
    }

    [Fact]
    public async Task CanonicalMarketRoutes_ReturnListingAndCoin()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, Listings(CompleteCoin));
        using var factory = CreateFactory(handler);
        using var client = CreateClient(factory);

        using var listing = await client.GetAsync("/api/market/coins?page=1&limit=50");
        using var detail = await client.GetAsync("/api/market/coins/1");

        Assert.Equal(HttpStatusCode.OK, listing.StatusCode);
        Assert.Equal(HttpStatusCode.OK, detail.StatusCode);
        using var listingJson = JsonDocument.Parse(await listing.Content.ReadAsStringAsync());
        using var detailJson = JsonDocument.Parse(await detail.Content.ReadAsStringAsync());
        Assert.Single(listingJson.RootElement.GetProperty("data").EnumerateArray());
        Assert.Equal(1, detailJson.RootElement.GetProperty("data").GetProperty("id").GetInt32());
    }

    [Fact]
    public async Task GetCoinById_Returns502WhenCoinDataIsIncomplete()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, Listings(IncompleteCoin));
        using var factory = CreateFactory(handler);
        using var client = CreateClient(factory);

        using var response = await client.GetAsync("/api/market/coins/1027");

        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
    }

    [Fact]
    public async Task OpenApi_IncludesMarketRoutesAndPagination()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, Listings());
        using var factory = CreateFactory(handler);
        using var client = CreateClient(factory);

        using var response = await client.GetAsync("/openapi/v1.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var paths = json.RootElement.GetProperty("paths");
        Assert.Equal(2, paths.EnumerateObject().Count());
        var listing = paths.GetProperty("/api/Market/coins").GetProperty("get");
        var detail = paths.GetProperty("/api/Market/coins/{id}").GetProperty("get");
        var queryNames = listing.GetProperty("parameters").EnumerateArray()
            .Select(parameter => parameter.GetProperty("name").GetString()).ToList();
        Assert.Contains("Page", queryNames);
        Assert.Contains("Limit", queryNames);
        Assert.Contains(detail.GetProperty("parameters").EnumerateArray(),
            parameter => parameter.GetProperty("name").GetString() == "id"
                && parameter.GetProperty("in").GetString() == "path");
        Assert.Null(handler.LastRequestUri);
    }

    [Theory]
    [InlineData("/api/market/coins", null)]
    [InlineData("/api/market/coins", "120")]
    [InlineData("/api/market/coins/1", null)]
    [InlineData("/api/market/coins/1", "Wed, 25 Nov 2026 12:00:00 GMT")]
    public async Task Market_Returns503WhenProviderRateLimits(string url, string? retryAfter)
    {
        var handler = new FakeHttpMessageHandler(
            HttpStatusCode.TooManyRequests, "private provider detail", retryAfter);
        using var factory = CreateFactory(handler);
        using var client = CreateClient(factory);

        using var response = await client.GetAsync(url);

        await AssertProblemResponse(response, HttpStatusCode.ServiceUnavailable,
            "Market data is temporarily unavailable. Please try again later.");
        Assert.Equal(retryAfter, response.Headers.RetryAfter?.ToString());
        Assert.Equal(retryAfter is not null, response.Headers.Contains("Retry-After"));
    }

    // These helpers only set up the test host or repeat common assertions.
    private WebApplicationFactory<Program> CreateFactory(HttpMessageHandler handler)
    {
        return _factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
                services.AddHttpClient<CoinMarketCapClient>(client =>
                    client.Timeout = TimeSpan.FromMilliseconds(100))
                    .ConfigurePrimaryHttpMessageHandler(() => handler)));
    }

    private static HttpClient CreateClient(WebApplicationFactory<Program> factory)
    {
        return factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });
    }

    private static string Listings(params string[] coins)
    {
        return "{\"data\":[" + string.Join(",", coins) + "]}";
    }

    private static async Task AssertProblemResponse(HttpResponseMessage response, HttpStatusCode status, string title)
    {
        Assert.Equal(status, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var body = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);
        Assert.Equal((int)status, json.RootElement.GetProperty("status").GetInt32());
        Assert.Equal(title, json.RootElement.GetProperty("title").GetString());
        Assert.DoesNotContain("private provider detail", body);
        Assert.False(json.RootElement.TryGetProperty("exception", out _));
        Assert.False(json.RootElement.TryGetProperty("detail", out _));
    }

    private sealed class UnexpectedFailureHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            throw new InvalidOperationException("private provider detail");
        }
    }
}
