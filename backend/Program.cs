var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();

var app = builder.Build();

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



app.Run();

