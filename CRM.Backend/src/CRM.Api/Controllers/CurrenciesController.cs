// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRM.Core.Ports.Input;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// Provides currency listing and conversion endpoints.
/// TODO-GAP-05
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class CurrenciesController : CrmControllerBase
{
    private readonly ICurrencyService _currencyService;
    private readonly ILogger<CurrenciesController> _logger;

    public CurrenciesController(ICurrencyService currencyService, ILogger<CurrenciesController> logger)
    {
        _currencyService = currencyService;
        _logger = logger;
    }

    /// <summary>
    /// Lists all supported currency codes.
    /// </summary>
    /// <returns>Array of ISO 4217 currency codes</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    public IActionResult GetSupportedCurrencies()
    {
        var currencies = _currencyService.GetSupportedCurrencies();
        return Ok(currencies);
    }

    /// <summary>
    /// Gets all exchange rates relative to a base currency.
    /// </summary>
    /// <param name="base">Base currency code (default: USD)</param>
    /// <param name="ct">Cancellation token</param>
    [HttpGet("rates")]
    [ProducesResponseType(typeof(IEnumerable<ExchangeRateDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetRates([FromQuery] string @base = "USD", CancellationToken ct = default)
    {
                var rates = await _currencyService.GetExchangeRatesAsync(@base, ct);
        return Ok(rates);
    }

    /// <summary>
    /// Converts an amount between two currencies.
    /// </summary>
    /// <param name="request">Conversion request body</param>
    /// <param name="ct">Cancellation token</param>
    [HttpPost("convert")]
    [ProducesResponseType(typeof(CurrencyConversionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Convert([FromBody] CurrencyConversionRequest request, CancellationToken ct = default)
    {
                if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var rateDto = await _currencyService.GetRateAsync(request.FromCurrency, request.ToCurrency, null, ct);
        var converted = await _currencyService.ConvertAsync(request.Amount, request.FromCurrency, request.ToCurrency, null, ct);

        return Ok(new CurrencyConversionResponse
        {
            Amount = request.Amount,
            FromCurrency = request.FromCurrency.ToUpperInvariant(),
            ToCurrency = request.ToCurrency.ToUpperInvariant(),
            ConvertedAmount = converted,
            Rate = rateDto?.Rate
        });
    }
}

#region Request / Response DTOs

/// <summary>Request body for POST /api/currencies/convert.</summary>
public class CurrencyConversionRequest
{
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Amount must be non-negative")]
    public decimal Amount { get; set; }

    [Required]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency code must be 3 characters")]
    public string FromCurrency { get; set; } = string.Empty;

    [Required]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency code must be 3 characters")]
    public string ToCurrency { get; set; } = string.Empty;
}

/// <summary>Response for POST /api/currencies/convert.</summary>
public class CurrencyConversionResponse
{
    public decimal Amount { get; set; }
    public string FromCurrency { get; set; } = string.Empty;
    public string ToCurrency { get; set; } = string.Empty;
    public decimal ConvertedAmount { get; set; }
    public decimal? Rate { get; set; }
}

#endregion
