using CryptoRateApp.Configuration;
using CryptoRateApp.Providers;
using Okala.Providers.Interfaces;
using Okala.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<CoinMarketCapSettings>(
    builder.Configuration.GetSection("CoinMarketCap"));
builder.Services.Configure<ExchangeRatesSettings>(
    builder.Configuration.GetSection("ExchangeRates"));

builder.Services.AddScoped<ICryptoService, CryptoService>();
builder.Services.AddScoped<ICryptoProvider, CoinMarketCapProvider>();
builder.Services.AddScoped<IExchangeRatesProvider, ExchangeRatesProvider>();
builder.Services.AddScoped<ICryptoRateCalculator, CryptoRateCalculator>();
builder.Services.AddHttpClient<CoinMarketCapProvider>();
builder.Services.AddHttpClient<ExchangeRatesProvider>();

builder.Services.AddMemoryCache();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

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
