using Okala.Application.DTOs;

namespace Okala.Application.Interfaces
{
    public interface ICryptoRateCalculator
    {
        IEnumerable<CryptoRateDto> CalculateRates(decimal usdPrice, Dictionary<string, decimal> eurRates);
    }


}
