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
                            """{"data":[{"id":1,"name":"Bitcoin","symbol":"BTC"}]}""")
                    })));

        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });
        using var response = await client.GetAsync("/api/dev/market");

        Assert.Equal(expectedStatus, (int)response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);

        if (expectedStatus == 200)
        {
            var coin = Assert.Single(json.RootElement.GetProperty("data").EnumerateArray());
            Assert.Equal(1, coin.GetProperty("id").GetInt32());
            Assert.Equal("Bitcoin", coin.GetProperty("name").GetString());
            Assert.Equal("BTC", coin.GetProperty("symbol").GetString());
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

    private sealed class UnexpectedFailureHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            throw new InvalidOperationException("private provider detail");
        }
    }
}
