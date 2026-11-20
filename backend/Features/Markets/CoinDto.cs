using System;

namespace backend.Features.Markets;

public class CoinDto
{
    public int Id {get; set;}
    public required string Name {get; set;}
    public required string Symbol {get; set;}
    public required decimal PriceUsd {get; set;}
    public required int Rank {get; set;}
    public required decimal MarketCapUsd {get; set;}
    public required decimal Volume24hUsd { get; set; }
    public required decimal Change24hPercent { get; set; }
    public required DateTimeOffset LastUpdatedUtc { get; set; }
}
