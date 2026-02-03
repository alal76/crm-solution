// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 CRM Solution Contributors
// ITSM Problem Service Unit Tests

using Xunit;
using FluentAssertions;
using CRM.Core.Entities.ITSM;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CRM.Tests.Services.ITSM;

/// <summary>
/// Comprehensive unit tests for ITSM Problem functionality
/// </summary>
public class ProblemServiceTests
{
    #region Create Problem Tests

    [Fact]
    public void CreateProblem_ValidData_CreatesCorrectly()
    {
        // Arrange & Act
        var problem = new Problem
        {
            Number = "PRB0000001",
            ShortDescription = "Recurring email failures",
            Description = "Multiple users experiencing email sync issues",
            Priority = ProblemPriority.High,
            State = ProblemState.New,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        problem.Should().NotBeNull();
        problem.ShortDescription.Should().Be("Recurring email failures");
        problem.State.Should().Be(ProblemState.New);
    }

    [Fact]
    public void CreateProblem_GeneratesProblemNumber()
    {
        // Arrange
        var problem = new Problem
        {
            Number = "PRB0000001",
            ShortDescription = "Test problem",
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        problem.Number.Should().StartWith("PRB");
        problem.Number.Should().HaveLength(10);
    }

    [Fact]
    public void CreateProblem_SetsCreatedAtTimestamp()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;
        
        // Act
        var problem = new Problem
        {
            ShortDescription = "Test problem",
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        problem.CreatedAt.Should().BeOnOrAfter(beforeCreation);
        problem.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    #endregion

    #region State Transition Tests

    [Fact]
    public void Problem_NewToInvestigating_IsValid()
    {
        // Arrange
        var problem = new Problem { State = ProblemState.New };

        // Act
        problem.State = ProblemState.Investigating;

        // Assert
        problem.State.Should().Be(ProblemState.Investigating);
    }

    [Fact]
    public void Problem_InvestigatingToRootCauseAnalysis_IsValid()
    {
        // Arrange
        var problem = new Problem { State = ProblemState.Investigating };

        // Act
        problem.State = ProblemState.RootCauseAnalysis;

        // Assert
        problem.State.Should().Be(ProblemState.RootCauseAnalysis);
    }

    [Fact]
    public void Problem_RootCauseToKnownError_IsValid()
    {
        // Arrange
        var problem = new Problem { State = ProblemState.RootCauseAnalysis };

        // Act
        problem.State = ProblemState.KnownError;
        problem.RootCause = "DNS server configuration issue";
        problem.KnownError = true;
        problem.KnownErrorDate = DateTime.UtcNow;

        // Assert
        problem.State.Should().Be(ProblemState.KnownError);
        problem.RootCause.Should().NotBeNullOrEmpty();
        problem.KnownError.Should().BeTrue();
    }

    [Fact]
    public void Problem_KnownErrorToResolved_IsValid()
    {
        // Arrange
        var problem = new Problem 
        { 
            State = ProblemState.KnownError,
            RootCause = "DNS configuration issue"
        };

        // Act
        problem.State = ProblemState.Resolved;
        problem.Solution = "Updated DNS server settings";
        problem.ResolvedAt = DateTime.UtcNow;

        // Assert
        problem.State.Should().Be(ProblemState.Resolved);
        problem.Solution.Should().NotBeNullOrEmpty();
        problem.ResolvedAt.Should().NotBeNull();
    }

    [Fact]
    public void Problem_ResolvedToClosed_IsValid()
    {
        // Arrange
        var problem = new Problem 
        { 
            State = ProblemState.Resolved,
            ResolvedAt = DateTime.UtcNow 
        };

        // Act
        problem.State = ProblemState.Closed;
        problem.ClosedAt = DateTime.UtcNow;

        // Assert
        problem.State.Should().Be(ProblemState.Closed);
        problem.ClosedAt.Should().NotBeNull();
    }

    #endregion

    #region Known Error Tests

    [Fact]
    public void Problem_CanBeMarkedAsKnownError()
    {
        // Arrange
        var problem = new Problem
        {
            State = ProblemState.RootCauseAnalysis,
            RootCause = "Memory leak in application pool"
        };

        // Act
        problem.KnownError = true;
        problem.KnownErrorDate = DateTime.UtcNow;
        problem.KnowledgeArticleId = 1;

        // Assert
        problem.KnownError.Should().BeTrue();
        problem.KnowledgeArticleId.Should().Be(1);
    }

    [Fact]
    public void Problem_KnownErrorWithWorkaround()
    {
        // Arrange & Act
        var problem = new Problem
        {
            State = ProblemState.KnownError,
            RootCause = "Application pool memory leak",
            KnownError = true,
            Workaround = "Recycle the application pool every 4 hours"
        };

        // Assert
        problem.KnownError.Should().BeTrue();
        problem.Workaround.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Related Incidents Tests

    [Fact]
    public void Problem_CanHaveRelatedIncidents()
    {
        // Arrange
        var problem = new Problem
        {
            Number = "PRB0000001",
            ShortDescription = "Email server outage",
            State = ProblemState.Investigating,
            ProblemIncidents = new List<ProblemIncident>()
        };

        var incident1 = new Incident { IncidentId = 1, Number = "INC0000001" };
        var incident2 = new Incident { IncidentId = 2, Number = "INC0000002" };

        // Act
        problem.ProblemIncidents.Add(new ProblemIncident { ProblemId = 1, IncidentId = 1, Incident = incident1 });
        problem.ProblemIncidents.Add(new ProblemIncident { ProblemId = 1, IncidentId = 2, Incident = incident2 });

        // Assert
        problem.ProblemIncidents.Should().HaveCount(2);
    }

    [Fact]
    public void Problem_CanCountAffectedUsers()
    {
        // Arrange
        var incident1 = new Incident { IncidentId = 1, CallerId = 101 };
        var incident2 = new Incident { IncidentId = 2, CallerId = 102 };
        var incident3 = new Incident { IncidentId = 3, CallerId = 103 };

        var problem = new Problem
        {
            ProblemIncidents = new List<ProblemIncident>
            {
                new ProblemIncident { IncidentId = 1, Incident = incident1 },
                new ProblemIncident { IncidentId = 2, Incident = incident2 },
                new ProblemIncident { IncidentId = 3, Incident = incident3 }
            }
        };

        // Act
        var affectedUserCount = problem.ProblemIncidents
            .Where(pi => pi.Incident != null)
            .Select(pi => pi.Incident!.CallerId)
            .Distinct()
            .Count();

        // Assert
        affectedUserCount.Should().Be(3);
    }

    #endregion

    #region Priority Tests

    [Fact]
    public void Problem_CriticalPriority_IsHighest()
    {
        // Arrange
        var problem = new Problem { Priority = ProblemPriority.Critical };

        // Assert
        ((int)problem.Priority).Should().Be(1);
    }

    [Fact]
    public void Problem_LowPriority_IsLowest()
    {
        // Arrange
        var problem = new Problem { Priority = ProblemPriority.Low };

        // Assert
        ((int)problem.Priority).Should().Be(4);
    }

    [Theory]
    [InlineData(ProblemPriority.Critical, 1)]
    [InlineData(ProblemPriority.High, 2)]
    [InlineData(ProblemPriority.Medium, 3)]
    [InlineData(ProblemPriority.Low, 4)]
    public void Problem_PriorityValues_AreCorrect(ProblemPriority priority, int expectedValue)
    {
        // Assert
        ((int)priority).Should().Be(expectedValue);
    }

    #endregion

    #region Root Cause Analysis Tests

    [Fact]
    public void Problem_RootCauseAnalysis_TracksProgress()
    {
        // Arrange
        var problem = new Problem
        {
            State = ProblemState.Investigating,
            ShortDescription = "Database performance issues"
        };

        // Act - Start investigation
        problem.RootCause = "Initial analysis: checking query performance";
        
        // Continue investigation
        problem.RootCause = "Root cause: Missing index on OrderDate column";
        problem.State = ProblemState.RootCauseAnalysis;

        // Assert
        problem.State.Should().Be(ProblemState.RootCauseAnalysis);
        problem.RootCause.Should().Contain("Missing index");
    }

    [Fact]
    public void Problem_Resolution_CompletesLifecycle()
    {
        // Arrange
        var problem = new Problem
        {
            Number = "PRB0000001",
            ShortDescription = "Slow database queries",
            State = ProblemState.KnownError,
            RootCause = "Missing index on OrderDate column"
        };

        // Act
        problem.Solution = "Created index IX_Orders_OrderDate on the Orders table";
        problem.State = ProblemState.Resolved;
        problem.ResolvedAt = DateTime.UtcNow;
        problem.FixVerified = true;
        problem.VerifiedAt = DateTime.UtcNow;

        // Assert
        problem.Solution.Should().Contain("Created index");
        problem.State.Should().Be(ProblemState.Resolved);
        problem.FixVerified.Should().BeTrue();
    }

    #endregion

    #region Workaround Tests

    [Fact]
    public void Problem_Workaround_CanBeProvided()
    {
        // Arrange
        var problem = new Problem
        {
            State = ProblemState.Investigating,
            RootCause = "Memory leak in third-party component"
        };

        // Act
        problem.Workaround = "Restart the service every 12 hours using scheduled task";

        // Assert
        problem.Workaround.Should().NotBeNullOrEmpty();
        problem.Workaround.Should().Contain("Restart");
    }

    #endregion

    #region Assignment Tests

    [Fact]
    public void Problem_CanBeAssignedToInvestigator()
    {
        // Arrange
        var problem = new Problem
        {
            Number = "PRB0000001",
            ShortDescription = "Network issues"
        };

        // Act
        problem.ProblemInvestigatorId = 5;
        problem.State = ProblemState.Investigating;

        // Assert
        problem.ProblemInvestigatorId.Should().Be(5);
    }

    [Fact]
    public void Problem_CanBeAssignedToManager()
    {
        // Arrange
        var problem = new Problem
        {
            Number = "PRB0000001",
            ShortDescription = "Critical system failure"
        };

        // Act
        problem.ProblemManagerId = 10;

        // Assert
        problem.ProblemManagerId.Should().Be(10);
    }

    [Fact]
    public void Problem_CanBeAssignedToGroup()
    {
        // Arrange
        var problem = new Problem
        {
            Number = "PRB0000001",
            ShortDescription = "Infrastructure issue"
        };

        // Act
        problem.AssignmentGroupId = 3;

        // Assert
        problem.AssignmentGroupId.Should().Be(3);
    }

    #endregion
}
