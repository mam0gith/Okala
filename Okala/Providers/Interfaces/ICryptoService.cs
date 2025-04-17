using CryptoRateApp.DTOs;

namespace Okala.Providers.Interfaces
{
    public interface ICryptoService
    {
        Task<IEnumerable<CryptoRateDto>> GetConvertedRatesAsync(string cryptoCode);
    }

}
