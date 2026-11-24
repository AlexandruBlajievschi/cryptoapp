using backend.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace backend.Features.Markets;

public class MarketController : BaseApiController
{
    private readonly MarketService _marketService;
    private readonly GetMarketCoinsRequestValidator _validationRules;

    public MarketController(MarketService marketService , GetMarketCoinsRequestValidator validationRules)
    {
        _marketService = marketService;
        _validationRules = validationRules;
    }

    [HttpGet]
    public async Task<IActionResult> GetCoins(CancellationToken cancellationToken, [FromQuery] GetMarketCoinsRequest getMarketCoinsRequest)
    {
        int page = getMarketCoinsRequest.Page;
        int limit = getMarketCoinsRequest.Limit;
        var validationResult = await _validationRules.ValidateAsync(getMarketCoinsRequest , cancellationToken);

        if (!validationResult.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(validationResult.ToDictionary()));
        }
        var coins = await _marketService.GetCoinsAsync(cancellationToken, page, limit);

        return Ok(new { data = coins });
    }
}
