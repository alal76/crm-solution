// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities.Events;
using CRM.Core.Entities.ITSM;
using CRM.Core.Exceptions;
using FluentAssertions;
using Xunit;

namespace CRM.Tests.Unit.Core;

/// <summary>
/// AP-059-P2D: Zero-mock behavioral unit tests for the Incident entity.
/// Covers Resolve (5), Close (3), Escalate (4), and General (3) — 15 total.
/// </summary>
public class IncidentEntityTests
{
    // ---------------------------------------------------------------------------
    #region Resolve Tests
    // ---------------------------------------------------------------------------

    [Fact]
    public void Resolve_ShouldSetStateToResolved_WhenInProgress()
    {
        // Arrange
        var incident = Incident.CreateForTesting(IncidentState.InProgress);

        // Act
        incident.Resolve("Fixed the issue");

        // Assert
        incident.State.Should().Be(IncidentState.Resolved);
    }

    [Fact]
    public void Resolve_ShouldSetResolvedAt_WhenCalled()
    {
        // Arrange
        var incident = Incident.CreateForTesting(IncidentState.InProgress);
        var before = DateTime.UtcNow.AddSeconds(-1);

        // Act
        incident.Resolve("Fixed the issue");

        // Assert
        incident.ResolvedAt.Should().NotBeNull();
        incident.ResolvedAt.Should().BeAfter(before);
    }

    [Fact]
    public void Resolve_ShouldRaiseIncidentResolvedEvent()
    {
        // Arrange
        var incident = Incident.CreateForTesting(IncidentState.InProgress);

        // Act
        incident.Resolve("Root cause identified and patched");

        // Assert
        incident.DomainEvents.Should().ContainSingle(e => e is IncidentResolvedEvent);
        var evt = incident.DomainEvents.OfType<IncidentResolvedEvent>().Single();
        evt.ResolutionSummary.Should().Be("Root cause identified and patched");
    }

    [Fact]
    public void Resolve_ShouldThrowBusinessRuleException_WhenAlreadyClosed()
    {
        // Arrange
        var incident = Incident.CreateForTesting(IncidentState.Closed);

        // Act
        Action act = () => incident.Resolve("Trying to resolve a closed incident");

        // Assert
        act.Should().Throw<BusinessRuleException>()
           .WithMessage("*Cannot resolve a closed incident*");
    }

    /// <summary>MANDATORY CRITICAL TEST — SLA breach detected when resolved after due date.</summary>
    [Fact]
    public void Resolve_ShouldDetectSLABreach_WhenResolvedAfterDueDate()
    {
        // Arrange — due date is 2 hours in the past
        var pastDueDate = DateTime.UtcNow.AddHours(-2);
        var incident = Incident.CreateForTesting(IncidentState.InProgress, resolutionDueAt: pastDueDate);

        // Act
        incident.Resolve("Resolved but SLA was already missed");

        // Assert — SLABreached flag on entity
        incident.SLABreached.Should().BeTrue();

        // Assert — event carries SlaBreach = true
        var evt = incident.DomainEvents.OfType<IncidentResolvedEvent>().Single();
        evt.SlaBreach.Should().BeTrue();
    }

    #endregion Resolve Tests

    // ---------------------------------------------------------------------------
    #region Close Tests
    // ---------------------------------------------------------------------------

    [Fact]
    public void Close_ShouldSetStateToClosed_WhenResolved()
    {
        // Arrange
        var incident = Incident.CreateForTesting(IncidentState.Resolved);

        // Act
        incident.Close();

        // Assert
        incident.State.Should().Be(IncidentState.Closed);
    }

