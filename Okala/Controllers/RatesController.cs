using CryptoRateApp.DTOs;
using Microsoft.AspNetCore.Mvc;
using Okala.Providers.Interfaces;
using Okala.Shared;

namespace CryptoRateApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CryptoController : ControllerBase
{
    private readonly ICryptoService _cryptoService;

    public CryptoController(ICryptoService cryptoService)
    {
        _cryptoService = cryptoService ?? throw new ArgumentNullException(nameof(cryptoService));
    }

    [HttpGet("{cryptoCode}")]
    public async Task<ActionResult<IEnumerable<CryptoRateDto>>> GetRates(string cryptoCode)
    {
        if (string.IsNullOrWhiteSpace(cryptoCode))
            return BadRequest("Invalid crypto code.");

        var result = await _cryptoService.GetConvertedRatesAsync(cryptoCode);
        return Ok(Result<IEnumerable<CryptoRateDto>>.Success(result));
    }
}