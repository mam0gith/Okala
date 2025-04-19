using System.Net;
using System.Text;
using FluentAssertions;
using Moq;
using Okala.Application.Interfaces.Clients;
using Okala.Application.Interfaces.Resilience;
using Polly;
using Xunit;

namespace Okala.UnitTests.Infrastructure.Providers
{
    public class ExchangeRatesProviderTests
    {
        private readonly Mock<IExchangeRatesApiClient> _apiClientMock;
        private readonly Mock<IResiliencePolicyFactory> _policyFactoryMock;
        private readonly Mock<ILogger<ExchangeRatesProvider>> _loggerMock;
        private readonly IAsyncPolicy<HttpResponseMessage> _resiliencePolicy;

        public ExchangeRatesProviderTests()
        {
            _apiClientMock = new Mock<IExchangeRatesApiClient>();
            _policyFactoryMock = new Mock<IResiliencePolicyFactory>();
            _loggerMock = new Mock<ILogger<ExchangeRatesProvider>>();

            // Use Polly's NoOp policy for testing
            _resiliencePolicy = Policy.NoOpAsync<HttpResponseMessage>();
            _policyFactoryMock.Setup(x => x.CreateResiliencePolicy()).Returns(_resiliencePolicy);
        }

        private ExchangeRatesProvider CreateProvider() =>
            new ExchangeRatesProvider(_apiClientMock.Object, _policyFactoryMock.Object, _loggerMock.Object);

        [Fact]
        public async Task GetRatesAgainstEURAsync_ShouldReturnExpectedRates()
        {
            // Arrange
            var symbols = new[] { "USD", "GBP" };
            var expectedJson = """
                {
                    "rates": {
                        "USD": 1.1,
                        "GBP": 0.9
                    }
                }
                """;

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(expectedJson, Encoding.UTF8, "application/json")
            };

            _apiClientMock.Setup(x => x.GetLatestRatesAsync(symbols, It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var provider = CreateProvider();

            // Act
            var result = await provider.GetRatesAgainstEURAsync(symbols, default);

            // Assert
            result.Should().HaveCount(2);
            result["USD"].Should().Be(1.1m);
            result["GBP"].Should().Be(0.9m);
        }

        [Fact]
        public async Task GetRatesAgainstEURAsync_ShouldThrow_WhenApiResponseIsError()
        {
            // Arrange
            var symbols = new[] { "USD" };
            var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);

            _apiClientMock.Setup(x => x.GetLatestRatesAsync(symbols, It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var provider = CreateProvider();

            // Act
            Func<Task> act = async () => await provider.GetRatesAgainstEURAsync(symbols, default);

            // Assert
            await act.Should().ThrowAsync<HttpRequestException>()
                .WithMessage("Failed to get exchange rates.");
        }

        [Fact]
        public async Task GetRatesAgainstEURAsync_ShouldThrow_WhenSymbolsIsNull()
        {
            var provider = CreateProvider();

            Func<Task> act = async () => await provider.GetRatesAgainstEURAsync(null, default);

            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("No symbols provided.");
        }

        [Fact]
        public async Task GetRatesAgainstEURAsync_ShouldReturnEmpty_WhenRateIsMissing()
        {
            // Arrange
            var symbols = new[] { "USD" };
            var jsonMissingRate = """
                {
                    "rates": {
                        "EUR": 1.0
                    }
                }
                """;

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonMissingRate, Encoding.UTF8, "application/json")
            };

            _apiClientMock.Setup(x => x.GetLatestRatesAsync(symbols, It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var provider = CreateProvider();

            // Act
            var result = await provider.GetRatesAgainstEURAsync(symbols, default);

            // Assert
            result.Should().BeEmpty();
        }
    }
}
