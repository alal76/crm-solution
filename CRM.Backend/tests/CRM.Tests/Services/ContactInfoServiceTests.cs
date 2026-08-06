// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>Unit tests for ContactInfoService (TCOV-018).</summary>
public class ContactInfoServiceTests : IDisposable
{
    private readonly CrmDbContext _context;
    private readonly ContactInfoService _service;

    public ContactInfoServiceTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase($"ContactInfoTestDb_{Guid.NewGuid()}")
            .Options;
        _context = new CrmDbContext(options, null);
        _service = new ContactInfoService(_context);
    }

    public void Dispose() => _context.Dispose();

    // ── Address tests ─────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAddressAsync_ShouldPersistAddress()
    {
        var dto = new CreateAddressDto
        {
            Line1 = "123 Main St",
            City = "Springfield",
            State = "IL",
            PostalCode = "62701",
            CountryCode = "US"
        };

        var result = await _service.CreateAddressAsync(dto, createdByUserId: 1);

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.City.Should().Be("Springfield");
    }

    [Fact]
    public async Task GetAddressByIdAsync_ShouldReturnNull_WhenAddressDoesNotExist()
    {
        var result = await _service.GetAddressByIdAsync(9999);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAddressByIdAsync_ShouldReturnAddress_WhenExists()
    {
        var dto = new CreateAddressDto { Line1 = "456 Oak Ave", City = "Denver", State = "CO", PostalCode = "80201" };
        var created = await _service.CreateAddressAsync(dto);

        var result = await _service.GetAddressByIdAsync(created.Id);

        result.Should().NotBeNull();
        result!.City.Should().Be("Denver");
    }

    [Fact]
    public async Task GetAddressesAsync_ShouldReturnEmpty_WhenNoLinksExist()
    {
        var result = await _service.GetAddressesAsync(EntityType.Account, entityId: 1);
        result.Should().BeEmpty();
    }

    // ── Phone tests ───────────────────────────────────────────────────────

    [Fact]
    public async Task CreatePhoneNumberAsync_ShouldPersistPhone()
    {
        var dto = new CreatePhoneNumberDto
        {
            Number = "+1-555-0100",
            CountryCode = "+1"
        };

        var result = await _service.CreatePhoneNumberAsync(dto, createdByUserId: 1);

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Number.Should().Be("+1-555-0100");
    }

    [Fact]
    public async Task GetPhoneNumbersAsync_ShouldReturnEmpty_WhenNoLinksExist()
    {
        var result = await _service.GetPhoneNumbersAsync(EntityType.Contact, entityId: 1);
        result.Should().BeEmpty();
    }

    // ── Email tests ───────────────────────────────────────────────────────

    [Fact]
    public async Task CreateEmailAddressAsync_ShouldPersistEmail()
    {
        var dto = new CreateEmailAddressDto
        {
            Email = "test@example.com"
        };

        var result = await _service.CreateEmailAddressAsync(dto, createdByUserId: 1);

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task FindEmailByAddressAsync_ShouldReturnNull_WhenEmailDoesNotExist()
    {
        var result = await _service.FindEmailByAddressAsync("nobody@nowhere.test");
        result.Should().BeNull();
    }
}
