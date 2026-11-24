using FluentValidation;

namespace backend.Features.Markets.GetCoins;

public class GetMarketCoinsRequestValidator : AbstractValidator<GetMarketCoinsRequest>
{

    public GetMarketCoinsRequestValidator()
    {
        RuleFor(r => r.Page).GreaterThan(0).WithMessage("Page number must be set to a positive non-zero value");
        RuleFor(r => r.Limit).InclusiveBetween(1,50).WithMessage("Limit number must be between 1 and 50");
        RuleFor(r => r.Page)
            .Must((request, page) => ((long)page - 1) * request.Limit + 1 <= int.MaxValue)
            .WithMessage("Page is too large for the selected limit.")
            .When(request => request.Page > 0 && request.Limit >= 1 && request.Limit <= 50);
    }

}
