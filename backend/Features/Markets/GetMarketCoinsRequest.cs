namespace backend.Features.Markets;

public class GetMarketCoinsRequest
{
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 50;
}
