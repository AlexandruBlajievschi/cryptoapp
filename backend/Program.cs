using backend.ExceptionHandling;
using backend.Features.Markets;
using backend.Features.Markets.GetCoins;
using backend.Features.Markets.GetCoinById;
using backend.Infrastructure.ExternalApis.CoinMarketCap;
using backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    DatabaseConfiguration.LoadLocalEnv(builder.Configuration, builder.Environment.ContentRootPath);
}

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(DatabaseConfiguration.GetConnectionString(builder.Configuration));
});
builder.Services.AddScoped<MarketService>();
builder.Services.AddScoped<GetMarketCoinsRequestValidator>();
builder.Services.AddScoped<GetMarketCoinRequestValidator>();
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

