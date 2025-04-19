using Microsoft.Extensions.Options;
using Okala.Application.Interfaces.Clients;
using Okala.Infrastructure.Configuration;

namespace Okala.Infrastructure.Clients
{
    public class ExchangeRatesApiClient : IExchangeRatesApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ExchangeRatesSettings _settings;

        public ExchangeRatesApiClient(HttpClient httpClient, IOptions<ExchangeRatesSettings> settings)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
        }

        public async Task<HttpResponseMessage> GetLatestRatesAsync(string[] symbols, CancellationToken cancellationToken)
        {
            var symbolsQuery = string.Join(",", symbols.Distinct().Where(s => !string.IsNullOrWhiteSpace(s)));
            var url = $"{_settings.BaseUrl}?access_key={_settings.ApiKey}&symbols={symbolsQuery}";

            return await _httpClient.GetAsync(url,cancellationToken );
        }
    }


}
