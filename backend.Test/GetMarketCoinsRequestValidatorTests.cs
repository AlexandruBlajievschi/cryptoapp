using backend.Features.Markets;

namespace backend.Test;

public class GetMarketCoinsRequestValidatorTests
{
    [Theory]
    [InlineData(1, 50)]
    [InlineData(2, 10)]
    [InlineData(101, 10)]
    [InlineData(1, 1)]
    [InlineData(42949673, 50)]
    [InlineData(int.MaxValue, 1)]
    public async Task Validate_AcceptsValidPagination(int page, int limit)
    {
        // Arrange
        var validator = new GetMarketCoinsRequestValidator();
        var request = new GetMarketCoinsRequest { Page = page, Limit = limit };

        // Act
        var result = await validator.ValidateAsync(request);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Theory]
    [InlineData(0, 10, "Page")]
    [InlineData(-1, 10, "Page")]
    [InlineData(1, 0, "Limit")]
    [InlineData(1, -1, "Limit")]
    [InlineData(1, 51, "Limit")]
    public async Task Validate_RejectsInvalidPagination(int page, int limit, string propertyName)
    {
        // Arrange
        var validator = new GetMarketCoinsRequestValidator();
        var request = new GetMarketCoinsRequest { Page = page, Limit = limit };

        // Act
        var result = await validator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        var error = Assert.Single(result.Errors);
        Assert.Equal(propertyName, error.PropertyName);
        Assert.False(string.IsNullOrWhiteSpace(error.ErrorMessage));
    }

    [Theory]
    [InlineData(42949674, 50)]
    [InlineData(int.MaxValue, 50)]
    public async Task Validate_RejectsPageWhenProviderStartWouldOverflow(int page, int limit)
    {
        // Arrange
        var validator = new GetMarketCoinsRequestValidator();
        var request = new GetMarketCoinsRequest { Page = page, Limit = limit };

        // Act
        var result = await validator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        var error = Assert.Single(result.Errors);
        Assert.Equal("Page", error.PropertyName);
        Assert.Equal("Page is too large for the selected limit.", error.ErrorMessage);
    }

}
