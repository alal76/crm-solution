// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Ports.Input;

/// <summary>
/// DTO representing an exchange rate between two currencies.
/// </summary>
public class ExchangeRateDto
{
    /// <summary>Source currency code (e.g. "USD")</summary>
    public string FromCurrency { get; set; } = string.Empty;

    /// <summary>Target currency code (e.g. "EUR")</summary>
    public string ToCurrency { get; set; } = string.Empty;

    /// <summary>Exchange rate (1 unit of FromCurrency = Rate units of ToCurrency)</summary>
    public decimal Rate { get; set; }

    /// <summary>Date of the rate</summary>
    public DateTime Date { get; set; }
}

/// <summary>
/// Input port for multi-currency conversion operations.
/// TODO-GAP-05
/// </summary>
public interface ICurrencyService
{
    /// <summary>
    /// Converts an amount from one currency to another.
    /// </summary>
    /// <param name="amount">Amount to convert</param>
    /// <param name="fromCurrency">Source currency code (e.g. "USD")</param>
    /// <param name="toCurrency">Target currency code (e.g. "EUR")</param>
    /// <param name="rateDate">Optional historical rate date; uses latest if null</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Converted amount</returns>
    Task<decimal> ConvertAsync(decimal amount, string fromCurrency, string toCurrency, DateTime? rateDate = null, CancellationToken ct = default);

    /// <summary>
    /// Gets all available exchange rates relative to a base currency.
    /// </summary>
    /// <param name="baseCurrency">Base currency code (default: "USD")</param>
    /// <param name="ct">Cancellation token</param>
    Task<IEnumerable<ExchangeRateDto>> GetExchangeRatesAsync(string baseCurrency = "USD", CancellationToken ct = default);

    /// <summary>
    /// Gets the exchange rate between two specific currencies.
    /// </summary>
    /// <param name="fromCurrency">Source currency code</param>
    /// <param name="toCurrency">Target currency code</param>
    /// <param name="date">Optional historical date; uses latest if null</param>
    /// <param name="ct">Cancellation token</param>
    Task<ExchangeRateDto?> GetRateAsync(string fromCurrency, string toCurrency, DateTime? date = null, CancellationToken ct = default);

    /// <summary>
    /// Returns all supported currency codes.
    /// </summary>
    IEnumerable<string> GetSupportedCurrencies();
}
