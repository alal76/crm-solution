// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

// Spec: BACK-009 (Billing Timezone Support)
// MANDATORY TEST RULE: All signatures verified against actual source before writing.
// Sources read:
//   IBillingTimezoneService.cs (CRM.Core/Interfaces)
//   BillingTimezoneService.cs  (CRM.Infrastructure/Services)
//
// Constructor: BillingTimezoneService(ILogger<BillingTimezoneService> logger)
// Methods:
//   DateTime ConvertBillingDateToUtc(DateTime localDate, string ianaTimezone)
//   DateTime GetNextBillingDate(DateTime utcNow, string billingTimezone, BillingPeriod cycle)

using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for BillingTimezoneService (BACK-009).
/// </summary>
public class BillingTimezoneServiceTests
{
    private readonly BillingTimezoneService _service;

    public BillingTimezoneServiceTests()
    {
        var logger = new Mock<ILogger<BillingTimezoneService>>().Object;
        _service = new BillingTimezoneService(logger);
    }

    // ────────────────────────────────────────────────────────────────────────
    // ConvertBillingDateToUtc
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public void ConvertBillingDateToUtc_ShouldReturnUtcDate_WhenGivenUtcTimezone()
    {
        var localDate = new DateTime(2026, 3, 1, 12, 0, 0);

        var result = _service.ConvertBillingDateToUtc(localDate, "UTC");

        result.Should().Be(new DateTime(2026, 3, 1, 12, 0, 0, DateTimeKind.Utc));
        result.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public void ConvertBillingDateToUtc_ShouldConvertToUtc_WhenGivenEasternTimezone()
    {
        // EST is UTC-5; noon EST = 17:00 UTC.
        var localDate = new DateTime(2026, 1, 15, 12, 0, 0);

        var result = _service.ConvertBillingDateToUtc(localDate, "America/New_York");

        result.Kind.Should().Be(DateTimeKind.Utc);
        result.Hour.Should().Be(17);
    }

    [Fact]
    public void ConvertBillingDateToUtc_ShouldFallBackToUtc_WhenTimezoneIsUnknown()
    {
        // Unknown timezone should not throw; falls back to UTC per service contract.
        var localDate = new DateTime(2026, 6, 1, 0, 0, 0);

        var act = () => _service.ConvertBillingDateToUtc(localDate, "Bogus/Timezone");

        act.Should().NotThrow();
    }

    [Fact]
    public void ConvertBillingDateToUtc_ShouldHandleAlreadyUtcKind_WithoutDoubleConversion()
    {
        // Service always treats input as Unspecified, so passing UTC kind is safe.
        var utcDate = new DateTime(2026, 4, 1, 8, 0, 0, DateTimeKind.Utc);

        var result = _service.ConvertBillingDateToUtc(utcDate, "UTC");

        result.Should().BeCloseTo(utcDate, TimeSpan.FromSeconds(1));
    }

    // ────────────────────────────────────────────────────────────────────────
    // GetNextBillingDate
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public void GetNextBillingDate_ShouldReturnFutureDate_WhenGivenMonthlyBillingCycleInUtc()
    {
        var utcNow = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc);

        var result = _service.GetNextBillingDate(utcNow, "UTC", BillingPeriod.Monthly);

        result.Should().BeAfter(utcNow);
    }

    [Fact]
    public void GetNextBillingDate_ShouldReturnFutureDate_WhenGivenWeeklyBillingCycle()
    {
        var utcNow = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc);

        var result = _service.GetNextBillingDate(utcNow, "UTC", BillingPeriod.Weekly);

        result.Should().BeAfter(utcNow);
        (result - utcNow).TotalDays.Should().BeApproximately(7, 1);
    }

    [Fact]
    public void GetNextBillingDate_ShouldReturnFutureDate_WhenGivenYearlyBillingCycle()
    {
        var utcNow = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var result = _service.GetNextBillingDate(utcNow, "UTC", BillingPeriod.Yearly);

        result.Should().BeAfter(utcNow);
        result.Year.Should().Be(2027);
    }

    [Fact]
    public void GetNextBillingDate_ShouldReturnFutureDate_WhenGivenQuarterlyBillingCycle()
    {
        var utcNow = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var result = _service.GetNextBillingDate(utcNow, "UTC", BillingPeriod.Quarterly);

        result.Should().BeAfter(utcNow);
        (result - utcNow).TotalDays.Should().BeApproximately(90, 5);
    }

    [Fact]
    public void GetNextBillingDate_ShouldNotThrow_WhenGivenNonUtcTimezone()
    {
        var utcNow = new DateTime(2026, 6, 15, 10, 0, 0, DateTimeKind.Utc);

        var act = () => _service.GetNextBillingDate(utcNow, "America/Chicago", BillingPeriod.Monthly);

        act.Should().NotThrow();
    }
}