    [Fact]
    public void Close_ShouldRaiseIncidentClosedEvent()
    {
        // Arrange
        var incident = Incident.CreateForTesting(IncidentState.Resolved);

        // Act
        incident.Close("All checks passed");

        // Assert
        incident.DomainEvents.Should().ContainSingle(e => e is IncidentClosedEvent);
        var evt = incident.DomainEvents.OfType<IncidentClosedEvent>().Single();
        evt.Notes.Should().Be("All checks passed");
        evt.ClosedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Close_ShouldThrowBusinessRuleException_WhenStateIsNotResolved()
    {
        // Arrange
        var incident = Incident.CreateForTesting(IncidentState.InProgress);

        // Act
        Action act = () => incident.Close();

        // Assert
        act.Should().Throw<BusinessRuleException>()
           .WithMessage("*Incident must be resolved before closing*");
    }

    #endregion Close Tests

    // ---------------------------------------------------------------------------
    #region Escalate Tests
    // ---------------------------------------------------------------------------

    [Fact]
    public void Escalate_ShouldSetEscalationLevel()
    {
        // Arrange
        var incident = Incident.CreateForTesting(IncidentState.InProgress);

        // Act
        incident.Escalate(2, "Customer requested escalation to L2");

        // Assert
        incident.EscalationLevel.Should().Be(2);
    }

    [Fact]
    public void Escalate_ShouldRaiseIncidentEscalatedEvent()
    {
        // Arrange
        var incident = Incident.CreateForTesting(IncidentState.Assigned);

        // Act
        incident.Escalate(3, "Exceeds L2 capability");

        // Assert
        incident.DomainEvents.Should().ContainSingle(e => e is IncidentEscalatedEvent);
        var evt = incident.DomainEvents.OfType<IncidentEscalatedEvent>().Single();
        evt.EscalationLevel.Should().Be(3);
        evt.Reason.Should().Be("Exceeds L2 capability");
    }

    [Fact]
    public void Escalate_ShouldThrowBusinessRuleException_WhenClosed()
    {
        // Arrange
        var incident = Incident.CreateForTesting(IncidentState.Closed);

        // Act
        Action act = () => incident.Escalate(1, "Attempted escalation on closed incident");

        // Assert
        act.Should().Throw<BusinessRuleException>()
           .WithMessage("*Cannot escalate a closed or resolved incident*");
    }

    [Fact]
    public void Escalate_ShouldThrowBusinessRuleException_WhenResolved()
    {
        // Arrange
        var incident = Incident.CreateForTesting(IncidentState.Resolved);

        // Act
        Action act = () => incident.Escalate(1, "Attempted escalation on resolved incident");

        // Assert
        act.Should().Throw<BusinessRuleException>()
           .WithMessage("*Cannot escalate a closed or resolved incident*");
    }

    #endregion Escalate Tests

    // ---------------------------------------------------------------------------
    #region General Tests
    // ---------------------------------------------------------------------------

    [Fact]
    public void DomainEvents_ShouldBeEmpty_WhenNewlyCreated()
    {
        // Arrange & Act
        var incident = Incident.CreateForTesting(IncidentState.New);

        // Assert
        incident.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_ShouldNotDetectSLABreach_WhenResolvedBeforeDueDate()
    {
        // Arrange — due date is 2 hours in the future
        var futureDueDate = DateTime.UtcNow.AddHours(2);
        var incident = Incident.CreateForTesting(IncidentState.InProgress, resolutionDueAt: futureDueDate);

        // Act
        incident.Resolve("Resolved well before SLA deadline");

        // Assert
        incident.SLABreached.Should().BeFalse();

        var evt = incident.DomainEvents.OfType<IncidentResolvedEvent>().Single();
        evt.SlaBreach.Should().BeFalse();
    }

    [Fact]
    public void Resolve_ShouldThrowBusinessRuleException_WhenAlreadyResolved()
    {
        // Arrange
        var incident = Incident.CreateForTesting(IncidentState.Resolved);

        // Act
        Action act = () => incident.Resolve("Trying to resolve again");

        // Assert
        act.Should().Throw<BusinessRuleException>()
           .WithMessage("*Incident is already resolved*");
    }

    #endregion General Tests
}
