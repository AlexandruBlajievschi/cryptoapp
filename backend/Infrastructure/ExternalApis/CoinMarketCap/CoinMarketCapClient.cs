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
        try
        {
            int start = (page -1 ) * limit + 1;

            var url = $"https://pro-api.coinmarketcap.com/public-api/v3/cryptocurrency/listings/latest?start={start}&limit={limit}&convert=USD";

            using var response = await _client.GetAsync(url, cancellationToken);

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
