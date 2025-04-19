using System.Net;
using System.Text;
using FluentAssertions;
using Moq;
using Okala.Application.Interfaces.Cache;
using Okala.Application.Interfaces.Clients;
using Okala.Application.Interfaces.Resilience;
using Okala.Infrastructure.Providers.CoinMarketCap;
using Polly;
using Xunit;

public class CoinMarketCapProviderTests
{
    private readonly Mock<ICoinMarketCapApiClient> _apiClientMock = new();
    private readonly Mock<IResiliencePolicyFactory> _policyFactoryMock = new();
    private readonly Mock<ICryptoRateCacheService> _cacheServiceMock = new();
    private readonly Mock<ILogger<CoinMarketCapProvider>> _loggerMock = new();

    private readonly IAsyncPolicy<HttpResponseMessage> _noopPolicy = Policy.NoOpAsync<HttpResponseMessage>();

    private CoinMarketCapProvider CreateProvider()
    {
        _policyFactoryMock.Setup(p => p.CreateResiliencePolicy()).Returns(_noopPolicy);
        return new CoinMarketCapProvider(_apiClientMock.Object, _policyFactoryMock.Object, _loggerMock.Object, _cacheServiceMock.Object);
    }

    [Fact]
    public async Task GetUsdValueAsync_ThrowsException_WhenCryptoCodeIsInvalid()
    {
        var provider = CreateProvider();
        await Assert.ThrowsAsync<ArgumentException>(() => provider.GetUsdValueAsync("", CancellationToken.None));
    }

    [Fact]
    public async Task GetUsdValueAsync_ReturnsCachedValue_IfExists()
    {
        _cacheServiceMock.Setup(c => c.GetCachedRateAsync("BTC")).ReturnsAsync(12345m);
        var provider = CreateProvider();

        var result = await provider.GetUsdValueAsync("BTC", CancellationToken.None);

        result.Should().Be(12345m);
        _apiClientMock.Verify(x => x.GetCryptoQuoteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetUsdValueAsync_CallsApiAndParsesPrice_WhenNotCached()
    {
        _cacheServiceMock.Setup(c => c.GetCachedRateAsync("BTC")).ReturnsAsync((decimal?)null);

        var json = """
        {
            "data": {
                "BTC": {
                    "quote": {
                        "USD": {
                            "price": 54321.99
                        }
                    }
                }
            }
        }
        """;

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        _apiClientMock.Setup(a => a.GetCryptoQuoteAsync("BTC", It.IsAny<CancellationToken>()))
                      .ReturnsAsync(response);

        var provider = CreateProvider();
        var result = await provider.GetUsdValueAsync("BTC", CancellationToken.None);

        result.Should().Be(54321.99m);
        _cacheServiceMock.Verify(c => c.SetCachedRateAsync("BTC", 54321.99m), Times.Once);
    }

    [Fact]
    public async Task GetUsdValueAsync_ThrowsException_WhenApiFails()
    {
        _cacheServiceMock.Setup(c => c.GetCachedRateAsync("BTC")).ReturnsAsync((decimal?)null);

        var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            ReasonPhrase = "Bad Request"
        };

        _apiClientMock.Setup(a => a.GetCryptoQuoteAsync("BTC", It.IsAny<CancellationToken>()))
                      .ReturnsAsync(response);

        var provider = CreateProvider();
        await Assert.ThrowsAsync<HttpRequestException>(() => provider.GetUsdValueAsync("BTC", CancellationToken.None));
    }
}