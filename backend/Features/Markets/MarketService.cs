using backend.Features.Markets.Contracts;
using backend.Features.Markets.Exceptions;
using backend.Features.Markets.Mapping;
using backend.Infrastructure.ExternalApis.CoinMarketCap;

namespace backend.Features.Markets;

public class MarketService
{
    private readonly CoinMarketCapClient _marketClient;

    public MarketService(CoinMarketCapClient marketClient)
    {
        _marketClient = marketClient;
    }

    public async Task<IReadOnlyList<CoinDto>> GetCoinsAsync(CancellationToken cancellationToken, int page , int limit)
    {
        var listings = await _marketClient.GetListingsAsync(cancellationToken, page , limit);

        return listings.Data
            .Select(CoinMapper.ToDto)
            .OfType<CoinDto>()
            .ToList();
    }

    public async Task<CoinDto?> GetCoinByIdAsync(CancellationToken cancellationToken, int id)
    {
        var coin = await _marketClient.GetCoinByIdAsync(cancellationToken, id);

        if (coin is null)
        {
            return null;
        }

        var coinDto = CoinMapper.ToDto(coin);

        if (coinDto is null)
        {
            throw new MarketDataProviderException(
                "Market data provider returned an incomplete coin.");
        }

        return coinDto;
    }
}
