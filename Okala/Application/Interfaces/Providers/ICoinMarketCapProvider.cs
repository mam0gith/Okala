namespace Okala.Application.Interfaces.Providers
{
    public interface ICoinMarketCapProvider
    {
        Task<decimal> GetUsdValueAsync(string cryptoCode, CancellationToken cancellationToken);
    }
}