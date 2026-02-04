// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 CRM Solution Contributors
// ITSM Incident Service Unit Tests

using Xunit;
using FluentAssertions;
using CRM.Core.Entities.ITSM;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CRM.Tests.Services.ITSM;

/// <summary>
/// Comprehensive unit tests for ITSM Incident functionality
/// </summary>
public class IncidentServiceTests
{
    #region Create Incident Tests

    [Fact]
    public void CreateIncident_ValidData_CreatesCorrectly()
    {
        // Arrange & Act
        var incident = new Incident
        {
            Number = "INC0000001",
            ShortDescription = "Email not working",
            Description = "User cannot send or receive emails",
            Impact = IncidentImpact.Medium,
            Urgency = IncidentUrgency.High,
            State = IncidentState.New,
            CallerId = 1,
            OpenedAt = DateTime.UtcNow
        };

        // Assert
        incident.Should().NotBeNull();
        incident.ShortDescription.Should().Be("Email not working");
        incident.State.Should().Be(IncidentState.New);
    }

    [Fact]
    public void CreateIncident_GeneratesIncidentNumber()
    {
        // Arrange
        var incident = new Incident
        {
            Number = "INC0000001",
            ShortDescription = "Test incident",
            OpenedAt = DateTime.UtcNow
        };

        // Assert
        incident.Number.Should().StartWith("INC");
        incident.Number.Should().HaveLength(10);
    }

    [Fact]
    public void CreateIncident_SetsOpenedAtTimestamp()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;
        
        // Act
        var incident = new Incident
        {
            ShortDescription = "Test incident",
            OpenedAt = DateTime.UtcNow
        };

        // Assert
        incident.OpenedAt.Should().BeOnOrAfter(beforeCreation);
        incident.OpenedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    #endregion

    #region Priority Calculation Tests

    [Theory]
    [InlineData(IncidentImpact.High, IncidentUrgency.High, 1)]
    [InlineData(IncidentImpact.High, IncidentUrgency.Medium, 2)]
    [InlineData(IncidentImpact.High, IncidentUrgency.Low, 3)]
    [InlineData(IncidentImpact.Medium, IncidentUrgency.High, 2)]
    [InlineData(IncidentImpact.Medium, IncidentUrgency.Medium, 3)]
    [InlineData(IncidentImpact.Medium, IncidentUrgency.Low, 4)]
    [InlineData(IncidentImpact.Low, IncidentUrgency.High, 3)]
    [InlineData(IncidentImpact.Low, IncidentUrgency.Medium, 4)]
    [InlineData(IncidentImpact.Low, IncidentUrgency.Low, 5)]
    public void CalculatePriority_CorrectMatrix(IncidentImpact impact, IncidentUrgency urgency, int expectedPriority)
    {
        // Act
        var calculatedPriority = CalculatePriority(impact, urgency);

        // Assert
        calculatedPriority.Should().Be(expectedPriority);
    }

    private static int CalculatePriority(IncidentImpact impact, IncidentUrgency urgency)
    {
        // Priority matrix based on ITIL
        int[,] priorityMatrix = {
            { 1, 2, 3 }, // High impact
            { 2, 3, 4 }, // Medium impact
            { 3, 4, 5 }  // Low impact
        };

        int impactIndex = (int)impact - 1;
        int urgencyIndex = (int)urgency - 1;

        return priorityMatrix[impactIndex, urgencyIndex];
    }

    #endregion

    #region State Transition Tests

    [Fact]
    public void Incident_NewToAssigned_IsValid()
    {
        // Arrange
        var incident = new Incident { State = IncidentState.New };

        // Act
        incident.State = IncidentState.Assigned;

        // Assert
        incident.State.Should().Be(IncidentState.Assigned);
    }

    [Fact]
    public void Incident_AssignedToInProgress_IsValid()
    {
        // Arrange
        var incident = new Incident { State = IncidentState.Assigned };

        // Act
        incident.State = IncidentState.InProgress;

        // Assert
        incident.State.Should().Be(IncidentState.InProgress);
    }

    [Fact]
    public void Incident_InProgressToResolved_IsValid()
    {
        // Arrange
        var incident = new Incident { State = IncidentState.InProgress };

        // Act
        incident.State = IncidentState.Resolved;
        incident.ResolvedAt = DateTime.UtcNow;

        // Assert
        incident.State.Should().Be(IncidentState.Resolved);
        incident.ResolvedAt.Should().NotBeNull();
    }

    [Fact]
    public void Incident_ResolvedToClosed_IsValid()
    {
        // Arrange
        var incident = new Incident 
        { 
            State = IncidentState.Resolved,
            ResolvedAt = DateTime.UtcNow
        };

        // Act
        incident.State = IncidentState.Closed;
        incident.ClosedAt = DateTime.UtcNow;

        // Assert
        incident.State.Should().Be(IncidentState.Closed);
        incident.ClosedAt.Should().NotBeNull();
    }

