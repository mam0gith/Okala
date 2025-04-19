using Polly.Extensions.Http;
using Serilog;
using Polly;
using Polly.Timeout;
using Okala.Application.Services;
using Okala.Infrastructure.Configuration;
using Okala.Infrastructure.Resilience;
using Okala.Infrastructure.Clients;
using Okala.Infrastructure.Providers.CoinMarketCap;
using Okala.Application.Interfaces;
using Okala.Application.Interfaces.Clients;
using Okala.Application.Interfaces.Providers;
using Okala.Application.Interfaces.Resilience;


Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/log.txt", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .CreateLogger();
var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog(); 

builder.Services.Configure<CoinMarketCapSettings>(
    builder.Configuration.GetSection("CoinMarketCap"));
builder.Services.Configure<ExchangeRatesSettings>(
    builder.Configuration.GetSection("ExchangeRates"));

builder.Services.AddScoped<ICryptoService, CryptoService>();
builder.Services.AddScoped<ICoinMarketCapProvider, CoinMarketCapProvider>();
builder.Services.AddScoped<IExchangeRatesProvider, ExchangeRatesProvider>();
builder.Services.AddScoped<ICryptoRateCalculator, CryptoRateCalculator>();
builder.Services.AddHttpClient<ICoinMarketCapApiClient, CoinMarketCapApiClient>();

builder.Services.AddHttpClient<IExchangeRatesApiClient, ExchangeRatesApiClient>();

var settings = builder.Configuration.GetSection("ExchangeRates").Get<ExchangeRatesSettings>();



builder.Services.AddSingleton<IResiliencePolicyFactory, DefaultResiliencePolicyFactory>();






builder.Services.AddMemoryCache();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
