namespace Okala.Tests.Unit.Infrastructure.Cache
{
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.Extensions.Caching.Memory;
    using Okala.Infrastructure.Cache;
    using Xunit;

    public class InMemoryCryptoRateCacheServiceTests
    {
        private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
        private readonly InMemoryCryptoRateCacheService _service;

        public InMemoryCryptoRateCacheServiceTests()
        {
            _service = new InMemoryCryptoRateCacheService(_cache);
        }

        [Fact]
        public async Task GetCachedRateAsync_ReturnsNull_WhenNotSet()
        {
            var result = await _service.GetCachedRateAsync("ETH");
            result.Should().BeNull();
        }

        [Fact]
        public async Task SetCachedRateAsync_StoresValueCorrectly()
        {
            await _service.SetCachedRateAsync("ETH", 1200.50m);
            var result = await _service.GetCachedRateAsync("ETH");

            result.Should().Be(1200.50m);
        }
    }
}
