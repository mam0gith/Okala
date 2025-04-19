using Okala.Application.Interfaces.Clients;

namespace Okala.Infrastructure.Clients
{
    public class ExchangeRatesApiClient : IExchangeRatesApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public ExchangeRatesApiClient(HttpClient httpClient, string apiKey)
        {
            _httpClient = httpClient;
            _apiKey = apiKey;
        }

        public async Task<HttpResponseMessage> GetLatestRatesAsync(string[] symbols)
        {
            var symbolsQuery = string.Join(",", symbols.Distinct().Where(s => !string.IsNullOrWhiteSpace(s)));
            var url = $"http://api.exchangeratesapi.io/v1/latest?access_key={_apiKey}&symbols={symbolsQuery}";

            return await _httpClient.GetAsync(url);
        }
    }


}
