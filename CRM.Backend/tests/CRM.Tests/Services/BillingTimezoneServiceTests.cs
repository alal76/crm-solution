// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for <see cref="BillingTimezoneService"/>.
///
/// TODO-SALES006-023: Verify timezone-aware billing date calculations including
/// DST spring-forward, DST fall-back, monthly/annual cycles, and unknown-timezone
/// fallback behaviour.
/// </summary>
public class BillingTimezoneServiceTests
{
    private readonly BillingTimezoneService _sut;

    public BillingTimezoneServiceTests()
    {
        _sut = new BillingTimezoneService(NullLogger<BillingTimezoneService>.Instance);
    }

    // -------------------------------------------------------------------------
    // ConvertBillingDateToUtc — UTC passthrough
    // -------------------------------------------------------------------------

    [Fact]
    public void ConvertBillingDateToUtc_ShouldReturnSameTime_WhenTimezoneIsUtc()
    {
        // Arrange
        var local = new DateTime(2025, 6, 1, 12, 0, 0);

        // Act
        var result = _sut.ConvertBillingDateToUtc(local, "UTC");

        // Assert
        result.Kind.Should().Be(DateTimeKind.Utc);
        result.Should().Be(new DateTime(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc));
    }

    // -------------------------------------------------------------------------
    // ConvertBillingDateToUtc — DST spring-forward (invalid local time)
    // Eastern Time (America/New_York): clocks spring forward at 02:00 → 03:00
    // on the second Sunday of March each year.
    // 2025-03-09 02:30 EST is an invalid clock time.
    // -------------------------------------------------------------------------

    [Fact]
    public void ConvertBillingDateToUtc_ShouldAdvancePastGap_WhenLocalTimeIsInvalid_SpringForward()
    {
        // Skip if the IANA timezone cannot be loaded (e.g. unusual container images).
        TimeZoneInfo? tz = null;
        try { tz = TimeZoneInfo.FindSystemTimeZoneById("America/New_York"); }
        catch (TimeZoneNotFoundException) { return; }

        // Arrange — 02:30 on 2025-03-09 does not exist in Eastern Time.
        var invalidLocal = new DateTime(2025, 3, 9, 2, 30, 0);
        tz.IsInvalidTime(invalidLocal).Should().BeTrue("the test date must be in the spring-forward gap");

        // Act
        var result = _sut.ConvertBillingDateToUtc(invalidLocal, "America/New_York");

        // Assert — result must be a valid UTC datetime (no exception) and land
        // in the DST-adjusted window (03:30 EST = 07:30 UTC on that day).
        result.Kind.Should().Be(DateTimeKind.Utc);
        result.Should().BeOnOrAfter(new DateTime(2025, 3, 9, 7, 0, 0, DateTimeKind.Utc));
    }

    // -------------------------------------------------------------------------
    // ConvertBillingDateToUtc — DST fall-back (ambiguous local time)
    // Eastern Time (America/New_York): clocks fall back at 02:00 → 01:00
    // on the first Sunday of November.
    // 2024-11-03 01:30 is ambiguous (occurs once in EDT, once in EST).
    // -------------------------------------------------------------------------

    [Fact]
    public void ConvertBillingDateToUtc_ShouldUseStandardOffset_WhenLocalTimeIsAmbiguous_FallBack()
    {
        TimeZoneInfo? tz = null;
        try { tz = TimeZoneInfo.FindSystemTimeZoneById("America/New_York"); }
        catch (TimeZoneNotFoundException) { return; }

        // Arrange — 01:30 on 2024-11-03 occurs twice in Eastern Time.
        var ambiguousLocal = new DateTime(2024, 11, 3, 1, 30, 0);
        tz.IsAmbiguousTime(ambiguousLocal).Should().BeTrue("the test date must be in the fall-back window");

        // Act
        var result = _sut.ConvertBillingDateToUtc(ambiguousLocal, "America/New_York");

        // Assert — using the standard (EST, UTC-5) offset gives 06:30 UTC,
        // which is the later/more conservative interpretation.
        result.Kind.Should().Be(DateTimeKind.Utc);
        // EST (standard) offset is -5 → 01:30 + 5h = 06:30 UTC
        result.Should().Be(new DateTime(2024, 11, 3, 6, 30, 0, DateTimeKind.Utc));
    }

    // -------------------------------------------------------------------------
    // GetNextBillingDate — monthly cycle
    // -------------------------------------------------------------------------

    [Fact]
    public void GetNextBillingDate_ShouldAdvanceOneMonth_WhenCycleIsMonthly()
    {
        // Arrange
        var utcNow = new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var result = _sut.GetNextBillingDate(utcNow, "UTC", BillingPeriod.Monthly);

        // Assert — next billing midnight UTC is 2025-02-15
        result.Kind.Should().Be(DateTimeKind.Utc);
        result.Date.Should().Be(new DateTime(2025, 2, 15));
    }

    // -------------------------------------------------------------------------
    // GetNextBillingDate — annual cycle
    // -------------------------------------------------------------------------

    [Fact]
    public void GetNextBillingDate_ShouldAdvanceOneYear_WhenCycleIsYearly()
    {
        // Arrange
        var utcNow = new DateTime(2025, 3, 1, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var result = _sut.GetNextBillingDate(utcNow, "UTC", BillingPeriod.Yearly);

        // Assert
        result.Kind.Should().Be(DateTimeKind.Utc);
        result.Date.Should().Be(new DateTime(2026, 3, 1));
    }

    // -------------------------------------------------------------------------
    // Unknown timezone → falls back to UTC
    // -------------------------------------------------------------------------

    [Fact]
    public void ConvertBillingDateToUtc_ShouldFallBackToUtc_WhenTimezoneIsUnknown()
    {
        // Arrange
        var local = new DateTime(2025, 8, 20, 9, 0, 0);

        // Act — must not throw
        var result = _sut.ConvertBillingDateToUtc(local, "Region/NonExistent_TZ_XYZ");

        // Assert — falls back to UTC, so local and UTC values are identical.
        result.Kind.Should().Be(DateTimeKind.Utc);
        result.Should().Be(new DateTime(2025, 8, 20, 9, 0, 0, DateTimeKind.Utc));
    }
}
