using Okala.Application.Interfaces.Clients;

namespace Okala.Infrastructure.Clients
{
    public class CoinMarketCapApiClient : ICoinMarketCapApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public CoinMarketCapApiClient(HttpClient httpClient, string apiKey)
        {
            _httpClient = httpClient;
            _apiKey = apiKey;
        }

        public async Task<HttpResponseMessage> GetCryptoQuoteAsync(string cryptoCode)
        {
            var url = $"https://pro-api.coinmarketcap.com/v1/cryptocurrency/quotes/latest?symbol={cryptoCode}";
            //var url = $"https://httpstat.us/500";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("X-CMC_PRO_API_KEY", _apiKey);
            return await _httpClient.SendAsync(request);
        }
    }
}
