using backend.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace backend.Features.Markets;

public class MarketController : BaseApiController
{
    private readonly MarketService _marketService;

    public MarketController(MarketService marketService)
    {
        _marketService = marketService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCoins(CancellationToken cancellationToken)
    {
        var coins = await _marketService.GetCoinsAsync(cancellationToken);

        return Ok(new { data = coins });
    }
}
