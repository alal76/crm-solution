// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services.AI;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services.AI;

/// <summary>
/// Unit tests for MeetingSummaryService (TODO-AI-08).
/// Covers: interaction not found → null; notes present → heuristic summary; AI port used when injected.
/// </summary>
public class MeetingSummaryServiceTests : ServiceTestFixtureBase<MeetingSummaryService>
{    public MeetingSummaryServiceTests()
    {    }

    [Fact]
    public async Task GenerateSummaryAsync_ShouldReturnNull_WhenInteractionNotFound()
    {
        // Arrange
        MockContext.Setup(c => c.Interactions)
            .Returns(MockDbSetFactory.CreateMockDbSet(new List<Interaction>()).Object);

        var sut = new MeetingSummaryService(MockContext.Object, MockLogger.Object);

        // Act
        var result = await sut.GenerateSummaryAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GenerateSummaryAsync_ShouldReturnHeuristicSummary_WhenMeetingNotesPresent()
    {
        // Arrange
        var interactions = new List<Interaction>
        {
            new Interaction
            {
                Id = 1,
                IsDeleted = false,
                Subject = "Q4 Review",
                InteractionDate = DateTime.UtcNow.AddDays(-1),
                MeetingNotes = "Discussed renewal options. - Action: Send updated pricing by Friday",
                Attendees = "alice@co.com, bob@co.com"
            }
        };
        MockContext.Setup(c => c.Interactions)
            .Returns(MockDbSetFactory.CreateMockDbSet(interactions).Object);

        var sut = new MeetingSummaryService(MockContext.Object, MockLogger.Object); // no AI port

        // Act
        var result = await sut.GenerateSummaryAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.InteractionId.Should().Be(1);
        result.Summary.Should().NotBeNullOrEmpty();
        result.IsAiGenerated.Should().BeFalse();
    }

    [Fact]
    public async Task GenerateSummaryAsync_ShouldUseAiPort_WhenAiPortIsInjected()
    {
        // Arrange
        var interactions = new List<Interaction>
        {
            new Interaction
            {
                Id = 2,
                IsDeleted = false,
                Subject = "Demo call",
                InteractionDate = DateTime.UtcNow,
                Description = "Shows product features"
            }
        };
        MockContext.Setup(c => c.Interactions)
            .Returns(MockDbSetFactory.CreateMockDbSet(interactions).Object);

        var mockAiPort = new Mock<CRM.Core.Ports.Output.Providers.IAIPort>();
        mockAiPort
            .Setup(a => a.SummarizeAsync(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("AI-generated summary of the demo call");

        var sut = new MeetingSummaryService(MockContext.Object, MockLogger.Object, mockAiPort.Object);

        // Act
        var result = await sut.GenerateSummaryAsync(2);

        // Assert
        result.Should().NotBeNull();
        result!.IsAiGenerated.Should().BeTrue();
        result.Summary.Should().Contain("AI-generated");
    }
}
