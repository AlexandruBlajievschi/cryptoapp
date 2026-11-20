using System.Text.Json.Serialization;

namespace backend.Infrastructure.ExternalApis.CoinMarketCap.Models;

public class CoinMarketCapCoin
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Symbol { get; set; }
    public List<CoinMarketCapQuote>? Quote {get; set;}

    [JsonPropertyName("cmc_rank")]
    public int? CmcRank {get; set;}
}
