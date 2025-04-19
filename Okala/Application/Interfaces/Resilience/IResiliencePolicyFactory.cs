using Polly;

namespace Okala.Application.Interfaces.Resilience
{
    public interface IResiliencePolicyFactory
    {
        IAsyncPolicy<HttpResponseMessage> CreateResiliencePolicy();
    }
}