using CryptoRateApp.Configuration;
using CryptoRateApp.Providers;
using Okala.Providers.Interfaces;
using Okala.Services;
using Polly.Extensions.Http;
using Serilog;
using Polly;
using Polly.Timeout;
using CryptoRateApp.Services.Resilience;
using Okala.Services.Resilience;
using Okala.Providers;


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
builder.Services.AddScoped<ICryptoProvider, CoinMarketCapProvider>();
builder.Services.AddScoped<IExchangeRatesProvider, ExchangeRatesProvider>();
builder.Services.AddScoped<ICryptoRateCalculator, CryptoRateCalculator>();
builder.Services.AddHttpClient<IExchangeRatesApiClient, ExchangeRatesApiClient>();




builder.Services.AddSingleton<IResiliencePolicyFactory, DefaultResiliencePolicyFactory>();
builder.Services.AddSingleton<ICoinMarketCapApiClient>(provider =>
    new CoinMarketCapApiClient(
        provider.GetRequiredService<HttpClient>(),
        builder.Configuration["CoinMarketCap:ApiKey"] ??
        throw new ArgumentNullException("CoinMarketCap:ApiKey configuration is missing")));
builder.Services.AddSingleton<IExchangeRatesApiClient>(provider =>
    new ExchangeRatesApiClient(
        provider.GetRequiredService<HttpClient>(),
        builder.Configuration["ExchangeRates:ApiKey"] ??
        throw new ArgumentNullException("ExchangeRates:ApiKey configuration is missing")));
//builder.Services.AddTransient<ICoinMarketCapResponseParser, CoinMarketCapResponseParser>();





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
