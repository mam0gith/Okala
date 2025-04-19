namespace Okala.Application.Interfaces.Cache
{
    public interface ICryptoRateCacheService
    {
        Task<decimal?> GetCachedRateAsync(string cryptoCode);
        Task SetCachedRateAsync(string cryptoCode, decimal price);
    }

}
