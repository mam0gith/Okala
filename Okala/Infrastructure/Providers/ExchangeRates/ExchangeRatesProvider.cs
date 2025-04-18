﻿using System.Text.Json;
using Okala.Application.Interfaces.Clients;
using Okala.Application.Interfaces.Providers;
using Okala.Application.Interfaces.Resilience;
using Polly;

public class ExchangeRatesProvider : IExchangeRatesProvider
{
    private readonly ILogger<ExchangeRatesProvider> _logger;
    private readonly IAsyncPolicy<HttpResponseMessage> _resiliencePolicy;
    private readonly IExchangeRatesApiClient _apiClient;

    public ExchangeRatesProvider(
        IExchangeRatesApiClient apiClient,
        IResiliencePolicyFactory resiliencePolicyFactory,
        ILogger<ExchangeRatesProvider> logger)
    {
        _apiClient = apiClient;
        _resiliencePolicy = resiliencePolicyFactory.CreateResiliencePolicy();
        _logger = logger;
    }

    public async Task<Dictionary<string, decimal>> GetRatesAgainstEURAsync(string[] symbols, CancellationToken cancellationToken)
    {
        if (symbols == null || symbols.Length == 0)
            throw new ArgumentException("No symbols provided.");

        var context = new Context("ExchangeRatesAPI");

        var response = await _resiliencePolicy.ExecuteAsync(
            async (ctx) => await _apiClient.GetLatestRatesAsync(symbols, cancellationToken),
            context);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("ExchangeRates API failed: {Code} {Reason}", response.StatusCode, response.ReasonPhrase);
            throw new HttpRequestException("Failed to get exchange rates.");
        }

        return ParsRatesFromResponse(response, symbols).Result;

    }

    private async Task<Dictionary<string, decimal>> ParsRatesFromResponse(HttpResponseMessage response, string[] symbols)
    {
        using var stream = await response.Content.ReadAsStreamAsync();
        var json = await JsonDocument.ParseAsync(stream);

        if (!json.RootElement.TryGetProperty("rates", out var ratesElement) || ratesElement.ValueKind != JsonValueKind.Object)
        {
            _logger.LogWarning("Rates property missing in response.");
            throw new InvalidOperationException("Rates data is invalid.");
        }

        var result = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

        foreach (var symbol in symbols.Distinct())
        {
            if (ratesElement.TryGetProperty(symbol, out var rateEl)
                && rateEl.TryGetDecimal(out var value)
                && value > 0)
            {
                result[symbol] = value;
            }
            else
            {
                _logger.LogWarning("Rate missing or invalid for symbol: {Symbol}", symbol);
            }
        }

        return result;
    }
}

