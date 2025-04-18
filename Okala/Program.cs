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
builder.Services.AddHttpClient<IExchangeRatesProvider, ExchangeRatesProvider>()
    .AddPolicyHandler(GetCircuitBreakerPolicy())
    .AddPolicyHandler(GetRetryPolicy());


builder.Services.AddHttpClient<ICryptoProvider, CoinMarketCapProvider>();
builder.Services.AddSingleton<IResiliencePolicyFactory, DefaultResiliencePolicyFactory>();




static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return Policy<HttpResponseMessage>
        .Handle<HttpRequestException>() 
        .OrResult(r => !r.IsSuccessStatusCode)
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(2),
            onRetry: (outcome, timespan, retryAttempt, context) =>
            {
                Console.WriteLine($"Retrying... attempt {retryAttempt}");
            });
}

static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 2,
            durationOfBreak: TimeSpan.FromSeconds(30),
            onBreak: (outcome, timespan) =>
            {
                Console.WriteLine("Circuit broken!");
            },
            onReset: () =>
            {
                Console.WriteLine("Circuit reset.");
            });
}

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
