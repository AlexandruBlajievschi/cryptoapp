using System;
using System.Net;

namespace backend.Test;

public class SlowHttpMessageHandler : HttpMessageHandler
{
    protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {


        await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);

        var response = new HttpResponseMessage(HttpStatusCode.OK);

        return response;

    }
}
