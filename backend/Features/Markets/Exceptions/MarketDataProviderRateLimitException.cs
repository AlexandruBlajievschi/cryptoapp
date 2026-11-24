namespace backend.Features.Markets.Exceptions;

public class MarketDataProviderRateLimitException : MarketDataProviderException
{
    public string? RetryAfter { get; }

    public MarketDataProviderRateLimitException(string? retryAfter)
        : base("Market data provider returned HTTP 429 Too Many Requests.")
    {
        RetryAfter = retryAfter;
    }
}
