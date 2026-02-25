// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
//
// Spec: SPEC-CRM002-06 (Lead Nurture Campaigns)
// TODO-CRM002-06: Lead Nurture Campaign GET / DELETE — unit tests
//
// MANDATORY TEST RULE: All method signatures, namespaces, and field names
// verified against the actual source before writing these tests.
// Source files read: LeadService.cs, ILeadService.cs,
//   Lead.cs, MarketingCampaign.cs, ICrmDbContext.cs

using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for LeadService nurture-campaign methods (TODO-CRM002-06):
/// - GetNurtureCampaignAsync
/// - RemoveFromNurtureCampaignAsync
/// </summary>
public class LeadNurtureCampaignTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<IEntityEventDispatcher> _mockDispatcher;
    private readonly LeadService _service;

    public LeadNurtureCampaignTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _mockDispatcher = new Mock<IEntityEventDispatcher>();

        _service = new LeadService(
            _mockContext.Object,
            _mockDispatcher.Object,
            Mock.Of<ILogger<LeadService>>(),
            Mock.Of<IDuplicateDetectionService>());
    }

    // ────────────────────────────────────────────────────────────────────────
    // Helpers
    // ────────────────────────────────────────────────────────────────────────

    private void SetupLeads(List<Lead> leads)
    {
        var mockSet = MockDbSetFactory.CreateMockDbSet(leads);
        _mockContext.Setup(c => c.Set<Lead>()).Returns(mockSet.Object);
    }

    private void SetupCampaigns(List<MarketingCampaign> campaigns)
    {
        var mockSet = MockDbSetFactory.CreateMockDbSet(campaigns);
        _mockContext.Setup(c => c.MarketingCampaigns).Returns(mockSet.Object);
    }

    private static Lead CreateLead(int id, int? nurtureCampaignId = null) => new()
    {
        Id = id,
        FirstName = "Jane",
        LastName = "Doe",
        Email = "jane@example.com",
        NurtureCampaignId = nurtureCampaignId,
        Status = LeadLifecycleStatus.New,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
        IsDeleted = false
    };

    private static MarketingCampaign CreateCampaign(int id, string name = "Test Campaign") => new()
    {
        Id = id,
        Name = name,
        IsDeleted = false,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    // ────────────────────────────────────────────────────────────────────────
    // GetNurtureCampaignAsync tests
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetNurtureCampaignAsync_ShouldReturnNull_WhenLeadHasNoNurtureCampaign()
    {
        // Arrange
        var lead = CreateLead(id: 1, nurtureCampaignId: null);
        SetupLeads([lead]);

        // Act
        var result = await _service.GetNurtureCampaignAsync(1);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetNurtureCampaignAsync_ShouldReturnCampaign_WhenLeadIsEnrolled()
    {
        // Arrange
        var campaign = CreateCampaign(id: 42, name: "Email Drip Q2");
        var lead = CreateLead(id: 1, nurtureCampaignId: 42);
        SetupLeads([lead]);
        SetupCampaigns([campaign]);

        // Act
        var result = await _service.GetNurtureCampaignAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(42);
        result.Name.Should().Be("Email Drip Q2");
    }

    // ────────────────────────────────────────────────────────────────────────
    // RemoveFromNurtureCampaignAsync tests
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task RemoveFromNurtureCampaignAsync_ShouldClearNurtureCampaignId_WhenCampaignMatches()
    {
        // Arrange
        var lead = CreateLead(id: 1, nurtureCampaignId: 7);
        SetupLeads([lead]);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _service.RemoveFromNurtureCampaignAsync(leadId: 1, campaignId: 7);

        // Assert
        result.Should().BeTrue();
        lead.NurtureCampaignId.Should().BeNull();
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveFromNurtureCampaignAsync_ShouldReturnFalse_WhenCampaignDoesNotMatch()
    {
        // Arrange — lead is enrolled in campaign 7; trying to remove campaign 99
        var lead = CreateLead(id: 1, nurtureCampaignId: 7);
        SetupLeads([lead]);

        // Act
        var result = await _service.RemoveFromNurtureCampaignAsync(leadId: 1, campaignId: 99);

        // Assert
        result.Should().BeFalse();
        lead.NurtureCampaignId.Should().Be(7); // unchanged
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
