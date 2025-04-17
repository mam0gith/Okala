
using Xunit;
using Okala.Services;

public class CryptoRateCalculatorTests
{
    private readonly CryptoRateCalculator _calculator;

    public CryptoRateCalculatorTests()
    {
        _calculator = new CryptoRateCalculator();
    }

    [Fact]
    public void CalculateRates_ValidRates_ReturnsExpectedCurrencies()
    {
        // Arrange
        decimal usdPrice = 1000m;
        var rates = new Dictionary<string, decimal>
        {
            { "USD", 1.2m },
            { "EUR", 1.0m },
            { "BRL", 6.0m },
            { "GBP", 0.85m },
            { "AUD", 1.5m }
        };

        // Act
        var result = _calculator.CalculateRates(usdPrice, rates).ToList();

        // Assert
        Assert.Equal(5, result.Count);
        Assert.Contains(result, r => r.Currency == "USD" && r.Value == 1000m);
        Assert.Contains(result, r => r.Currency == "EUR");
        Assert.Contains(result, r => r.Currency == "BRL");
        Assert.Contains(result, r => r.Currency == "GBP");
        Assert.Contains(result, r => r.Currency == "AUD");
    }

    [Fact]
    public void CalculateRates_MissingUsdRate_ThrowsInvalidOperation()
    {
        // Arrange
        var rates = new Dictionary<string, decimal>
        {
            { "EUR", 1.0m },
            { "BRL", 5.5m }
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _calculator.CalculateRates(1000m, rates));
    }

    [Fact]
    public void CalculateRates_ZeroOrNegativeUsdRate_ThrowsInvalidOperation()
    {
        var rates = new Dictionary<string, decimal>
        {
            { "USD", 0m },
            { "EUR", 1.0m }
        };

        Assert.Throws<InvalidOperationException>(() => _calculator.CalculateRates(1000m, rates));
    }

    [Fact]
    public void CalculateRates_NegativeOrZeroTargetCurrencyRate_SkipsThatCurrency()
    {
        var rates = new Dictionary<string, decimal>
        {
            { "USD", 1.2m },
            { "BRL", -5.5m },
            { "AUD", 0 },
            { "GBP", 0.8m }
        };

        var result = _calculator.CalculateRates(1000m, rates).ToList();

        Assert.Equal(2, result.Count); // USD and GBP
        Assert.Contains(result, r => r.Currency == "USD");
        Assert.Contains(result, r => r.Currency == "GBP");
    }
}
