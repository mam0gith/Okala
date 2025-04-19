using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Okala.Application.DTOs;
using Okala.Application.Interfaces;
using Okala.Application.Interfaces.Providers;
using Okala.Application.Services;
using Xunit;

namespace Okala.UnitTests.Application.Services
{
    public class CryptoServiceTests
    {
        private readonly Mock<ICoinMarketCapProvider> _cryptoProviderMock;
        private readonly Mock<IExchangeRatesProvider> _exchangeRatesProviderMock;
        private readonly Mock<ICryptoRateCalculator> _calculatorMock;
        private readonly Mock<ILogger<CryptoService>> _loggerMock;

        public CryptoServiceTests()
        {
            _cryptoProviderMock = new Mock<ICoinMarketCapProvider>();
            _exchangeRatesProviderMock = new Mock<IExchangeRatesProvider>();
            _calculatorMock = new Mock<ICryptoRateCalculator>();
            _loggerMock = new Mock<ILogger<CryptoService>>();
        }

        private CryptoService CreateService() =>
            new CryptoService(
                _cryptoProviderMock.Object,
                _exchangeRatesProviderMock.Object,
                _calculatorMock.Object,
                _loggerMock.Object
            );

        [Fact]
        public async Task GetConvertedRatesAsync_ShouldReturnExpectedResult()
        {
            // Arrange
            var cryptoCode = "BTC";
            var usdPrice = 65000.0m;
            var rates = new Dictionary<string, decimal>
            {
                { "USD", 1 },
                { "EUR", 0.9m },
                { "GBP", 0.8m },
                { "BRL", 5.1m },
                { "AUD", 1.6m }
            };

            var expectedResult = new List<CryptoRateDto>
            {
                new CryptoRateDto ( "USD",  65000.0m ),
                new CryptoRateDto ("EUR", 58500.0m),
            };

            _cryptoProviderMock
                .Setup(x => x.GetUsdValueAsync(cryptoCode, It.IsAny<CancellationToken>()))
                .ReturnsAsync(usdPrice);

            _exchangeRatesProviderMock
                .Setup(x => x.GetRatesAgainstEURAsync(It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(rates);

            _calculatorMock
                .Setup(x => x.CalculateRates(usdPrice, rates))
                .Returns(expectedResult);

            var service = CreateService();

            // Act
            var result = await service.GetConvertedRatesAsync(cryptoCode, default);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }

        [Fact]
        public async Task GetConvertedRatesAsync_ShouldCallProvidersAndCalculator()
        {
            // Arrange
            var cryptoCode = "ETH";
            var usdPrice = 3000m;
            var eurRates = new Dictionary<string, decimal>
            {
                { "USD", 1 },
                { "EUR", 0.95m },
                { "GBP", 0.8m },
                { "BRL", 5.0m },
                { "AUD", 1.5m }
            };

            _cryptoProviderMock
                .Setup(x => x.GetUsdValueAsync(cryptoCode, It.IsAny<CancellationToken>()))
                .ReturnsAsync(usdPrice);

            _exchangeRatesProviderMock
                .Setup(x => x.GetRatesAgainstEURAsync(It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(eurRates);

            _calculatorMock
                .Setup(x => x.CalculateRates(It.IsAny<decimal>(), It.IsAny<Dictionary<string, decimal>>()))
                .Returns(new List<CryptoRateDto>());

            var service = CreateService();

            // Act
            var result = await service.GetConvertedRatesAsync(cryptoCode, default);

            // Assert
            _cryptoProviderMock.Verify(x => x.GetUsdValueAsync(cryptoCode, It.IsAny<CancellationToken>()), Times.Once);
            _exchangeRatesProviderMock.Verify(x => x.GetRatesAgainstEURAsync(It.IsAny<string[]>(), It.IsAny<CancellationToken>()), Times.Once);
            _calculatorMock.Verify(x => x.CalculateRates(usdPrice, eurRates), Times.Once);
        }
    }
}
