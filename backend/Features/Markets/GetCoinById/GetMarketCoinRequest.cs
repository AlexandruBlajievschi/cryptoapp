using Microsoft.AspNetCore.Mvc;

namespace backend.Features.Markets.GetCoinById;

public class GetMarketCoinRequest
{
    [FromRoute(Name = "id")]
    public int Id { get; set; }
}
