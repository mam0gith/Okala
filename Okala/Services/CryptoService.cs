using CryptoRateApp.DTOs;
using Okala.Providers.Interfaces;

public class CryptoService : ICryptoService
{
    private readonly ICryptoProvider _cryptoProvider;
    private readonly IExchangeRatesProvider _exchangeRatesProvider;
    private readonly ICryptoRateCalculator _calculator;
    private readonly ILogger<CryptoService> _logger;

    public CryptoService(
        ICryptoProvider cryptoProvider,
        IExchangeRatesProvider exchangeRatesProvider,
        ICryptoRateCalculator calculator,
        ILogger<CryptoService> logger)
    {
        _cryptoProvider = cryptoProvider;
        _exchangeRatesProvider = exchangeRatesProvider;
        _calculator = calculator;
        _logger = logger;
    }

    public async Task<IEnumerable<CryptoRateDto>> GetConvertedRatesAsync(string cryptoCode)
    {
        if (string.IsNullOrWhiteSpace(cryptoCode))
        {
            _logger.LogWarning("Crypto code was null or empty.");
            throw new ArgumentException("Crypto code must be provided.");
        }

        _logger.LogInformation("Fetching USD price for crypto code: {CryptoCode}", cryptoCode);

        var usdPrice = await _cryptoProvider.GetUsdValueAsync(cryptoCode);
        _logger.LogInformation("USD price for {CryptoCode}: {UsdPrice}", cryptoCode, usdPrice);

        var currencies = new[] { "USD", "EUR", "BRL", "GBP", "AUD" };
        _logger.LogInformation("Fetching EUR rates for currencies: {Currencies}", currencies);

        var eurRates = await _exchangeRatesProvider.GetRatesAgainstEURAsync(currencies);
        _logger.LogInformation("Fetched EUR rates: {@EurRates}", eurRates);

        var results = _calculator.CalculateRates(usdPrice, eurRates);
        _logger.LogInformation("Calculated final rates for {CryptoCode}: {@Results}", cryptoCode, results);

        return results;
    }
}
