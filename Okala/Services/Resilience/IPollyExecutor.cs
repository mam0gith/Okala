using Polly;

namespace CryptoRateApp.Services.Resilience
{
    public interface IResiliencePolicyFactory
    {
        IAsyncPolicy<HttpResponseMessage> CreateResiliencePolicy();
    }
}