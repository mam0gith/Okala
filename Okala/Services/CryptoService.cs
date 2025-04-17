using CryptoRateApp.DTOs;
using Okala.Providers.Interfaces;

public class CryptoService : ICryptoService
{
    private readonly ICryptoProvider _cryptoProvider;
    private readonly IExchangeRatesProvider _exchangeRatesProvider;
    private readonly ICryptoRateCalculator _calculator;

    public CryptoService(ICryptoProvider cryptoProvider, IExchangeRatesProvider exchangeRatesProvider, ICryptoRateCalculator calculator)
    {
        _cryptoProvider = cryptoProvider;
        _exchangeRatesProvider = exchangeRatesProvider;
        _calculator = calculator;
    }

    public async Task<IEnumerable<CryptoRateDto>> GetConvertedRatesAsync(string cryptoCode)
    {
        if (string.IsNullOrWhiteSpace(cryptoCode))
            throw new ArgumentException("Crypto code must be provided.");

        var usdPrice = await _cryptoProvider.GetUsdValueAsync(cryptoCode);
        var eurRates = await _exchangeRatesProvider.GetRatesAgainstEURAsync(new[] { "USD", "EUR", "BRL", "GBP", "AUD" });

        return _calculator.CalculateRates(usdPrice, eurRates);
    }
}
