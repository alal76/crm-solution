// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using Microsoft.Extensions.Logging;
using CRM.Core.Ports.Input;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Currency conversion service using hardcoded fallback rates (USD as base).
/// Rates are approximate and for demonstration purposes.
/// TODO-GAP-05
/// </summary>
public class CurrencyService : ICurrencyService
{
    private readonly ILogger<CurrencyService> _logger;

    /// <summary>
    /// Static fallback exchange rates relative to USD.
    /// Keys are currency codes; values are how many units equal 1 USD.
    /// </summary>
    private static readonly Dictionary<string, decimal> UsdBaseRates = new(StringComparer.OrdinalIgnoreCase)
    {
        { "USD", 1.0m },
        { "EUR", 0.92m },
        { "GBP", 0.79m },
        { "JPY", 150.0m },
        { "CAD", 1.36m },
        { "AUD", 1.53m },
        { "CHF", 0.90m },
        { "CNY", 7.24m },
        { "INR", 83.0m },
        { "MXN", 17.15m },
        { "BRL", 4.97m },
        { "SGD", 1.34m },
        { "HKD", 7.82m },
        { "NZD", 1.63m },
        { "SEK", 10.44m },
        { "NOK", 10.55m },
        { "DKK", 6.88m },
        { "ZAR", 18.63m },
        { "AED", 3.67m },
        { "SAR", 3.75m },
    };

    public CurrencyService(ILogger<CurrencyService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public Task<decimal> ConvertAsync(decimal amount, string fromCurrency, string toCurrency, DateTime? rateDate = null, CancellationToken ct = default)
    {
        if (string.Equals(fromCurrency, toCurrency, StringComparison.OrdinalIgnoreCase))
            return Task.FromResult(amount);

        var rate = GetCrossRate(fromCurrency, toCurrency);
        if (rate == null)
        {
            _logger.LogWarning("No exchange rate found for {FromCurrency} -> {ToCurrency}", fromCurrency, toCurrency);
            return Task.FromResult(amount); // Return original amount if rate not found
        }

        return Task.FromResult(Math.Round(amount * rate.Value, 2));
    }

    /// <inheritdoc />
    public Task<IEnumerable<ExchangeRateDto>> GetExchangeRatesAsync(string baseCurrency = "USD", CancellationToken ct = default)
    {
        var rates = new List<ExchangeRateDto>();
        var today = DateTime.UtcNow.Date;

        if (!UsdBaseRates.TryGetValue(baseCurrency, out var baseUsdRate) || baseUsdRate == 0)
        {
            _logger.LogWarning("Unsupported base currency: {BaseCurrency}", baseCurrency);
            return Task.FromResult<IEnumerable<ExchangeRateDto>>(rates);
        }

        foreach (var kvp in UsdBaseRates)
        {
            if (string.Equals(kvp.Key, baseCurrency, StringComparison.OrdinalIgnoreCase))
                continue;

            // Cross-rate: baseCurrency -> kvp.Key
            var crossRate = kvp.Value / baseUsdRate;
            rates.Add(new ExchangeRateDto
            {
                FromCurrency = baseCurrency.ToUpperInvariant(),
                ToCurrency = kvp.Key.ToUpperInvariant(),
                Rate = Math.Round(crossRate, 6),
                Date = today
            });
        }

        return Task.FromResult<IEnumerable<ExchangeRateDto>>(rates);
    }

    /// <inheritdoc />
    public Task<ExchangeRateDto?> GetRateAsync(string fromCurrency, string toCurrency, DateTime? date = null, CancellationToken ct = default)
    {
        var rate = GetCrossRate(fromCurrency, toCurrency);
        if (rate == null)
            return Task.FromResult<ExchangeRateDto?>(null);

        return Task.FromResult<ExchangeRateDto?>(new ExchangeRateDto
        {
            FromCurrency = fromCurrency.ToUpperInvariant(),
            ToCurrency = toCurrency.ToUpperInvariant(),
            Rate = rate.Value,
            Date = (date ?? DateTime.UtcNow).Date
        });
    }

    /// <inheritdoc />
    public IEnumerable<string> GetSupportedCurrencies()
    {
        return UsdBaseRates.Keys.OrderBy(k => k);
    }

    private decimal? GetCrossRate(string from, string to)
    {
        if (!UsdBaseRates.TryGetValue(from, out var fromUsdRate) || fromUsdRate == 0)
            return null;
        if (!UsdBaseRates.TryGetValue(to, out var toUsdRate))
            return null;

        // Convert: from -> USD -> to
        return Math.Round(toUsdRate / fromUsdRate, 6);
    }
}
