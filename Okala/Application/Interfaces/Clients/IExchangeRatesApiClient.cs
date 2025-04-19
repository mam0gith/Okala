namespace Okala.Application.Interfaces.Clients
{
    public interface IExchangeRatesApiClient
    {
        Task<HttpResponseMessage> GetLatestRatesAsync(string[] symbols);
    }


}
