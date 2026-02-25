// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.DTOs.ITSM;
using CRM.Core.Entities.ITSM;
using CRM.Core.Exceptions;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services.ITSM;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services.ITSM;

/// <summary>
/// Unit tests for ProblemManagementService.
/// Tests the core service methods using InMemory EF Core.
/// TODO-ITSM-02: Verify ProblemManagementService implementation correctness.
/// </summary>
public class ProblemManagementServiceTests : IDisposable
{
    private readonly CrmDbContext _context;
    private readonly ProblemManagementService _service;

    public ProblemManagementServiceTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new CrmDbContext(options, null!);
        var logger = new Mock<ILogger<ProblemManagementService>>().Object;
        _service = new ProblemManagementService(_context, logger);
    }

    public void Dispose() => _context.Dispose();

    // -------------------------------------------------------------------------
    // DetermineCauseAsync tests
    // -------------------------------------------------------------------------

    [Fact]
    public async Task DetermineCauseAsync_ShouldSetRootCauseAndState_WhenCalled()
    {
        // Arrange
        var problem = new Problem
        {
            Number = "PRB000001",
            ShortDescription = "Network issue",
            Priority = ProblemPriority.High,
            State = ProblemState.Investigating,
            CreatedAt = DateTime.UtcNow
        };
        _context.Problems.Add(problem);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.DetermineCauseAsync(
            problem.ProblemId,
            "Faulty network switch in rack B",
            "Restart the switch daily",
            1);

        // Assert
        result.Should().NotBeNull();
        result.RootCause.Should().Be("Faulty network switch in rack B");
        result.Workaround.Should().Be("Restart the switch daily");

        var updated = await _context.Problems.FindAsync(problem.ProblemId);
        updated!.State.Should().Be(ProblemState.RootCauseAnalysis);
        updated.RootCause.Should().Be("Faulty network switch in rack B");
        updated.Workaround.Should().Be("Restart the switch daily");
        updated.ModifiedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task DetermineCauseAsync_ShouldThrowValidationException_WhenRootCauseIsEmpty()
    {
        // Arrange
        var problem = new Problem
        {
            Number = "PRB000002",
            ShortDescription = "DB failure",
            Priority = ProblemPriority.Critical,
            State = ProblemState.Investigating,
            CreatedAt = DateTime.UtcNow
        };
        _context.Problems.Add(problem);
        await _context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.DetermineCauseAsync(problem.ProblemId, "  ", null, 1));
    }

    [Fact]
    public async Task DetermineCauseAsync_ShouldThrowEntityNotFoundException_WhenProblemNotFound()
    {
        // Arrange – no data seeded

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(() =>
            _service.DetermineCauseAsync(9999, "Some root cause", null, 1));
    }

    // -------------------------------------------------------------------------
    // IdentifyTemporaryWorkaroundAsync tests
    // -------------------------------------------------------------------------

    [Fact]
    public async Task IdentifyTemporaryWorkaroundAsync_ShouldSetWorkaround_WhenCalled()
    {
        // Arrange
        var problem = new Problem
        {
            Number = "PRB000003",
            ShortDescription = "Storage latency",
            Priority = ProblemPriority.Medium,
            State = ProblemState.Investigating,
            CreatedAt = DateTime.UtcNow
        };
        _context.Problems.Add(problem);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.IdentifyTemporaryWorkaroundAsync(
            problem.ProblemId,
            "Use SSD cache tier as a temporary measure",
            1);

        // Assert
        result.Should().NotBeNull();
        result.Workaround.Should().Be("Use SSD cache tier as a temporary measure");

        var updated = await _context.Problems.FindAsync(problem.ProblemId);
        updated!.Workaround.Should().Be("Use SSD cache tier as a temporary measure");
        updated.ModifiedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task IdentifyTemporaryWorkaroundAsync_ShouldThrowValidationException_WhenWorkaroundIsEmpty()
    {
        // Arrange
        var problem = new Problem
        {
            Number = "PRB000004",
            ShortDescription = "Memory leak",
            Priority = ProblemPriority.High,
            State = ProblemState.Investigating,
            CreatedAt = DateTime.UtcNow
        };
        _context.Problems.Add(problem);
        await _context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.IdentifyTemporaryWorkaroundAsync(problem.ProblemId, "", 1));
    }

    [Fact]
    public async Task IdentifyTemporaryWorkaroundAsync_ShouldThrowEntityNotFoundException_WhenProblemNotFound()
    {
        // Arrange – no data

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(() =>
            _service.IdentifyTemporaryWorkaroundAsync(9999, "Any workaround", 1));
    }
}
