// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Ports.Input;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for CurrencyService.
/// No database dependency — all rates are hardcoded in the service.
/// </summary>
public class CurrencyServiceTests
{
    private readonly CurrencyService _service =
        new CurrencyService(new Mock<ILogger<CurrencyService>>().Object);

    // ── ConvertAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task ConvertAsync_ShouldReturnSameAmount_WhenCurrenciesAreEqual()
    {
        var result = await _service.ConvertAsync(100m, "USD", "USD");

        result.Should().Be(100m);
    }

    [Fact]
    public async Task ConvertAsync_ShouldConvertUsdToEur_UsingHardcodedRate()
    {
        // 1 USD = 0.92 EUR  → 100 USD ≈ 92 EUR
        var result = await _service.ConvertAsync(100m, "USD", "EUR");

        result.Should().BeApproximately(92m, 1m);
    }

    [Fact]
    public async Task ConvertAsync_ShouldConvertEurToUsd()
    {
        // 0.92 EUR = 1 USD → 92 EUR ≈ 100 USD
        var result = await _service.ConvertAsync(92m, "EUR", "USD");

        result.Should().BeApproximately(100m, 2m);
    }

    [Fact]
    public async Task ConvertAsync_ShouldReturnOriginalAmount_WhenCurrencyNotSupported()
    {
        var result = await _service.ConvertAsync(100m, "USD", "XYZ");

        result.Should().Be(100m); // fallback: return as-is
    }

    [Fact]
    public async Task ConvertAsync_ShouldHandleCaseInsensitiveCurrencyCodes()
    {
        var lower = await _service.ConvertAsync(100m, "usd", "eur");
        var upper = await _service.ConvertAsync(100m, "USD", "EUR");

        lower.Should().Be(upper);
    }

    // ── GetExchangeRatesAsync ────────────────────────────────────────────────

    [Fact]
    public async Task GetExchangeRatesAsync_ShouldReturnRatesForAllSupportedCurrencies_ExceptBase()
    {
        var rates = (await _service.GetExchangeRatesAsync("USD")).ToList();

        rates.Should().NotBeEmpty();
        rates.Should().AllSatisfy(r => r.FromCurrency.Should().Be("USD"));
        rates.Should().NotContain(r => r.ToCurrency == "USD");
    }

    [Fact]
    public async Task GetExchangeRatesAsync_ShouldReturnEmptyList_WhenBaseCurrencyUnsupported()
    {
        var rates = await _service.GetExchangeRatesAsync("XYZ");

        rates.Should().BeEmpty();
    }

    // ── GetRateAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetRateAsync_ShouldReturnRate_WhenBothCurrenciesAreSupported()
    {
        var rate = await _service.GetRateAsync("USD", "GBP");

        rate.Should().NotBeNull();
        rate!.FromCurrency.Should().Be("USD");
        rate.ToCurrency.Should().Be("GBP");
        rate.Rate.Should().BeApproximately(0.79m, 0.05m);
    }

    [Fact]
    public async Task GetRateAsync_ShouldReturnNull_WhenFromCurrencyUnsupported()
    {
        var rate = await _service.GetRateAsync("XYZ", "USD");

        rate.Should().BeNull();
    }

    // ── GetSupportedCurrencies ───────────────────────────────────────────────

    [Fact]
    public void GetSupportedCurrencies_ShouldReturn20Currencies()
    {
        var currencies = _service.GetSupportedCurrencies().ToList();

        currencies.Should().HaveCount(20);
        currencies.Should().Contain("USD");
        currencies.Should().Contain("EUR");
        currencies.Should().Contain("GBP");
    }

    [Fact]
    public void GetSupportedCurrencies_ShouldReturnSortedList()
    {
        var currencies = _service.GetSupportedCurrencies().ToList();
        var sorted = currencies.OrderBy(c => c).ToList();

        currencies.Should().Equal(sorted);
    }
}
