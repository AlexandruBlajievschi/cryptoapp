using FluentValidation;

namespace backend.Features.Markets;

public class GetMarketCoinRequestValidator : AbstractValidator<GetMarketCoinRequest>
{
    public GetMarketCoinRequestValidator()
    {
        RuleFor(request => request.Id)
            .GreaterThan(0)
            .WithMessage("Coin ID must be greater than zero.");
    }
}
