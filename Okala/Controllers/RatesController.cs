using CryptoRateApp.DTOs;
using Microsoft.AspNetCore.Mvc;
using Okala.Providers.Interfaces;

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

        try
        {
            var result = await _cryptoService.GetConvertedRatesAsync(cryptoCode.ToUpper());
            return Ok(result);
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(503, $"External API error: {ex.Message}");
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound($"Currency not found: {ex.Message}");
        }
        catch (Exception ex)
        {
            // You can log the error here as well
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}