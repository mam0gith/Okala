using Microsoft.Extensions.Options;
using Okala.Application.Interfaces.Clients;
using Okala.Infrastructure.Configuration;

namespace Okala.Infrastructure.Clients
{
    public class CoinMarketCapApiClient : ICoinMarketCapApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly CoinMarketCapSettings _settings;

        public CoinMarketCapApiClient(HttpClient httpClient,  IOptions<CoinMarketCapSettings> settings)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
        }

        public async Task<HttpResponseMessage> GetCryptoQuoteAsync(string cryptoCode)
        {
            
            var url = $"{_settings.BaseUrl}{cryptoCode}";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("X-CMC_PRO_API_KEY", _settings.ApiKey);
            return await _httpClient.SendAsync(request);
        }
    }
}
