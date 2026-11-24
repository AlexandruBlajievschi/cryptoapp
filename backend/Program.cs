using backend.ExceptionHandling;
using backend.Features.Markets;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddScoped<MarketService>();
builder.Services.AddScoped<GetMarketCoinsRequestValidator>();
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

app.MapControllers();


app.Run();
public partial class Program { }

