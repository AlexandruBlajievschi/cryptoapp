using backend.ExceptionHandling;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddHttpClient<CoinMarketCapClient>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
});
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "v1");
    });
}

app.UseHttpsRedirection();

app.MapGet("/api/health", () =>
{
    var status = new
    {
        status = "healthy"
    };

    return status;
})
.WithName("GetHealthCheck")
.WithSummary("Initial get health check!")
.WithDescription("Returns a stable string representing the status of the health check");

app.MapGet("/api/dev/market", async (CoinMarketCapClient marketClient, CancellationToken cancellationToken) =>
{
    var listings = await marketClient.GetListingsAsync(cancellationToken);
    return Results.Ok(listings);
});


app.Run();
public partial class Program { }

