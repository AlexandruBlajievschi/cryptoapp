using System.Net;

namespace backend.Test.TestDoubles.Http;

public class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpStatusCode _statusCode;
    private readonly string _body;
    private readonly string? _retryAfter;
    public Uri? LastRequestUri { get; private set; }

    public FakeHttpMessageHandler(HttpStatusCode statusCode, string body, string? retryAfter = null)
    {
        _statusCode = statusCode;
        _body = body;
        _retryAfter = retryAfter;
    }
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        LastRequestUri = request.RequestUri;
        var response = new HttpResponseMessage(_statusCode)
        {
            Content = new StringContent(_body)
        };

        if (_retryAfter is not null)
        {
            response.Headers.TryAddWithoutValidation("Retry-After", _retryAfter);
        }

        return Task.FromResult(response);
    }
}
