using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Okala.Infrastructure.Clients;
using Okala.Infrastructure.Configuration;
using Xunit;

namespace Okala.UnitTests.Infrastructure.Clients
{
    public class CoinMarketCapApiClientTests
    {
        private readonly CoinMarketCapSettings _settings = new()
        {
            BaseUrl = "https://fake.api.com/v1/crypto/",
            ApiKey = "test-api-key"
        };

        [Fact]
        public async Task GetCryptoQuoteAsync_ShouldCallCorrectUrlAndReturnResponse()
        {
            // Arrange
            var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri.ToString() == $"{_settings.BaseUrl}BTC" &&
                        req.Headers.Contains("X-CMC_PRO_API_KEY")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(expectedResponse)
                .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object);
            var options = Options.Create(_settings);

            var client = new CoinMarketCapApiClient(httpClient, options);

            // Act
            var result = await client.GetCryptoQuoteAsync("BTC", CancellationToken.None);

            // Assert
            result.Should().Be(expectedResponse);
            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        }
    }
}
