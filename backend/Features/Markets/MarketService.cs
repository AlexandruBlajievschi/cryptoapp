namespace backend.Features.Markets;

public class MarketService
{
    private readonly CoinMarketCapClient _marketClient;

    public MarketService(CoinMarketCapClient marketClient)
    {
        _marketClient = marketClient;
    }

    public async Task<IReadOnlyList<CoinDto>> GetCoinsAsync(CancellationToken cancellationToken)
    {
        var listings = await _marketClient.GetListingsAsync(cancellationToken);

        return listings.Data
            .Select(CoinMapper.ToDto)
            .OfType<CoinDto>()
            .ToList();
    }
}