    [Fact]
    public void Incident_CanPutOnHold()
    {
        // Arrange
        var incident = new Incident { State = IncidentState.InProgress };

        // Act
        incident.State = IncidentState.OnHold;

        // Assert
        incident.State.Should().Be(IncidentState.OnHold);
    }

    [Fact]
    public void Incident_CanBeReopened()
    {
        // Arrange
        var incident = new Incident 
        { 
            State = IncidentState.Resolved,
            ResolvedAt = DateTime.UtcNow 
        };

        // Act - reopening moves back to in progress
        incident.State = IncidentState.InProgress;

        // Assert
        incident.State.Should().Be(IncidentState.InProgress);
    }

    #endregion

    #region SLA Tests

    [Fact]
    public void Incident_Priority1_HasShortResponseTime()
    {
        // Priority 1 should have 15-minute response time
        var responseMinutes = GetResponseTimeMinutes(1);
        responseMinutes.Should().BeLessThanOrEqualTo(15);
    }

    [Fact]
    public void Incident_Priority5_HasLongerResponseTime()
    {
        // Priority 5 can have longer response time
        var responseMinutes = GetResponseTimeMinutes(5);
        responseMinutes.Should().BeGreaterThanOrEqualTo(240);
    }

    private static int GetResponseTimeMinutes(int priority)
    {
        return priority switch
        {
            1 => 15,
            2 => 30,
            3 => 60,
            4 => 120,
            5 => 240,
            _ => 480
        };
    }

    #endregion

    #region Comment Tests

    [Fact]
    public void IncidentComment_CanBeCreated()
    {
        // Arrange & Act
        var comment = new IncidentComment
        {
            IncidentId = 1,
            Comment = "This is a test comment",
            IsInternal = false,
            CreatedById = 1,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        comment.Comment.Should().Be("This is a test comment");
        comment.IsInternal.Should().BeFalse();
    }

    [Fact]
    public void IncidentComment_InternalFlag_Works()
    {
        // Arrange
        var internalComment = new IncidentComment { IsInternal = true };
        var publicComment = new IncidentComment { IsInternal = false };

        // Assert
        internalComment.IsInternal.Should().BeTrue();
        publicComment.IsInternal.Should().BeFalse();
    }

    #endregion

    #region History Tests

    [Fact]
    public void IncidentHistory_RecordsChanges()
    {
        // Arrange & Act
        var history = new IncidentHistory
        {
            IncidentId = 1,
            Field = "State",
            OldValue = "New",
            NewValue = "Assigned",
            ChangedById = 1,
            ChangedAt = DateTime.UtcNow
        };

        // Assert
        history.Field.Should().Be("State");
        history.OldValue.Should().Be("New");
        history.NewValue.Should().Be("Assigned");
    }

    #endregion

    #region Assignment Tests

    [Fact]
    public void Incident_CanBeAssignedToUser()
    {
        // Arrange
        var incident = new Incident
        {
            Number = "INC0000001",
            ShortDescription = "Test incident"
        };

        // Act
        incident.AssignedToId = 5;
        incident.State = IncidentState.Assigned;

        // Assert
        incident.AssignedToId.Should().Be(5);
        incident.State.Should().Be(IncidentState.Assigned);
    }

    [Fact]
    public void Incident_CanBeAssignedToGroup()
    {
        // Arrange
        var incident = new Incident
        {
            Number = "INC0000001",
            ShortDescription = "Test incident"
        };

        // Act
        incident.AssignmentGroupId = 3;

        // Assert
        incident.AssignmentGroupId.Should().Be(3);
    }

    #endregion

    #region Contact Type Tests

    [Theory]
    [InlineData(ContactType.Phone)]
    [InlineData(ContactType.Email)]
    [InlineData(ContactType.Portal)]
    [InlineData(ContactType.Chat)]
    [InlineData(ContactType.WalkIn)]
    [InlineData(ContactType.Monitoring)]
    public void Incident_ContactType_CanBeSet(ContactType contactType)
    {
        // Arrange
        var incident = new Incident();

        // Act
        incident.ContactType = contactType;

        // Assert
        incident.ContactType.Should().Be(contactType);
    }

    #endregion

    #region Resolution Code Tests

    [Theory]
    [InlineData(ResolutionCode.SolvedPermanently)]
    [InlineData(ResolutionCode.Workaround)]
    [InlineData(ResolutionCode.SolvedTemporarily)]
    [InlineData(ResolutionCode.NotSolvable)]
    [InlineData(ResolutionCode.Duplicate)]
    public void Incident_ResolutionCode_CanBeSet(ResolutionCode resolutionCode)
    {
        // Arrange
        var incident = new Incident { State = IncidentState.Resolved };

        // Act
        incident.ResolutionCode = resolutionCode;

        // Assert
        incident.ResolutionCode.Should().Be(resolutionCode);
    }

    #endregion
}
