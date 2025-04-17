namespace Okala.Providers.Interfaces
{
    public interface ICryptoProvider
    {
        Task<decimal> GetUsdValueAsync(string cryptoCode);
    }
}