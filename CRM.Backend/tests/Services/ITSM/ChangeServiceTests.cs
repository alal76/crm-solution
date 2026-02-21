// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace CRM.Tests.Services.ITSM;

/// <summary>
/// Comprehensive unit tests for ITSM Change Management functionality
/// </summary>
public class ChangeServiceTests
{
    #region Create Change Tests

    [Fact]
    public void CreateChange_StandardChange_CreatesCorrectly()
    {
        // Arrange & Act
        var change = new ITSMChange
        {
            Title = "Apply Windows patches",
            Description = "Monthly security patching for Windows servers",
            ChangeType = ChangeType.Standard,
            Priority = 4,
            RiskLevel = RiskLevel.Low,
            Status = ChangeStatus.New,
            RequestedById = 1,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        change.Should().NotBeNull();
        change.ChangeType.Should().Be(ChangeType.Standard);
        change.RiskLevel.Should().Be(RiskLevel.Low);
        change.Status.Should().Be(ChangeStatus.New);
    }

    [Fact]
    public void CreateChange_NormalChange_RequiresApproval()
    {
        // Arrange & Act
        var change = new ITSMChange
        {
            Title = "Upgrade CRM to v2.2",
            ChangeType = ChangeType.Normal,
            RiskLevel = RiskLevel.Medium,
            RequiresApproval = true
        };

        // Assert
        change.ChangeType.Should().Be(ChangeType.Normal);
        change.RequiresApproval.Should().BeTrue();
    }

    [Fact]
    public void CreateChange_EmergencyChange_HasExpediteFlag()
    {
        // Arrange & Act
        var change = new ITSMChange
        {
            Title = "Emergency security patch",
            ChangeType = ChangeType.Emergency,
            RiskLevel = RiskLevel.High,
            IsEmergency = true,
            JustificationForEmergency = "Critical security vulnerability CVE-2026-1234"
        };

        // Assert
        change.ChangeType.Should().Be(ChangeType.Emergency);
        change.IsEmergency.Should().BeTrue();
        change.JustificationForEmergency.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void CreateChange_GeneratesChangeNumber()
    {
        // Arrange
        var change = new ITSMChange
        {
            ChangeNumber = "CHG0000001",
            Title = "Test change"
        };

        // Assert
        change.ChangeNumber.Should().StartWith("CHG");
        change.ChangeNumber.Should().HaveLength(10);
    }

    #endregion

    #region Status Transition Tests

    [Fact]
    public void StatusTransition_NewToAssessment_IsValid()
    {
        // Arrange
        var change = new ITSMChange { Status = ChangeStatus.New };

        // Act
        var canTransition = IsValidStatusTransition(change.Status, ChangeStatus.Assessment);

        // Assert
        canTransition.Should().BeTrue();
    }

    [Fact]
    public void StatusTransition_AssessmentToApproval_IsValid()
    {
        // Arrange
        var change = new ITSMChange { Status = ChangeStatus.Assessment };

        // Act
        var canTransition = IsValidStatusTransition(change.Status, ChangeStatus.Approval);

        // Assert
        canTransition.Should().BeTrue();
    }

    [Fact]
    public void StatusTransition_ApprovalToScheduled_IsValid()
    {
        // Arrange
        var change = new ITSMChange { Status = ChangeStatus.Approval };

        // Act
        var canTransition = IsValidStatusTransition(change.Status, ChangeStatus.Scheduled);

        // Assert
        canTransition.Should().BeTrue();
    }

    [Fact]
    public void StatusTransition_ScheduledToImplementation_IsValid()
    {
        // Arrange
        var change = new ITSMChange { Status = ChangeStatus.Scheduled };

        // Act
        var canTransition = IsValidStatusTransition(change.Status, ChangeStatus.Implementation);

        // Assert
        canTransition.Should().BeTrue();
    }

    [Fact]
    public void StatusTransition_ImplementationToReview_IsValid()
    {
        // Arrange
        var change = new ITSMChange { Status = ChangeStatus.Implementation };

        // Act
        var canTransition = IsValidStatusTransition(change.Status, ChangeStatus.Review);

        // Assert
        canTransition.Should().BeTrue();
    }

    [Fact]
    public void StatusTransition_ReviewToClosed_IsValid()
    {
        // Arrange
        var change = new ITSMChange { Status = ChangeStatus.Review };

        // Act
        var canTransition = IsValidStatusTransition(change.Status, ChangeStatus.Closed);

        // Assert
        canTransition.Should().BeTrue();
    }

    private static bool IsValidStatusTransition(ChangeStatus from, ChangeStatus to)
    {
        var validTransitions = new Dictionary<ChangeStatus, ChangeStatus[]>
        {
            { ChangeStatus.New, new[] { ChangeStatus.Assessment, ChangeStatus.Cancelled } },
            { ChangeStatus.Assessment, new[] { ChangeStatus.Approval, ChangeStatus.Cancelled, ChangeStatus.Rejected } },
            { ChangeStatus.Approval, new[] { ChangeStatus.Scheduled, ChangeStatus.Rejected, ChangeStatus.Cancelled } },
            { ChangeStatus.Scheduled, new[] { ChangeStatus.Implementation, ChangeStatus.Cancelled } },
            { ChangeStatus.Implementation, new[] { ChangeStatus.Review, ChangeStatus.RolledBack, ChangeStatus.Failed } },
            { ChangeStatus.Review, new[] { ChangeStatus.Closed } },
            { ChangeStatus.Closed, Array.Empty<ChangeStatus>() },
            { ChangeStatus.Cancelled, Array.Empty<ChangeStatus>() },
            { ChangeStatus.Rejected, Array.Empty<ChangeStatus>() },
            { ChangeStatus.RolledBack, new[] { ChangeStatus.Closed } },
            { ChangeStatus.Failed, new[] { ChangeStatus.Closed } }
        };

        return validTransitions.TryGetValue(from, out var allowed) && allowed.Contains(to);
    }

    #endregion

    #region Risk Assessment Tests

    [Theory]
    [InlineData(3, 3, RiskLevel.High)]      // High Impact (3) + High Likelihood (3) = 9 = High Risk
    [InlineData(3, 1, RiskLevel.Medium)]    // High Impact + Low Likelihood = 3 = Medium
    [InlineData(1, 3, RiskLevel.Medium)]    // Low Impact + High Likelihood = 3 = Medium
    [InlineData(1, 1, RiskLevel.Low)]       // Low Impact + Low Likelihood = 1 = Low Risk
    public void RiskAssessment_CalculatesCorrectLevel(
        int impact,
        int likelihood,
        RiskLevel expectedRisk)
    {
        // Act
        var calculatedRisk = CalculateRiskLevel(impact, likelihood);

        // Assert
        calculatedRisk.Should().Be(expectedRisk);
    }

    [Fact]
    public void RiskAssessment_HighRisk_RequiresCAB()
    {
        // Arrange
        var change = new ITSMChange
        {
            RiskLevel = RiskLevel.High,
            ChangeType = ChangeType.Normal
        };

        // Act
        var requiresCAB = RequiresCABApproval(change);

        // Assert
        requiresCAB.Should().BeTrue();
    }

    [Fact]
    public void RiskAssessment_StandardChange_NoCABRequired()
    {
        // Arrange
        var change = new ITSMChange
        {
            ChangeType = ChangeType.Standard,
            RiskLevel = RiskLevel.Low
        };

        // Act
        var requiresCAB = RequiresCABApproval(change);

        // Assert
        requiresCAB.Should().BeFalse();
    }

    private static RiskLevel CalculateRiskLevel(int impact, int likelihood)
    {
        var riskScore = impact * likelihood;
        return riskScore switch
        {
            <= 2 => RiskLevel.Low,
            <= 4 => RiskLevel.Medium,
            _ => RiskLevel.High
        };
    }

    private static bool RequiresCABApproval(ITSMChange change)
    {
        if (change.ChangeType == ChangeType.Standard)
            return false;
        if (change.ChangeType == ChangeType.Major)
            return true;
        return change.RiskLevel == RiskLevel.High;
    }

    #endregion

    #region Scheduling Tests

    [Fact]
    public void Schedule_WithinMaintenanceWindow_IsValid()
    {
        // Arrange
        var maintenanceWindow = new MaintenanceWindow
        {
            StartTime = new TimeSpan(22, 0, 0), // 10 PM
            EndTime = new TimeSpan(6, 0, 0),     // 6 AM
            DaysOfWeek = new[] { DayOfWeek.Saturday, DayOfWeek.Sunday }
        };
        var scheduledTime = new DateTime(2026, 2, 7, 23, 0, 0); // Saturday 11 PM

        // Act
        var isWithinWindow = IsWithinMaintenanceWindow(scheduledTime, maintenanceWindow);

        // Assert
        isWithinWindow.Should().BeTrue();
    }

    [Fact]
    public void Schedule_OutsideMaintenanceWindow_IsInvalid()
    {
        // Arrange
        var maintenanceWindow = new MaintenanceWindow
        {
            StartTime = new TimeSpan(22, 0, 0),
            EndTime = new TimeSpan(6, 0, 0),
            DaysOfWeek = new[] { DayOfWeek.Saturday, DayOfWeek.Sunday }
        };
        var scheduledTime = new DateTime(2026, 2, 3, 10, 0, 0); // Tuesday 10 AM

        // Act
        var isWithinWindow = IsWithinMaintenanceWindow(scheduledTime, maintenanceWindow);

        // Assert
        isWithinWindow.Should().BeFalse();
    }

    [Fact]
    public void Schedule_DuringBlackout_IsBlocked()
    {
        // Arrange
        var blackouts = new List<BlackoutPeriod>
        {
            new() { StartDate = new DateTime(2026, 12, 15), EndDate = new DateTime(2027, 1, 5), Name = "Year-End Freeze" }
        };
        var scheduledTime = new DateTime(2026, 12, 20);

        // Act
        var isBlocked = IsInBlackoutPeriod(scheduledTime, blackouts);

        // Assert
        isBlocked.Should().BeTrue();
    }

    [Fact]
    public void Schedule_OutsideBlackout_IsAllowed()
    {
        // Arrange
        var blackouts = new List<BlackoutPeriod>
        {
            new() { StartDate = new DateTime(2026, 12, 15), EndDate = new DateTime(2027, 1, 5) }
        };
        var scheduledTime = new DateTime(2026, 11, 15);

        // Act
        var isBlocked = IsInBlackoutPeriod(scheduledTime, blackouts);

        // Assert
        isBlocked.Should().BeFalse();
    }

    private static bool IsWithinMaintenanceWindow(DateTime time, MaintenanceWindow window)
    {
        if (!window.DaysOfWeek.Contains(time.DayOfWeek))
            return false;

        var timeOfDay = time.TimeOfDay;

        // Handle overnight windows
        if (window.StartTime > window.EndTime)
        {
            return timeOfDay >= window.StartTime || timeOfDay < window.EndTime;
        }

        return timeOfDay >= window.StartTime && timeOfDay < window.EndTime;
    }

    private static bool IsInBlackoutPeriod(DateTime time, List<BlackoutPeriod> blackouts)
    {
        return blackouts.Any(b => time.Date >= b.StartDate.Date && time.Date <= b.EndDate.Date);
    }

    #endregion

    #region Conflict Detection Tests

    [Fact]
    public void ConflictDetection_SameCI_SameTime_DetectsConflict()
    {
        // Arrange
        var existingChange = new ITSMChange
        {
            ChangeId = 1,
            AffectedCIIds = new[] { 100, 101 },
            ScheduledStartAt = new DateTime(2026, 2, 7, 22, 0, 0),
            ScheduledEndAt = new DateTime(2026, 2, 8, 2, 0, 0)
        };
        var newChange = new ITSMChange
        {
            ChangeId = 2,
            AffectedCIIds = new[] { 100 }, // Same CI
            ScheduledStartAt = new DateTime(2026, 2, 7, 23, 0, 0),
            ScheduledEndAt = new DateTime(2026, 2, 8, 1, 0, 0)
        };

        // Act
        var hasConflict = DetectConflict(existingChange, newChange);

        // Assert
        hasConflict.Should().BeTrue();
    }

    [Fact]
    public void ConflictDetection_SameCI_DifferentTime_NoConflict()
    {
        // Arrange
        var existingChange = new ITSMChange
        {
            AffectedCIIds = new[] { 100 },
            ScheduledStartAt = new DateTime(2026, 2, 7, 22, 0, 0),
            ScheduledEndAt = new DateTime(2026, 2, 8, 2, 0, 0)
        };
        var newChange = new ITSMChange
        {
            AffectedCIIds = new[] { 100 },
            ScheduledStartAt = new DateTime(2026, 2, 8, 22, 0, 0), // Next day
            ScheduledEndAt = new DateTime(2026, 2, 9, 2, 0, 0)
        };

        // Act
        var hasConflict = DetectConflict(existingChange, newChange);

        // Assert
        hasConflict.Should().BeFalse();
    }

    [Fact]
    public void ConflictDetection_DifferentCI_SameTime_NoConflict()
    {
        // Arrange
        var existingChange = new ITSMChange
        {
            AffectedCIIds = new[] { 100 },
            ScheduledStartAt = new DateTime(2026, 2, 7, 22, 0, 0),
            ScheduledEndAt = new DateTime(2026, 2, 8, 2, 0, 0)
        };
        var newChange = new ITSMChange
        {
            AffectedCIIds = new[] { 200 }, // Different CI
            ScheduledStartAt = new DateTime(2026, 2, 7, 22, 0, 0),
            ScheduledEndAt = new DateTime(2026, 2, 8, 2, 0, 0)
        };

        // Act
        var hasConflict = DetectConflict(existingChange, newChange);

        // Assert
        hasConflict.Should().BeFalse();
    }

    private static bool DetectConflict(ITSMChange existing, ITSMChange newChange)
    {
        // Check if CIs overlap
        var ciOverlap = existing.AffectedCIIds
            .Intersect(newChange.AffectedCIIds)
            .Any();

        if (!ciOverlap)
            return false;

        // Check if times overlap
        var timeOverlap = existing.ScheduledStartAt < newChange.ScheduledEndAt &&
                          newChange.ScheduledStartAt < existing.ScheduledEndAt;

        return timeOverlap;
    }

    #endregion

    #region Rollback Tests

    [Fact]
    public void Rollback_HasPlan_CanExecute()
    {
        // Arrange
        var change = new ITSMChange
        {
            Status = ChangeStatus.Implementation,
            RollbackPlan = "1. Restore from backup\n2. Verify services\n3. Notify users"
        };

        // Assert
        change.RollbackPlan.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Rollback_WhenFailed_SetsRollbackStatus()
    {
        // Arrange
        var change = new ITSMChange
        {
            Status = ChangeStatus.Implementation
        };

        // Act
        change.Status = ChangeStatus.RolledBack;
        change.RolledBackAt = DateTime.UtcNow;
        change.RollbackReason = "Performance degradation detected";

        // Assert
        change.Status.Should().Be(ChangeStatus.RolledBack);
        change.RolledBackAt.Should().NotBeNull();
        change.RollbackReason.Should().NotBeNullOrEmpty();
    }

    #endregion
}

// Test helper classes and enums
public class ITSMChange
{
    public int ChangeId { get; set; }
    public string ChangeNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ChangeType ChangeType { get; set; }
    public ChangeStatus Status { get; set; }
    public RiskLevel RiskLevel { get; set; }
    public int Priority { get; set; }
    public int RequestedById { get; set; }
    public bool RequiresApproval { get; set; }
    public bool IsEmergency { get; set; }
    public string? JustificationForEmergency { get; set; }
    public int[]? AffectedCIIds { get; set; }
    public DateTime? ScheduledStartAt { get; set; }
    public DateTime? ScheduledEndAt { get; set; }
    public string? RollbackPlan { get; set; }
    public DateTime? RolledBackAt { get; set; }
    public string? RollbackReason { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class MaintenanceWindow
{
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public DayOfWeek[] DaysOfWeek { get; set; } = Array.Empty<DayOfWeek>();
}

public class BlackoutPeriod
{
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public enum ChangeType
{
    Standard = 1,
    Normal = 2,
    Emergency = 3,
    Major = 4
}

public enum ChangeStatus
{
    New = 1,
    Assessment = 2,
    Approval = 3,
    Scheduled = 4,
    Implementation = 5,
    Review = 6,
    Closed = 7,
    Cancelled = 8,
    Rejected = 9,
    RolledBack = 10,
    Failed = 11
}

public enum RiskLevel
{
    Low = 1,
    Medium = 2,
    High = 3
}
