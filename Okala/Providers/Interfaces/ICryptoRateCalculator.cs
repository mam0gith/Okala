using CryptoRateApp.DTOs;

namespace Okala.Providers.Interfaces
{
    public interface ICryptoRateCalculator
    {
        IEnumerable<CryptoRateDto> CalculateRates(decimal usdPrice, Dictionary<string, decimal> eurRates);
    }


}
