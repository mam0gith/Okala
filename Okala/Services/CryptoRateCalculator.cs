using CryptoRateApp.DTOs;
using Okala.Providers.Interfaces;

namespace Okala.Services
{
    public class CryptoRateCalculator : ICryptoRateCalculator
    {
        public IEnumerable<CryptoRateDto> CalculateRates(decimal usdPrice, Dictionary<string, decimal> eurRates)
        {
            var result = new List<CryptoRateDto>();

            if (!eurRates.TryGetValue("USD", out decimal eurToUsd) || eurToUsd <= 0)
                throw new InvalidOperationException("Invalid USD rate from EUR");

            result.Add(new CryptoRateDto("USD", Math.Round(usdPrice, 2)));

            foreach (var kvp in eurRates)
            {
                if (kvp.Key == "USD") continue;

                decimal usdToCurrency = kvp.Value / eurToUsd;
                if (usdToCurrency <= 0) continue;

                result.Add(new CryptoRateDto(kvp.Key, Math.Round(usdPrice * usdToCurrency, 2)));
            }

            return result;
        }
    }

}
