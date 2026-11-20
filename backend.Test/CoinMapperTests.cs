using backend.Features.Markets;
using backend.Infrastructure.ExternalApis.CoinMarketCap.Models;


namespace backend.Test;

public class CoinMapperTests
{

[Fact]
public void CoinDtoCreation_ReturnsNotNull()
{
    // Given
    var coin = new CoinMarketCapCoin
    {
        Id = 1,
        Name = "Quorse",
        Symbol = "QRS",
        CmcRank = 1,
        Quote = new List<CoinMarketCapQuote>{ new CoinMarketCapQuote
        {
            Symbol = "EUR",
            Price = 65000m,
            MarketCap = 23123123,
            Volume24h = 100m,
            PercentChange24h = 1.5m,
            LastUpdated = new DateTimeOffset(2026, 11, 20, 15, 0, 0, TimeSpan.Zero)
        },
        new CoinMarketCapQuote
        {
            Symbol = "USD",
            Price = 0,
            MarketCap = 23123123,
            Volume24h = 123456.78m,
            PercentChange24h = -6.12m,
            LastUpdated = new DateTimeOffset(2026, 11, 20, 18, 41, 3, TimeSpan.FromHours(2))
        }
        }
    };


    var coinDto = CoinMapper.ToDto(coin);
    Assert.NotNull(coinDto);
    Assert.Equal(1 , coinDto.Id);
    Assert.Equal("Quorse" , coinDto.Name);
    Assert.Equal("QRS" , coinDto.Symbol);
    Assert.Equal(0 , coinDto.PriceUsd);
    Assert.Equal(1 , coinDto.Rank);
    Assert.Equal(23123123 , coinDto.MarketCapUsd);
    Assert.Equal(123456.78m, coinDto.Volume24hUsd);
    Assert.Equal(-6.12m, coinDto.Change24hPercent);
    Assert.Equal(new DateTimeOffset(2026, 11, 20, 16, 41, 3, TimeSpan.Zero), coinDto.LastUpdatedUtc);
    Assert.Equal(TimeSpan.Zero, coinDto.LastUpdatedUtc.Offset);


}

[Theory]
[InlineData(null)]
[InlineData(-1)]
public void CoinDtoCreation_MissingPriceReturnsNull(int? price)
{
    var coin = new CoinMarketCapCoin
    {
        Id = 1,
        Name = "Quorse",
        Symbol = "QRS",
        CmcRank = 1,
        Quote = new List<CoinMarketCapQuote>{ new CoinMarketCapQuote
        {
            Symbol = "EUR",
            Price = 65000m
        },
        new CoinMarketCapQuote
        {
            Symbol = "USD",
            Price = price,
            MarketCap = 23123123m,
            Volume24h = 123456.78m,
            PercentChange24h = -6.12m,
            LastUpdated = new DateTimeOffset(2026, 11, 20, 16, 41, 3, TimeSpan.Zero)
        }
        }
    };

    var coinDto = CoinMapper.ToDto(coin);
    Assert.Null(coinDto);
}

[Theory]
[InlineData(null)]
[InlineData("")]
[InlineData("   ")]
public void CoinDtoCreation_MissingNameReturnsNull(string? name)
{
    var coin = new CoinMarketCapCoin
    {
        Id = 1,
        Name = name,
        Symbol = "QRS",
        CmcRank = 1,
        Quote = new List<CoinMarketCapQuote>{ new CoinMarketCapQuote
        {
            Symbol = "EUR",
            Price = 65000m
        },
        new CoinMarketCapQuote
        {
            Symbol = "USD",
            Price = 1000000m,
            MarketCap = 23123123m,
            Volume24h = 123456.78m,
            PercentChange24h = -6.12m,
            LastUpdated = new DateTimeOffset(2026, 11, 20, 16, 41, 3, TimeSpan.Zero)
        }
        }
    };

    var coinDto = CoinMapper.ToDto(coin);
    Assert.Null(coinDto);
}

[Theory]
[InlineData(true)]
[InlineData(false)]
public void CoinDtoCreation_MissingVolumeOrChangeReturnsNull(bool missingVolume)
{
    var coin = new CoinMarketCapCoin
    {
        Id = 1,
        Name = "Bitcoin",
        Symbol = "BTC",
        CmcRank = 1,
        Quote = new List<CoinMarketCapQuote>
        {
            new CoinMarketCapQuote
            {
                Symbol = "USD",
                Price = 60000m,
                MarketCap = 1200000000000m,
                Volume24h = missingVolume ? null : 123456.78m,
                PercentChange24h = missingVolume ? -6.12m : null,
                LastUpdated = new DateTimeOffset(2026, 11, 20, 16, 41, 3, TimeSpan.Zero)
            }
        }
    };

    Assert.Null(CoinMapper.ToDto(coin));
}

[Fact]
public void CoinDtoCreation_MissingTimestampReturnsNull()
{
    var coin = new CoinMarketCapCoin
    {
        Id = 1,
        Name = "Bitcoin",
        Symbol = "BTC",
        CmcRank = 1,
        Quote = new List<CoinMarketCapQuote>
        {
            new CoinMarketCapQuote
            {
                Symbol = "USD",
                Price = 60000m,
                MarketCap = 1200000000000m,
                Volume24h = 123456.78m,
                PercentChange24h = -6.12m,
                LastUpdated = null
            }
        }
    };

    Assert.Null(CoinMapper.ToDto(coin));
}
}
