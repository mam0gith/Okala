namespace Okala.Application.Interfaces.Clients
{
    public interface ICoinMarketCapApiClient
    {
        Task<HttpResponseMessage> GetCryptoQuoteAsync(string cryptoCode, CancellationToken cancellationToken);
    }
}
