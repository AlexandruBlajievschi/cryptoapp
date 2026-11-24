using Microsoft.AspNetCore.Diagnostics;
using backend.Features.Markets.Exceptions;

namespace backend.ExceptionHandling;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is MarketDataProviderRateLimitException rateLimitException)
        {
            _logger.LogWarning("Market data provider returned HTTP {ProviderStatusCode}; responding with HTTP {StatusCode}.",
                StatusCodes.Status429TooManyRequests, StatusCodes.Status503ServiceUnavailable);

            if (rateLimitException.RetryAfter is not null)
            {
                httpContext.Response.Headers.RetryAfter = rateLimitException.RetryAfter;
            }

            var rateLimitResult = Results.Problem(
                statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "Market data is temporarily unavailable. Please try again later.");
            await rateLimitResult.ExecuteAsync(httpContext);
            return true;
        }

        if(exception is MarketDataProviderTimeoutException)
        {
            var resultProblem = Results.Problem(statusCode: StatusCodes.Status504GatewayTimeout , title: "Market data provider timed out.");
            await resultProblem.ExecuteAsync(httpContext);
            return true;
        }
        if(exception is MarketDataProviderException)
        {
            var resultProblem = Results.Problem(statusCode: StatusCodes.Status502BadGateway , title: "Market data provider is unavailable.");
            await resultProblem.ExecuteAsync(httpContext);
            return true;
        }
        var result = Results.Problem(statusCode:StatusCodes.Status500InternalServerError, title : "An unexpected error occurred.");
        await result.ExecuteAsync(httpContext);
        return true;
    }
}
