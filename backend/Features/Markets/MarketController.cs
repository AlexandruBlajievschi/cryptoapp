using backend.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace backend.Features.Markets;

public class MarketController : BaseApiController
{
    private readonly MarketService _marketService;
    private readonly GetMarketCoinsRequestValidator _validationRules;
    private readonly GetMarketCoinRequestValidator _coinValidationRules;

    public MarketController(MarketService marketService, GetMarketCoinsRequestValidator validationRules,
        GetMarketCoinRequestValidator coinValidationRules)
    {
        _marketService = marketService;
        _validationRules = validationRules;
        _coinValidationRules = coinValidationRules;
    }

    [HttpGet("coins")]
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

    [HttpGet("coins/{id:int}")]
    public async Task<IActionResult> GetCoinById(
        [FromRoute] GetMarketCoinRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await _coinValidationRules.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(validationResult.ToDictionary()));
        }

        var coin = await _marketService.GetCoinByIdAsync(cancellationToken, request.Id);

        if (coin is null)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, title: "Coin not found.");
        }

        return Ok(new { data = coin });
    }
}
