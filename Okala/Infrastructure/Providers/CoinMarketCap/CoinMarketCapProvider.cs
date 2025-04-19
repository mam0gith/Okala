using System.Text.Json;
using Okala.Application.Interfaces.Cache;
using Okala.Application.Interfaces.Clients;
using Okala.Application.Interfaces.Providers;
using Okala.Application.Interfaces.Resilience;
using Polly;

namespace Okala.Infrastructure.Providers.CoinMarketCap
{

    public class CoinMarketCapProvider : ICoinMarketCapProvider
    {
        private readonly ILogger<CoinMarketCapProvider> _logger;
        private readonly IAsyncPolicy<HttpResponseMessage> _resiliencePolicy;
        private readonly ICoinMarketCapApiClient _apiClient;
        private readonly ICryptoRateCacheService _cacheService;


        public CoinMarketCapProvider(
            ICoinMarketCapApiClient apiClient,
            IResiliencePolicyFactory policyFactory,
            ILogger<CoinMarketCapProvider> logger,
            ICryptoRateCacheService cacheService
           )
        {
            _logger = logger;
            _resiliencePolicy = policyFactory.CreateResiliencePolicy();
            _apiClient = apiClient;
            _cacheService = cacheService;

        }

        public async Task<decimal> GetUsdValueAsync(string cryptoCode, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(cryptoCode))
                throw new ArgumentException("Invalid crypto code.");

            var cached = await _cacheService.GetCachedRateAsync(cryptoCode);
            if (cached.HasValue)
            {
                _logger.LogInformation("Returning cached price for {CryptoCode}", cryptoCode);
                return cached.Value;
            }

            var context = new Context("GetCryptoPrice");

            var response = await _resiliencePolicy.ExecuteAsync(
                async (ctx) => await _apiClient.GetCryptoQuoteAsync(cryptoCode, cancellationToken),
                context);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("CoinMarketCap API failed: {Code} {Reason}", response.StatusCode, response.ReasonPhrase);
                throw new HttpRequestException("Failed to get crypto price.");
            }

            var price = await ParsePriceFromResponse(response, cryptoCode);
            await _cacheService.SetCachedRateAsync(cryptoCode, price);

            return price;
        }

        private async Task<decimal>  ParsePriceFromResponse(HttpResponseMessage response, string cryptoCode)
        {
            using var contentStream = await response.Content.ReadAsStreamAsync();
            var json = await JsonDocument.ParseAsync(contentStream);
            if (!json.RootElement.TryGetProperty("data", out var data) ||
                !data.TryGetProperty(cryptoCode, out var coinData) ||
                !coinData.TryGetProperty("quote", out var quote) ||
                !quote.TryGetProperty("USD", out var usdObj) ||
                !usdObj.TryGetProperty("price", out var priceElement) ||
                priceElement.GetDecimal() <= 0)
            {
                _logger.LogError("Invalid or missing data from CoinMarketCap response.");
                throw new InvalidOperationException("Invalid crypto data.");
            }

            return priceElement.GetDecimal();
        }


    }

}