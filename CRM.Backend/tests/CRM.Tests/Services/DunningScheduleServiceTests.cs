// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for <see cref="DunningScheduleService"/>.
/// BACK-010: Dunning Schedule CRUD.
/// </summary>
public sealed class DunningScheduleServiceTests : IDisposable
{
    private readonly CrmDbContext _context;
    private readonly DunningScheduleService _sut;

    public DunningScheduleServiceTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var config = new Mock<IConfiguration>();
        config.Setup(c => c["Jwt:Secret"]).Returns("test-secret-key-at-least-32-characters-long!");
        config.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
        config.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");
        config.Setup(c => c["Jwt:ExpirationMinutes"]).Returns("60");

        _context = new CrmDbContext(options, config.Object);
        _sut = new DunningScheduleService(_context, NullLogger<DunningScheduleService>.Instance);
    }

    public void Dispose() => _context.Dispose();

    // ─────────────────────────────────────────────────────────────────────────
    // CreateAsync
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_ShouldPersistStepAndReturnDto_WhenValidPayloadProvided()
    {
        // Arrange
        var dto = new CreateDunningScheduleDto
        {
            Name = "3-Day Reminder",
            DaysOverdue = 3,
            EmailSubject = "Payment Overdue",
            EmailBody = "Please pay your invoice within 24 hours.",
            IsActive = true,
            StepOrder = 1,
        };

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.Id.Should().BeGreaterThan(0);
        result.Name.Should().Be("3-Day Reminder");
        result.DaysOverdue.Should().Be(3);
        result.IsActive.Should().BeTrue();
        result.StepOrder.Should().Be(1);

        var inDb = await _context.DunningSchedules.FindAsync(result.Id);
        inDb.Should().NotBeNull();
        inDb!.IsDeleted.Should().BeFalse();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GetAllAsync
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_ShouldReturnOnlyActiveSteps_WhenActiveOnlyIsTrue()
    {
        // Arrange — seed two steps: one active, one inactive
        await _sut.CreateAsync(new CreateDunningScheduleDto
        {
            Name = "Active Step",
            DaysOverdue = 3,
            EmailSubject = "Sub Active",
            EmailBody = "Body Active",
            IsActive = true,
            StepOrder = 1,
        });

        await _sut.CreateAsync(new CreateDunningScheduleDto
        {
            Name = "Inactive Step",
            DaysOverdue = 7,
            EmailSubject = "Sub Inactive",
            EmailBody = "Body Inactive",
            IsActive = false,
            StepOrder = 2,
        });

        // Act
        var results = await _sut.GetAllAsync(activeOnly: true);

        // Assert
        results.Should().HaveCount(1);
        results[0].Name.Should().Be("Active Step");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // UpdateAsync
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_ShouldModifyFields_WhenStepExists()
    {
        // Arrange
        var created = await _sut.CreateAsync(new CreateDunningScheduleDto
        {
            Name = "Original Name",
            DaysOverdue = 5,
            EmailSubject = "Original Subject",
            EmailBody = "Original body text.",
            IsActive = true,
            StepOrder = 1,
        });

        var updateDto = new UpdateDunningScheduleDto
        {
            Name = "Updated Name",
            IsActive = false,
        };

        // Act
        var updated = await _sut.UpdateAsync(created.Id, updateDto);

        // Assert
        updated.Name.Should().Be("Updated Name");
        updated.IsActive.Should().BeFalse();
        updated.DaysOverdue.Should().Be(5); // unchanged
    }

    // ─────────────────────────────────────────────────────────────────────────
    // DeleteAsync
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteStep_AndReturnTrueWhenFound()
    {
        // Arrange
        var created = await _sut.CreateAsync(new CreateDunningScheduleDto
        {
            Name = "Step To Delete",
            DaysOverdue = 10,
            EmailSubject = "Final Notice",
            EmailBody = "Your account will be suspended.",
            IsActive = true,
            StepOrder = 3,
        });

        // Act
        var deleted = await _sut.DeleteAsync(created.Id);

        // Assert
        deleted.Should().BeTrue();

        // The step should no longer appear via GetByIdAsync
        var found = await _sut.GetByIdAsync(created.Id);
        found.Should().BeNull();

        // But the record should still be in the database with IsDeleted = true
        var inDb = await _context.DunningSchedules.FindAsync(created.Id);
        inDb!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenStepDoesNotExist()
    {
        // Act
        var deleted = await _sut.DeleteAsync(99999);

        // Assert
        deleted.Should().BeFalse();
    }
}
