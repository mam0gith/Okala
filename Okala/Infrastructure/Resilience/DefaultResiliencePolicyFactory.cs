﻿using Microsoft.Extensions.Options;
using Okala.Application.Interfaces.Resilience;
using Okala.Infrastructure.Configuration;
using Polly;

namespace Okala.Infrastructure.Resilience
{
    public class DefaultResiliencePolicyFactory : IResiliencePolicyFactory
    {
        private readonly ILogger<DefaultResiliencePolicyFactory> _logger;
        private readonly ResilienceSettings _settings;

        public DefaultResiliencePolicyFactory(ILogger<DefaultResiliencePolicyFactory> logger,
            IOptions<ResilienceSettings> settings)
        {
            _logger = logger;
            _settings = settings.Value;
        }

        public IAsyncPolicy<HttpResponseMessage> CreateResiliencePolicy()
        {
            // Retry Policy
            var retryPolicy = Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .OrResult(response => !response.IsSuccessStatusCode)
                .WaitAndRetryAsync(
                    retryCount: _settings.RetryCount,
                    sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    onRetry: (result, delay, retryCount, context) =>
                    {
                        _logger.LogWarning($"{context.OperationKey} Retry {retryCount} due to: {result.Exception?.Message ?? result.Result.StatusCode.ToString()}");
                    });

            // Circuit Breaker Policy
            var circuitBreakerPolicy = Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .OrResult(response => !response.IsSuccessStatusCode)
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: _settings.ExceptionsAllowedBeforeBreaking,
                    durationOfBreak: TimeSpan.FromSeconds(_settings.BreakDurationSeconds),
                    onBreak: (result, breakDelay, context) =>
                    {
                        _logger.LogError($"{context.OperationKey} Circuit broken! Will not attempt for {breakDelay.TotalSeconds} seconds. Reason: {result.Exception?.Message ?? result.Result.StatusCode.ToString()}");
                    },
                    onReset: (context) =>
                    {
                        _logger.LogInformation("{context.OperationKey} Circuit reset - requests allowed again");
                    },
                    onHalfOpen: () =>
                    {
                        _logger.LogInformation("{context.OperationKey} Circuit in test mode - one request will be allowed");
                    });

            // Wrap the policies
            return Policy.WrapAsync(retryPolicy, circuitBreakerPolicy);
        }
    }
}
