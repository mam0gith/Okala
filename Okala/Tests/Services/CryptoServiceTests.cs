using Moq;
using Okala.Application.DTOs;
using Okala.Application.Interfaces;
using Okala.Application.Interfaces.Providers;
using Xunit;

namespace Okala.Tests.Services
{
    public class CryptoServiceTests
    {
        private readonly Mock<ICoinMarketCapProvider> _cryptoProviderMock;
        private readonly Mock<IExchangeRatesProvider> _exchangeRatesProviderMock;
        private readonly Mock<ICryptoRateCalculator> _calculatorMock;
        private readonly Mock<ILogger<CryptoService>> _loggerMock;
        private readonly CryptoService _cryptoService;

        public CryptoServiceTests()
        {
            _cryptoProviderMock = new Mock<ICoinMarketCapProvider>();
            _exchangeRatesProviderMock = new Mock<IExchangeRatesProvider>();
            _calculatorMock = new Mock<ICryptoRateCalculator>();
            _loggerMock = new Mock<ILogger<CryptoService>>();

            _cryptoService = new CryptoService(
                _cryptoProviderMock.Object,
                _exchangeRatesProviderMock.Object,
                _calculatorMock.Object,_loggerMock.Object);
        }

        [Fact]
        public async Task GetConvertedRatesAsync_ValidInput_ReturnsExpectedRates()
        {
            // Arrange
            string cryptoCode = "BTC";
            decimal usdPrice = 30000m;
            var exchangeRates = new Dictionary<string, decimal>
            {
                { "USD", 1.1m },
                { "BRL", 5.5m },
                { "GBP", 0.85m },
                { "AUD", 1.6m }
            };
            var expectedRates = new List<CryptoRateDto>
            {
                new CryptoRateDto("USD", 30000m),
                new CryptoRateDto("GBP", 25000m)
            };

            _cryptoProviderMock
                .Setup(p => p.GetUsdValueAsync(cryptoCode))
                .ReturnsAsync(usdPrice);

            _exchangeRatesProviderMock
                .Setup(p => p.GetRatesAgainstEURAsync(It.IsAny<string[]>()))
                .ReturnsAsync(exchangeRates);

            _calculatorMock
                .Setup(c => c.CalculateRates(usdPrice, exchangeRates))
                .Returns(expectedRates);

            // Act
            var result = await _cryptoService.GetConvertedRatesAsync(cryptoCode);

            // Assert
            Assert.Equal(expectedRates, result);
        }

        [Fact]
        public async Task GetConvertedRatesAsync_InvalidCryptoCode_ThrowsArgumentException()
        {
            // Arrange
            string cryptoCode = "";

            // Act + Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                _cryptoService.GetConvertedRatesAsync(cryptoCode));

            Assert.Equal("Crypto code must be provided.", ex.Message);
        }

        [Fact]
        public async Task GetConvertedRatesAsync_ProviderThrowsException_ThrowsItUpwards()
        {
            _cryptoProviderMock
                .Setup(p => p.GetUsdValueAsync(It.IsAny<string>()))
                .ThrowsAsync(new HttpRequestException("Downstream error"));

            await Assert.ThrowsAsync<HttpRequestException>(() =>
                _cryptoService.GetConvertedRatesAsync("BTC"));
        }

        [Fact]
        public async Task GetConvertedRatesAsync_ExchangeRatesReturnsNull_ThrowsException()
        {
            _cryptoProviderMock
                .Setup(p => p.GetUsdValueAsync(It.IsAny<string>()))
                .ReturnsAsync(30000m);

            _exchangeRatesProviderMock
                .Setup(p => p.GetRatesAgainstEURAsync(It.IsAny<string[]>()))
                .ReturnsAsync((Dictionary<string, decimal>?)null);

            _calculatorMock
                .Setup(c => c.CalculateRates(It.IsAny<decimal>(), null!))
                .Throws<ArgumentNullException>();

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _cryptoService.GetConvertedRatesAsync("BTC"));
        }
    }
}
