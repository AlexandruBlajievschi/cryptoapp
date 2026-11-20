using System;
using backend.Infrastructure.ExternalApis.CoinMarketCap.Models;

namespace backend.Features.Markets;

public class CoinMapper
{
    public static CoinDto? ToDto(CoinMarketCapCoin coin)
    {
        if(string.IsNullOrWhiteSpace(coin.Name) || string.IsNullOrWhiteSpace(coin.Symbol) || coin.CmcRank == null)
        {
            return null;
        }

        var quote = coin.Quote?.FirstOrDefault(q => q.Symbol=="USD");

        if (quote == null || quote.Price == null || quote.Price < 0 || quote.MarketCap == null
            || quote.Volume24h == null || quote.PercentChange24h == null || quote.LastUpdated == null)
        {
            return null;
        }


        return new CoinDto
        {
            Id = coin.Id,
            Name = coin.Name,
            Symbol = coin.Symbol,
            PriceUsd = quote.Price.Value,
            Rank = coin.CmcRank.Value,
            MarketCapUsd = quote.MarketCap.Value,
            Volume24hUsd = quote.Volume24h.Value,
            Change24hPercent = quote.PercentChange24h.Value,
            LastUpdatedUtc = quote.LastUpdated.Value.ToUniversalTime(),
        };
    }
}
