// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

public class DuplicateDetectionServiceTests : IDisposable
{
    private readonly CrmDbContext _context;
    private readonly Mock<ILogger<DuplicateDetectionService>> _mockLogger;
    private readonly DuplicateDetectionService _service;

    public DuplicateDetectionServiceTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase($"DuplicateDetectionTestDb_{Guid.NewGuid()}")
            .Options;
        _context = new CrmDbContext(options, null);
        _mockLogger = new Mock<ILogger<DuplicateDetectionService>>();
        _service = new DuplicateDetectionService(_context, _mockLogger.Object);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task CheckForDuplicatesAsync_ShouldReturnEmpty_WhenNoActiveRulesExist()
    {
        var fields = new Dictionary<string, string?>
        {
            ["email"] = "test@example.com",
            ["firstName"] = "John"
        };

        var result = await _service.CheckForDuplicatesAsync("Contact", fields, excludeRecordId: null);

        result.Should().NotBeNull();
        result.Duplicates.Should().BeEmpty();
    }

    [Fact]
    public async Task CheckForDuplicatesAsync_ShouldReturnEmpty_WhenNoRulesForEntityType()
    {
        // Add a rule for Lead, but check Contact
        _context.DuplicateRules.Add(new DuplicateRule
        {
            Id = 1,
            Name = "Lead Email Rule",
            IsActive = true,
            EntityType = DuplicateEntityType.Lead,
            MatchThreshold = 80,
            Action = DuplicateAction.Block,
            RunOnCreate = true,
            RunOnUpdate = false,
            RunOnImport = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        var fields = new Dictionary<string, string?> { ["email"] = "test@example.com" };

        var result = await _service.CheckForDuplicatesAsync("Contact", fields, excludeRecordId: null);

        result.Duplicates.Should().BeEmpty();
    }

    [Fact]
    public async Task CheckForDuplicatesAsync_ShouldReturnEmpty_WhenRuleIsInactive()
    {
        _context.DuplicateRules.Add(new DuplicateRule
        {
            Id = 2,
            Name = "Inactive Rule",
            IsActive = false,
            EntityType = DuplicateEntityType.Contact,
            MatchThreshold = 80,
            Action = DuplicateAction.Warn,
            RunOnCreate = true,
            RunOnUpdate = false,
            RunOnImport = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        var fields = new Dictionary<string, string?> { ["email"] = "test@example.com" };

        var result = await _service.CheckForDuplicatesAsync("Contact", fields, excludeRecordId: null);

        result.Duplicates.Should().BeEmpty();
    }

    [Fact]
    public async Task CheckForDuplicatesAsync_ShouldReturnNonNull_WithEmptyFields()
    {
        var result = await _service.CheckForDuplicatesAsync("Lead", new Dictionary<string, string?>(), null);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CheckForDuplicatesAsync_ShouldHandleNullFieldValues()
    {
        var fields = new Dictionary<string, string?>
        {
            ["email"] = null,
            ["phone"] = null
        };

        var act = async () => await _service.CheckForDuplicatesAsync("Account", fields, null);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task CheckForDuplicatesAsync_ShouldReturnEmpty_WhenEntityTypeIsInvalid()
    {
        // An unknown entity type should be handled gracefully (no exception, empty duplicates)
        var fields = new Dictionary<string, string?> { ["email"] = "test@ex.com" };

        var result = await _service.CheckForDuplicatesAsync("NonExistentEntityTypeXYZ", fields);

        result.Should().NotBeNull();
        result.Duplicates.Should().BeEmpty();
    }

    [Fact]
    public async Task GetActiveRulesAsync_CanBeInvokedViaCheckForDuplicates_WithNoRules()
    {
        // Contact entity type is valid; with no rules the check should return cleanly
        var fields = new Dictionary<string, string?> { ["firstName"] = "Jane", ["lastName"] = "Doe" };

        var result = await _service.CheckForDuplicatesAsync("Contact", fields);

        result.Should().NotBeNull();
        result.HasDuplicates.Should().BeFalse();
    }
}
