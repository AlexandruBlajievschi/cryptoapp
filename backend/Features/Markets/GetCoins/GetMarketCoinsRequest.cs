namespace backend.Features.Markets.GetCoins;

public class GetMarketCoinsRequest
{
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 50;
}
