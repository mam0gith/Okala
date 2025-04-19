using Okala.Application.DTOs;

namespace Okala.Application.Interfaces
{
    public interface ICryptoService
    {
        Task<IEnumerable<CryptoRateDto>> GetConvertedRatesAsync(string cryptoCode, CancellationToken cancellationToken);
    }

}
