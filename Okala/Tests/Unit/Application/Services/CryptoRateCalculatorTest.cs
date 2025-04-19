using FluentAssertions;
using Okala.Application.DTOs;
using Okala.Application.Services;
using Xunit;

namespace Okala.UnitTests.Application.Services
{
    public class CryptoRateCalculatorTests
    {
        private readonly CryptoRateCalculator _calculator;

        public CryptoRateCalculatorTests()
        {
            _calculator = new CryptoRateCalculator();
        }

        [Fact]
        public void CalculateRates_ShouldReturnConvertedRates()
        {
            // Arrange
            decimal usdPrice = 100m;
            var eurRates = new Dictionary<string, decimal>
            {
                { "USD", 1.1m },
                { "EUR", 1.0m },
                { "GBP", 0.85m }
            };

            // Act
            var result = _calculator.CalculateRates(usdPrice, eurRates).ToList();

            // Assert
            result.Should().HaveCount(3);

            result.Should().ContainSingle(r => r.Currency == "USD" && r.Value == 100m);

            var eurRate = eurRates["EUR"] / eurRates["USD"];
            var expectedEurValue = Math.Round(usdPrice * eurRate, 2);
            result.Should().ContainSingle(r => r.Currency == "EUR" && r.Value == expectedEurValue);

            var gbpRate = eurRates["GBP"] / eurRates["USD"];
            var expectedGbpValue = Math.Round(usdPrice * gbpRate, 2);
            result.Should().ContainSingle(r => r.Currency == "GBP" && r.Value == expectedGbpValue);
        }

        [Theory]
        [InlineData(0, 1.1)]
        [InlineData(-10, 1.1)]
        [InlineData(100, 0)]
        [InlineData(100, -1.2)]
        public void CalculateRates_ShouldThrow_WhenInvalidUsdOrUsdRate(decimal usdPrice, decimal eurToUsd)
        {
            // Arrange
            var rates = new Dictionary<string, decimal>
            {
                { "USD", eurToUsd },
                { "EUR", 1.0m }
            };

            // Act
            var act = () => _calculator.CalculateRates(usdPrice, rates);

            // Assert
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void CalculateRates_ShouldSkipInvalidCurrencies()
        {
            // Arrange
            decimal usdPrice = 100m;
            var eurRates = new Dictionary<string, decimal>
            {
                { "USD", 1.1m },
                { "EUR", 1.0m },
                { "GBP", 0.0m },    // invalid
                { "AUD", -5.0m },   // invalid
                { "BRL", 0.5m }     // valid
            };

            // Act
            var result = _calculator.CalculateRates(usdPrice, eurRates).ToList();

            // Assert
            result.Should().HaveCount(3); // USD + EUR + BRL
            result.Should().Contain(r => r.Currency == "BRL");
            result.Should().NotContain(r => r.Currency == "AUD" || r.Currency == "GBP");
        }
    }
}
