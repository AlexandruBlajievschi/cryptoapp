using System.Net;

namespace backend.Test;

public class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpStatusCode _statusCode;
    private readonly string _body;
    public Uri? LastRequestUri { get; private set; }

    public FakeHttpMessageHandler(HttpStatusCode statusCode, string body)
    {
        _statusCode = statusCode;
        _body = body;
    }
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        LastRequestUri = request.RequestUri;
        var response = new HttpResponseMessage(_statusCode)
        {
            Content = new StringContent(_body)
        };

        return Task.FromResult(response);
    }
}
