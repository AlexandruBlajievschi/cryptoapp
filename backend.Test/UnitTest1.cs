using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace backend.Test;

public class BackendTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public BackendTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }
    [Fact]
    public async Task GetHealthCheck_ReturnsOkResult()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/health");

        if (!response.IsSuccessStatusCode)
        {
            Assert.Fail("Failed to get health check");
        }
        var content = await response.Content.ReadAsStringAsync();

        using var json = JsonDocument.Parse(content);

        var status = json.RootElement.GetProperty("status").GetString();

        Assert.Equal("healthy", status);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetHealthCheck_ReturnsNotFound()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("api/healthdsaf");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
