using System.Text.Json;
using CryptoRateApp.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Okala.Providers.Interfaces;

public class ExchangeRatesProvider : IExchangeRatesProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ExchangeRatesProvider> _logger;
    private readonly string _apiKey;

    public ExchangeRatesProvider(HttpClient httpClient, IConfiguration config, ILogger<ExchangeRatesProvider> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiKey = config["ExchangeRates:ApiKey"] ?? throw new ArgumentNullException("API Key not configured");
    }

    public async Task<Dictionary<string, decimal>> GetRatesAgainstEURAsync(string[] symbols)
    {
        var symbolsQuery = string.Join(",", symbols.Distinct().Where(s => !string.IsNullOrWhiteSpace(s)));

        var url = $"http://api.exchangeratesapi.io/v1/latest?access_key={_apiKey}&symbols={symbolsQuery}";

        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("ExchangeRates API failed: {Code} {Reason}", response.StatusCode, response.ReasonPhrase);
            throw new HttpRequestException("Failed to get exchange rates.");
        }

        using var contentStream = await response.Content.ReadAsStreamAsync();
        var json = await JsonDocument.ParseAsync(contentStream);

        if (!json.RootElement.TryGetProperty("rates", out var rates) || rates.ValueKind != JsonValueKind.Object)
        {
            _logger.LogWarning("Rates missing in response.");
            throw new InvalidOperationException("Rates data is invalid.");
        }

        var result = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

        foreach (var symbol in symbols.Distinct())
        {
            if (rates.TryGetProperty(symbol, out var rateElement) && rateElement.TryGetDecimal(out var value) && value > 0)
                result[symbol] = value;
            else
                _logger.LogWarning("Rate missing or invalid for symbol: {Symbol}", symbol);
        }

        return result;
    }

   
}
