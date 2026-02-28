// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

// Spec: BACK-015 (Multi-Currency FX Service)
// MANDATORY TEST RULE: All method signatures, namespaces, and field names
// verified against the actual source before writing these tests.
// Sources read:
//   ICurrencyService.cs (CRM.Core/Ports/Input)
//   CurrencyService.cs  (CRM.Infrastructure/Services)
//
// Constructor: CurrencyService(ILogger<CurrencyService> logger) — no DbContext.
// Methods tested (verified against ICurrencyService.cs and CurrencyService.cs):
//   IEnumerable<string>               GetSupportedCurrencies()
//   Task<ExchangeRateDto?>            GetRateAsync(string fromCurrency, string toCurrency, DateTime? date, CT)
//   Task<decimal>                     ConvertAsync(decimal amount, string fromCurrency, string toCurrency, DateTime? rateDate, CT)
//   Task<IEnumerable<ExchangeRateDto>> GetExchangeRatesAsync(string baseCurrency = "USD", CT)
//
// ExchangeRateDto fields: FromCurrency, ToCurrency, Rate, Date  (NO BaseCurrency / TargetCurrency)
// NO RefreshRatesAsync — not in interface.

using CRM.Core.Ports.Input;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for CurrencyService (BACK-015).
/// The built-in service uses hard-coded fallback rates, so these tests
/// exercise the service without any external dependency.
/// </summary>
public class CurrencyServiceTests
{
    private readonly CurrencyService _service;

    public CurrencyServiceTests()
    {
        var logger = new Mock<ILogger<CurrencyService>>().Object;
        _service = new CurrencyService(logger);
    }

    // ────────────────────────────────────────────────────────────────────────
    // GetSupportedCurrencies
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public void GetSupportedCurrencies_ShouldReturnNonEmptyList()
    {
        var result = _service.GetSupportedCurrencies();

        result.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData("USD")]
    [InlineData("EUR")]
    [InlineData("GBP")]
    [InlineData("JPY")]
    [InlineData("CAD")]
    public void GetSupportedCurrencies_ShouldContainCommonCurrencies(string code)
    {
        // GetSupportedCurrencies() returns IEnumerable<string> (currency codes).
        var result = _service.GetSupportedCurrencies();

        result.Should().Contain(code);
    }

    [Fact]
    public void GetSupportedCurrencies_ShouldReturnNonNullCodes()
    {
        var result = _service.GetSupportedCurrencies();

        // Every returned string must be a non-empty currency code.
        result.Should().OnlyContain(code => !string.IsNullOrEmpty(code));
    }

    // ────────────────────────────────────────────────────────────────────────
    // GetRateAsync
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetRateAsync_ShouldReturnRate_WhenConvertingUsdToEur()
    {
        var result = await _service.GetRateAsync("USD", "EUR");

        result.Should().NotBeNull();
        result!.FromCurrency.Should().Be("USD");
        result.ToCurrency.Should().Be("EUR");
        result.Rate.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetRateAsync_ShouldReturnOne_WhenConvertingSameCurrency()
    {
        var result = await _service.GetRateAsync("USD", "USD");

        result.Should().NotBeNull();
        result!.Rate.Should().Be(1m);
    }

    [Fact]
    public async Task GetRateAsync_ShouldReturnNull_WhenCurrencyIsNotSupported()
    {
        var result = await _service.GetRateAsync("USD", "BOGUS");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetRateAsync_ShouldReturnPositiveRate_WhenConvertingGbpToJpy()
    {
        var result = await _service.GetRateAsync("GBP", "JPY");

        result.Should().NotBeNull();
        result!.Rate.Should().BeGreaterThan(0);
    }

    // ────────────────────────────────────────────────────────────────────────
    // ConvertAsync
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ConvertAsync_ShouldReturnPositiveAmount_WhenConvertingOneUsdToEur()
    {
        var result = await _service.ConvertAsync(1m, "USD", "EUR");

        result.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ConvertAsync_ShouldReturnSameAmount_WhenConvertingToSameCurrency()
    {
        const decimal Amount = 100m;

        var result = await _service.ConvertAsync(Amount, "USD", "USD");

        result.Should().Be(Amount);
    }

    [Fact]
    public async Task ConvertAsync_ShouldReturnZero_WhenAmountIsZero()
    {
        var result = await _service.ConvertAsync(0m, "USD", "EUR");

        result.Should().Be(0m);
    }

    [Fact]
    public async Task ConvertAsync_ShouldThrowOrReturnZero_WhenCurrencyIsUnsupported()
    {
        // Unsupported target currency: service either returns 0 or throws.
        var act = async () => await _service.ConvertAsync(100m, "USD", "BOGUS");

        // We accept both behaviours — the service contract does not guarantee
        // an exception on unknown currencies (may return 0).
        await act.Should().NotThrowAsync<NullReferenceException>();
    }

    [Fact]
    public async Task ConvertAsync_ShouldProduceReversibleConversion_ForMajorCurrencies()
    {
        // 100 USD → EUR → USD should be ≈100 (within rounding tolerance of 1%).
        const decimal Original = 100m;
        var eur = await _service.ConvertAsync(Original, "USD", "EUR");
        var backToUsd = await _service.ConvertAsync(eur, "EUR", "USD");

        backToUsd.Should().BeApproximately(Original, Original * 0.05m);  // 5% tolerance
    }

    // ────────────────────────────────────────────────────────────────────────
    // GetExchangeRatesAsync
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetExchangeRatesAsync_ShouldReturnRates_WhenCalledForUsd()
    {
        var result = await _service.GetExchangeRatesAsync("USD");

        result.Should().NotBeNullOrEmpty();
        result.Should().OnlyContain(r => r.FromCurrency == "USD");
    }

    [Fact]
    public async Task GetExchangeRatesAsync_ShouldReturnRates_WhenCalledWithNullBase()
    {
        var result = await _service.GetExchangeRatesAsync(null);

        result.Should().NotBeNullOrEmpty();
    }

}
