using System;

namespace backend.Infrastructure.ExternalApis.CoinMarketCap.Models;

public class CoinMarketCapListingsResponse
{
    public required List<CoinMarketCapCoin> Data { get; set; }
}
