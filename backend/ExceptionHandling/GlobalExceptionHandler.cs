using Microsoft.AspNetCore.Diagnostics;

namespace backend.ExceptionHandling;

public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
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
