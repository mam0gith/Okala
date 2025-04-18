namespace Okala.Providers.Interfaces
{
    public interface IExchangeRatesApiClient
    {
        Task<HttpResponseMessage> GetLatestRatesAsync(string[] symbols);
    }


}
