using Microsoft.AspNetCore.Mvc;
using Okala.Application.DTOs;
using Okala.Application.Interfaces;
using Okala.Infrastructure.Common.Models;

namespace Okala.Presentation.Controllers;

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
    public async Task<ActionResult<IEnumerable<CryptoRateDto>>> GetRates(string cryptoCode,CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(cryptoCode))
            return BadRequest("Invalid crypto code.");

        var result = await _cryptoService.GetConvertedRatesAsync(cryptoCode,cancellationToken);
        return Ok(Result<IEnumerable<CryptoRateDto>>.Success(result));
    }
}