using System.Net;
using System.Text.Json;
using backend.ExceptionHandling;
using backend.Infrastructure.ExternalApis.CoinMarketCap.Models;

public class CoinMarketCapClient
{
    private readonly HttpClient _client;

    public CoinMarketCapClient(HttpClient client)
    {
        _client = client;
    }

    public async Task<CoinMarketCapListingsResponse> GetListingsAsync(CancellationToken cancellationToken, int page, int limit)
    {
        int start = (page - 1) * limit + 1;
        var url = $"https://pro-api.coinmarketcap.com/public-api/v3/cryptocurrency/listings/latest?start={start}&limit={limit}&convert=USD";

        return await GetResponseAsync(url, cancellationToken);
    }

    public async Task<CoinMarketCapCoin?> GetCoinByIdAsync(CancellationToken cancellationToken, int id)
    {
        var url = $"https://pro-api.coinmarketcap.com/public-api/v3/cryptocurrency/quotes/latest?id={id}&convert=USD&skip_invalid=true";
        var response = await GetResponseAsync(url, cancellationToken);

        return response.Data.FirstOrDefault(coin => coin.Id == id);
    }

    private async Task<CoinMarketCapListingsResponse> GetResponseAsync(string url, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await _client.GetAsync(url, cancellationToken);

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                // Only forward a valid, parsed Retry-After value supplied by the provider.
                throw new MarketDataProviderRateLimitException(response.Headers.RetryAfter?.ToString());
            }

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);

            var listings = JsonSerializer.Deserialize<CoinMarketCapListingsResponse>(
                json,
                JsonSerializerOptions.Web);

            if (listings is null)
            {
                throw new JsonException("CoinMarketCap returned a null response.");
            }

            return listings;

        } catch (HttpRequestException exception)
        {
            throw new MarketDataProviderException("Market data provider is unavailable." , exception);
        }
        catch(JsonException exception)
        {
            throw new MarketDataProviderException("Market data provider returned an invalid response." , exception);
        }
        catch(OperationCanceledException exception)
        when (!cancellationToken.IsCancellationRequested)
        {
            throw new MarketDataProviderTimeoutException("Market data provider timed out." , exception);
        }
        }

}
