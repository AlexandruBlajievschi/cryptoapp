using System.Net;
using System.Text.Json;
using backend.Features.Markets.Exceptions;
using backend.Infrastructure.ExternalApis.CoinMarketCap;
using backend.Test.TestDoubles.Http;

namespace backend.Test.Unit.Infrastructure.ExternalApis.CoinMarketCap;

public class CoinMarketCapClientTests
{

    public CoinMarketCapClientTests()
    {
    }

    [Fact]
    public async Task GetListingsAsync_ReturnsDeserializedCoins()
    {
        FakeHttpMessageHandler handler = new FakeHttpMessageHandler(HttpStatusCode.OK, """{"data":[{"id":1,"name":"Bitcoin","symbol":"BTC"}]}""");
        using var client = new HttpClient(handler);
        CoinMarketCapClient coinMarketCapClient = new CoinMarketCapClient(client);

        var listings = await coinMarketCapClient.GetListingsAsync(CancellationToken.None, 1 , 50);

        var coin = Assert.Single(listings.Data);
        Assert.Equal("BTC", coin.Symbol);
        Assert.Equal("Bitcoin", coin.Name);
        Assert.Equal(1, coin.Id);
    }

    [Fact]
    public async Task GetListingsAsync_ThrowsWhenProviderReturnsUnavailable()
    {
        FakeHttpMessageHandler handler = new FakeHttpMessageHandler(HttpStatusCode.ServiceUnavailable, """{"data":[{"id":1,"name":"Bitcoin","symbol":"BTC"}]}""");
        using var client = new HttpClient(handler);
        CoinMarketCapClient coinMarketCapClient = new CoinMarketCapClient(client);

        var exception = await Assert.ThrowsAsync<MarketDataProviderException>(async () =>
        {
            await coinMarketCapClient.GetListingsAsync(CancellationToken.None , 1 , 50);
        });

        var returnedException = Assert.IsType<HttpRequestException>(exception.InnerException);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, returnedException.StatusCode);
    }

    [Fact]
    public async Task GetListingsAsync_ThrowsWhenProviderReturnsInvalidJson()
    {
        FakeHttpMessageHandler handler = new FakeHttpMessageHandler(HttpStatusCode.OK, "not valid json");
        using var client = new HttpClient(handler);
        CoinMarketCapClient coinMarketCapClient = new CoinMarketCapClient(client);

        var exception = await Assert.ThrowsAsync<MarketDataProviderException>(async () =>
        {
            await coinMarketCapClient.GetListingsAsync(CancellationToken.None, 1 , 50);
        });

        Assert.IsType<JsonException>(exception.InnerException);

   }

    [Theory]
    [InlineData(null, null)]
    [InlineData("120", "120")]
    [InlineData("Wed, 25 Nov 2026 12:00:00 GMT", "Wed, 25 Nov 2026 12:00:00 GMT")]
    [InlineData("invalid value", null)]
    public async Task GetListingsAsync_ReportsRateLimitWithOptionalRetryAfter(
        string? providerRetryAfter, string? expectedRetryAfter)
    {
        var handler = new FakeHttpMessageHandler(
            HttpStatusCode.TooManyRequests, "private provider detail", providerRetryAfter);
        using var client = new HttpClient(handler);
        var marketClient = new CoinMarketCapClient(client);

        var exception = await Assert.ThrowsAsync<MarketDataProviderRateLimitException>(() =>
            marketClient.GetListingsAsync(CancellationToken.None, 1, 50));

        Assert.Equal(expectedRetryAfter, exception.RetryAfter);
    }

   [Fact]
   public async Task GetListingsAsync_ThrowsWhenProviderTimesOut()
   {
        SlowHttpMessageHandler slowHttpMessageHandler = new SlowHttpMessageHandler();
        using var client = new HttpClient(slowHttpMessageHandler);
        client.Timeout = TimeSpan.FromMilliseconds(100);
        CoinMarketCapClient coinMarketCapClient = new CoinMarketCapClient(client);
        var exception = await Assert.ThrowsAsync<MarketDataProviderTimeoutException>(async () =>
        {
            await coinMarketCapClient.GetListingsAsync(CancellationToken.None , 1 , 50);
        });

        Assert.IsType<OperationCanceledException>(exception.InnerException, exactMatch: false);

   }

}
