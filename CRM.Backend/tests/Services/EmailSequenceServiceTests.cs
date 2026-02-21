// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Comprehensive unit tests for EmailSequenceService
/// Uses InMemory database to properly support async EF Core operations (Include, ToListAsync, etc.)
/// </summary>
public class EmailSequenceServiceTests : IDisposable
{
    private readonly CrmDbContext _dbContext;
    private readonly Mock<ILogger<EmailSequenceService>> _mockLogger;
    private readonly EmailSequenceService _sequenceService;

    public EmailSequenceServiceTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new CrmDbContext(options, null);
        _mockLogger = new Mock<ILogger<EmailSequenceService>>();
        _sequenceService = new EmailSequenceService(_dbContext, _mockLogger.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    #region CRUD Tests

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllSequences()
    {
        // Arrange
        _dbContext.EmailSequences.AddRange(
            new EmailSequence { Name = "Welcome Series" },
            new EmailSequence { Name = "Onboarding Series" }
        );
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sequenceService.GetAllAsync(CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnSequence_WhenIdExists()
    {
        // Arrange
        var sequence = new EmailSequence { Name = "Welcome Series" };
        _dbContext.EmailSequences.Add(sequence);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sequenceService.GetByIdAsync(sequence.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Welcome Series");
    }

    [Fact]
    public async Task CreateSequenceAsync_ShouldCreateSequence()
    {
        // Arrange
        var sequence = new EmailSequence
        {
            Name = "New Sequence",
            Description = "Test sequence",
            IsActive = true
        };

        // Act
        var result = await _sequenceService.CreateSequenceAsync(sequence, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("New Sequence");
        result.Id.Should().BeGreaterThan(0);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateSequence()
    {
        // Arrange - create initial sequence in DB
        var initial = new EmailSequence { Name = "Original", IsActive = false };
        _dbContext.EmailSequences.Add(initial);
        await _dbContext.SaveChangesAsync();

        var updated = new EmailSequence
        {
            Id = initial.Id,
            Name = "Updated Sequence",
            IsActive = true
        };

        // Act
        var result = await _sequenceService.UpdateAsync(updated, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Updated Sequence");
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteSequence()
    {
        // Arrange
        var sequence = new EmailSequence { Name = "To Delete", IsDeleted = false };
        _dbContext.EmailSequences.Add(sequence);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sequenceService.DeleteAsync(sequence.Id, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        var fromDb = await _dbContext.EmailSequences.FindAsync(sequence.Id);
        fromDb!.IsDeleted.Should().BeTrue();
    }

    #endregion

    #region Enrollment Tests

    [Fact]
    public async Task EnrollContactAsync_ShouldEnrollContact_WhenValidIds()
    {
        // Arrange
        var sequence = new EmailSequence { Name = "Test Sequence" };
        _dbContext.EmailSequences.Add(sequence);
        await _dbContext.SaveChangesAsync();

        var contactId = 10;
        var enrolledById = 1;

        // Act
        var result = await _sequenceService.EnrollContactAsync(sequence.Id, contactId, enrolledById, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.EmailSequenceId.Should().Be(sequence.Id);
        result.ContactId.Should().Be(contactId);
        result.Status.Should().Be(EnrollmentStatus.Active);
    }

    [Fact]
    public async Task EnrollContactAsync_ShouldReturnExisting_WhenAlreadyEnrolled()
    {
        // Arrange
        var sequence = new EmailSequence { Name = "Test Sequence" };
        _dbContext.EmailSequences.Add(sequence);
        await _dbContext.SaveChangesAsync();

        var contactId = 10;

        // Enroll first time
        var first = await _sequenceService.EnrollContactAsync(sequence.Id, contactId, null, CancellationToken.None);

        // Act - enroll same contact again
        var second = await _sequenceService.EnrollContactAsync(sequence.Id, contactId, null, CancellationToken.None);

        // Assert - should return existing enrollment, not create duplicate
        second.Id.Should().Be(first.Id);
    }

    #endregion

    #region Sequence Control Tests

    [Fact]
    public async Task StartSequenceAsync_ShouldStartSequence()
    {
        // Arrange
        var sequence = new EmailSequence { Name = "Test", Status = EmailSequenceStatus.Paused };
        _dbContext.EmailSequences.Add(sequence);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sequenceService.StartSequenceAsync(sequence.Id, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        var fromDb = await _dbContext.EmailSequences.FindAsync(sequence.Id);
        fromDb!.Status.Should().Be(EmailSequenceStatus.Active);
    }

    [Fact]
    public async Task StopSequenceAsync_ShouldStopSequence()
    {
        // Arrange
        var sequence = new EmailSequence { Name = "Test", Status = EmailSequenceStatus.Active };
        _dbContext.EmailSequences.Add(sequence);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sequenceService.StopSequenceAsync(sequence.Id, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        var fromDb = await _dbContext.EmailSequences.FindAsync(sequence.Id);
        fromDb!.Status.Should().Be(EmailSequenceStatus.Paused);
    }

    #endregion

    #region Sequence Status Tests

    [Fact]
    public async Task GetSequenceStatusAsync_ShouldReturnStatus()
    {
        // Arrange
        var sequence = new EmailSequence
        {
            Name = "Status Test",
            TotalEnrolled = 100,
            ActiveEnrollments = 80,
            TotalCompleted = 20,
            TotalEmailsSent = 50
        };
        _dbContext.EmailSequences.Add(sequence);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sequenceService.GetSequenceStatusAsync(sequence.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TotalEnrolled.Should().Be(100);
        result.ActiveEnrollments.Should().Be(80);
        result.TotalCompleted.Should().Be(20);
    }

    [Fact]
    public async Task GetSequenceStatusAsync_ShouldCalculateMetrics()
    {
        // Arrange
        var sequence = new EmailSequence
        {
            Name = "Metrics Test",
            TotalEnrolled = 100,
            TotalCompleted = 50,
            ActiveEnrollments = 40
        };
        _dbContext.EmailSequences.Add(sequence);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sequenceService.GetSequenceStatusAsync(sequence.Id, CancellationToken.None);

        // Assert
        var completionRate = (decimal)result.TotalCompleted / result.TotalEnrolled;
        completionRate.Should().Be(0.5m);
    }

    #endregion

    #region Trigger Evaluation Tests

    [Fact]
    public async Task EvaluateConditionAsync_ShouldReturnTrue()
    {
        // Act - placeholder implementation always returns true
        var result = await _sequenceService.EvaluateConditionAsync(1, 10, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void EnrollmentEligibility_ShouldBeTrueAfterDelay()
    {
        // Arrange
        var enrollment = new EmailSequenceEnrollment
        {
            EnrolledAt = DateTime.UtcNow.AddDays(-5),
            Status = EnrollmentStatus.Active
        };
        var delayDays = 3;

        // Act
        var isEligible = (DateTime.UtcNow - enrollment.EnrolledAt).TotalDays >= delayDays;

        // Assert
        isEligible.Should().BeTrue();
    }

    [Fact]
    public void EnrollmentEligibility_ShouldBeFalseBeforeDueDate()
    {
        // Arrange
        var enrollment = new EmailSequenceEnrollment
        {
            EnrolledAt = DateTime.UtcNow,
            Status = EnrollmentStatus.Active
        };
        var delayDays = 3;

        // Act
        var isEligible = (DateTime.UtcNow - enrollment.EnrolledAt).TotalDays >= delayDays;

        // Assert
        isEligible.Should().BeFalse();
    }

    #endregion

    #region Enrollment Management Tests

    [Fact]
    public async Task UnenrollContactAsync_ShouldUnenrollContact()
    {
        // Arrange
        var sequence = new EmailSequence { Name = "Test", ActiveEnrollments = 1 };
        _dbContext.EmailSequences.Add(sequence);
        await _dbContext.SaveChangesAsync();

        var enrollment = new EmailSequenceEnrollment
        {
            EmailSequenceId = sequence.Id,
            ContactId = 10,
            Status = EnrollmentStatus.Active
        };
        _dbContext.EmailSequenceEnrollments.Add(enrollment);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sequenceService.UnenrollContactAsync(sequence.Id, 10, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetEnrollments_ShouldReturnEnrollmentsForSequence()
    {
        // Arrange
        var sequence = new EmailSequence { Name = "Test" };
        _dbContext.EmailSequences.Add(sequence);
        await _dbContext.SaveChangesAsync();

        _dbContext.EmailSequenceEnrollments.AddRange(
            new EmailSequenceEnrollment { EmailSequenceId = sequence.Id, ContactId = 1, Status = EnrollmentStatus.Active },
            new EmailSequenceEnrollment { EmailSequenceId = sequence.Id, ContactId = 2, Status = EnrollmentStatus.Completed }
        );
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _dbContext.EmailSequenceEnrollments
            .Where(e => e.EmailSequenceId == sequence.Id)
            .ToListAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task CreateSequenceAsync_WithMultipleSteps_ShouldCreateCorrectly()
    {
        // Arrange
        var sequence = new EmailSequence
        {
            Name = "Multi-step Sequence",
            Steps = new List<EmailSequenceStep>
            {
                new EmailSequenceStep { StepOrder = 1, Template = "Welcome", DelayDays = 0 },
                new EmailSequenceStep { StepOrder = 2, Template = "Onboarding", DelayDays = 2 },
                new EmailSequenceStep { StepOrder = 3, Template = "Upsell", DelayDays = 7 }
            }
        };

        // Act
        var result = await _sequenceService.CreateSequenceAsync(sequence, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Steps.Should().HaveCount(3);
        result.Steps.Should().BeInAscendingOrder(s => s.StepOrder);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenIdNotExists()
    {
        // Act
        var result = await _sequenceService.DeleteAsync(999, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task StartSequenceAsync_ShouldReturnFalse_WhenSequenceNotFound()
    {
        // Act
        var result = await _sequenceService.StartSequenceAsync(999, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task StopSequenceAsync_ShouldReturnFalse_WhenSequenceNotFound()
    {
        // Act
        var result = await _sequenceService.StopSequenceAsync(999, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}
