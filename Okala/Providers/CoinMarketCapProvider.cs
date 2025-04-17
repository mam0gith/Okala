using System.Net.Http.Headers;
using System.Text.Json;
using CryptoRateApp.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Okala.Providers.Interfaces;

namespace CryptoRateApp.Providers
{

    public class CoinMarketCapProvider : ICryptoProvider
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CoinMarketCapProvider> _logger;
        private readonly string _apiKey;

        public CoinMarketCapProvider(HttpClient httpClient, IConfiguration config, ILogger<CoinMarketCapProvider> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _apiKey = config["CoinMarketCap:ApiKey"] ?? throw new ArgumentNullException("API Key not configured");
        }

        public async Task<decimal> GetUsdValueAsync(string cryptoCode)
        {
            if (string.IsNullOrWhiteSpace(cryptoCode))
                throw new ArgumentException("Invalid crypto code.");

            var url = $"https://pro-api.coinmarketcap.com/v1/cryptocurrency/quotes/latest?symbol={cryptoCode}";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("X-CMC_PRO_API_KEY", _apiKey);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("CoinMarketCap API failed: {Code} {Reason}", response.StatusCode, response.ReasonPhrase);
                throw new HttpRequestException("Failed to get crypto price.");
            }

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