using Microsoft.Extensions.Caching.Memory;
using Okala.Application.Interfaces.Cache;

namespace Okala.Infrastructure.Cache
{
    public class InMemoryCryptoRateCacheService : ICryptoRateCacheService
    {
        private readonly IMemoryCache _cache;
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(1);

        public InMemoryCryptoRateCacheService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public Task<decimal?> GetCachedRateAsync(string cryptoCode)
        {
            if (_cache.TryGetValue(GetCacheKey(cryptoCode), out decimal value))
                return Task.FromResult<decimal?>(value);

            return Task.FromResult<decimal?>(null);
        }

        public Task SetCachedRateAsync(string cryptoCode, decimal price)
        {
            _cache.Set(GetCacheKey(cryptoCode), price, CacheDuration);
            return Task.CompletedTask;
        }

        private string GetCacheKey(string cryptoCode) => $"CoinPrice_{cryptoCode}";
    }

}
