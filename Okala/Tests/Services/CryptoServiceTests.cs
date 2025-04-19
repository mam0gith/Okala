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

        
    }
}
