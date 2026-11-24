using System.Net;
using System.Text.Json;
using backend.ExceptionHandling;

namespace backend.Test;

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
