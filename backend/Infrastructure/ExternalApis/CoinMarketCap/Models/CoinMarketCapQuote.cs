using System.Text.Json.Serialization;

namespace backend.Infrastructure.ExternalApis.CoinMarketCap.Models;

public class CoinMarketCapQuote
{
    public decimal? Price {get; set;}
    public string? Symbol {get; set;}

    [JsonPropertyName("market_cap")]
    public decimal? MarketCap {get; set;}

    [JsonPropertyName("volume_24h")]
    public decimal? Volume24h { get; set; }

    [JsonPropertyName("percent_change_24h")]
    public decimal? PercentChange24h { get; set; }

    [JsonPropertyName("last_updated")]
    public DateTimeOffset? LastUpdated { get; set; }
}
