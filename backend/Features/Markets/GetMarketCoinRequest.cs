using Microsoft.AspNetCore.Mvc;

namespace backend.Features.Markets;

public class GetMarketCoinRequest
{
    [FromRoute(Name = "id")]
    public int Id { get; set; }
}
