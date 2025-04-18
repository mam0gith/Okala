namespace Okala.Providers.Interfaces
{
    public interface ICoinMarketCapApiClient
    {
        Task<HttpResponseMessage> GetCryptoQuoteAsync(string cryptoCode);
    }
}
