// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System;
using System.Linq;
using System.Threading.Tasks;
using CRM.Core.Entities;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for <see cref="ZipCodeService"/>.
/// Uses an InMemory <see cref="CrmDbContext"/> (concrete class required by constructor).
/// </summary>
public class ZipCodeServiceTests
{
    private static CrmDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase($"ZipCodeDb_{Guid.NewGuid()}")
            .Options;
        return new CrmDbContext(options, null!);
    }

    private static ZipCodeService BuildService(CrmDbContext context)
        => new(context, Mock.Of<ILogger<ZipCodeService>>());

    // ──────────────────────────────────────────────────────────────────
    // LookupByPostalCodeAsync — null / empty guard
    // ──────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task LookupByPostalCodeAsync_NullOrWhitespace_ReturnsEmpty(string? postalCode)
    {
        await using var context = CreateContext();
        var service = BuildService(context);

        var result = await service.LookupByPostalCodeAsync(postalCode!);

        result.Should().BeEmpty();
    }

    // ──────────────────────────────────────────────────────────────────
    // LookupByPostalCodeAsync — exact match
    // ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task LookupByPostalCodeAsync_KnownPostalCode_ReturnsMatchingRecord()
    {
        // Arrange
        await using var context = CreateContext();
        context.ZipCodes.Add(new ZipCode
        {
            PostalCode = "90210",
            City = "Beverly Hills",
            State = "California",
            StateCode = "CA",
            Country = "United States",
            CountryCode = "US",
            IsActive = true
        });
        await context.SaveChangesAsync();

        var service = BuildService(context);

        // Act
        var results = await service.LookupByPostalCodeAsync("90210");

        // Assert
        results.Should().ContainSingle();
        results.First().City.Should().Be("Beverly Hills");
    }

    [Fact]
    public async Task LookupByPostalCodeAsync_WithCountryFilter_ReturnsFilteredResults()
    {
        // Arrange
        await using var context = CreateContext();
        context.ZipCodes.AddRange(
            new ZipCode { PostalCode = "10001", City = "New York", CountryCode = "US", Country = "United States", IsActive = true },
            new ZipCode { PostalCode = "10001", City = "SomeCity", CountryCode = "XX", Country = "OtherLand", IsActive = true });
        await context.SaveChangesAsync();

        var service = BuildService(context);

        // Act
        var results = await service.LookupByPostalCodeAsync("10001", "US");

        // Assert
        results.Should().ContainSingle(r => r.CountryCode == "US");
    }

    [Fact]
    public async Task LookupByPostalCodeAsync_InactiveRecord_NotReturned()
    {
        // Arrange
        await using var context = CreateContext();
        context.ZipCodes.Add(new ZipCode
        {
            PostalCode = "99999",
            City = "Ghost Town",
            CountryCode = "US",
            Country = "United States",
            IsActive = false
        });
        await context.SaveChangesAsync();

        var service = BuildService(context);

        // Act
        var results = await service.LookupByPostalCodeAsync("99999");

        // Assert
        results.Should().BeEmpty();
    }

    // ──────────────────────────────────────────────────────────────────
    // GetZipCodeCountAsync
    // ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetZipCodeCountAsync_ReturnsCountOfActiveRecords()
    {
        // Arrange
        await using var context = CreateContext();
        context.ZipCodes.AddRange(
            new ZipCode { PostalCode = "A1", City = "Active1", CountryCode = "US", Country = "US", IsActive = true },
            new ZipCode { PostalCode = "A2", City = "Active2", CountryCode = "US", Country = "US", IsActive = true },
            new ZipCode { PostalCode = "I1", City = "Inactive", CountryCode = "US", Country = "US", IsActive = false });
        await context.SaveChangesAsync();

        var service = BuildService(context);

        // Act
        var count = await service.GetZipCodeCountAsync();

        // Assert — only active records counted
        count.Should().Be(2);
    }

    // ──────────────────────────────────────────────────────────────────
    // SearchByCityAsync
    // ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SearchByCityAsync_ShortCity_ReturnsEmpty()
    {
        await using var context = CreateContext();
        var service = BuildService(context);

        var result = await service.SearchByCityAsync("A"); // < 2 chars

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchByCityAsync_MatchingCity_ReturnsResults()
    {
        // Arrange
        await using var context = CreateContext();
        context.ZipCodes.AddRange(
            new ZipCode { PostalCode = "B1", City = "Boston", CountryCode = "US", Country = "US", IsActive = true },
            new ZipCode { PostalCode = "B2", City = "Boulder", CountryCode = "US", Country = "US", IsActive = true },
            new ZipCode { PostalCode = "D1", City = "Dallas", CountryCode = "US", Country = "US", IsActive = true });
        await context.SaveChangesAsync();

        var service = BuildService(context);

        // Act
        var results = await service.SearchByCityAsync("Bo");

        // Assert
        results.Should().HaveCount(2);
    }
}
