using System;

namespace backend.Infrastructure.ExternalApis.CoinMarketCap.Models;

public class CoinMarketCapCoin
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Symbol { get; set; }

}
