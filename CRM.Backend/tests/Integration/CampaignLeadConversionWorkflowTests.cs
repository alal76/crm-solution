// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CRM.Tests.Integration;

/// <summary>
/// Phase 3-3: Tests the campaign lead-nurture workflow:
/// Assign lead to nurture campaign → Query campaign → Remove from campaign.
/// Also covers MarketingCampaign CRUD and CampaignConversion tracking.
/// </summary>
public class CampaignLeadConversionWorkflowTests
{
    private readonly Mock<ILeadService> _leadSvc = new();
    private readonly Mock<IMarketingCampaignService> _campaignSvc = new();
    private readonly Mock<ICampaignConversionService> _conversionSvc = new();

    // ══════════════════════════════════════════════════════════════════
    // 1. Assign lead to nurture campaign
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public async Task AssignToNurtureCampaign_ReturnsTrue_WhenSuccessful()
    {
        _leadSvc.Setup(s => s.AssignToNurtureCampaignAsync(1, 10, default))
            .ReturnsAsync(true);

        var result = await _leadSvc.Object.AssignToNurtureCampaignAsync(1, 10);

        result.Should().BeTrue();
        _leadSvc.Verify(s => s.AssignToNurtureCampaignAsync(1, 10, default), Times.Once);
    }

    [Fact]
    public async Task AssignToNurtureCampaign_ReturnsFalse_WhenLeadNotFound()
    {
        _leadSvc.Setup(s => s.AssignToNurtureCampaignAsync(999, 10, default))
            .ReturnsAsync(false);

        var result = await _leadSvc.Object.AssignToNurtureCampaignAsync(999, 10);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task AssignToNurtureCampaign_ReturnsFalse_WhenCampaignNotFound()
    {
        _leadSvc.Setup(s => s.AssignToNurtureCampaignAsync(1, 999, default))
            .ReturnsAsync(false);

        var result = await _leadSvc.Object.AssignToNurtureCampaignAsync(1, 999);

        result.Should().BeFalse();
    }

    // ══════════════════════════════════════════════════════════════════
    // 2. Get nurture campaign for lead
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public async Task GetNurtureCampaign_ReturnsCampaign_WhenAssigned()
    {
        var campaign = new MarketingCampaign { Id = 10, Name = "Q1 Nurture" };
        _leadSvc.Setup(s => s.GetNurtureCampaignAsync(1, default))
            .ReturnsAsync(campaign);

        var result = await _leadSvc.Object.GetNurtureCampaignAsync(1);

        result.Should().NotBeNull();
        result!.Id.Should().Be(10);
        result.Name.Should().Be("Q1 Nurture");
    }

    [Fact]
    public async Task GetNurtureCampaign_ReturnsNull_WhenNotAssigned()
    {
        _leadSvc.Setup(s => s.GetNurtureCampaignAsync(1, default))
            .ReturnsAsync((MarketingCampaign?)null);

        var result = await _leadSvc.Object.GetNurtureCampaignAsync(1);

        result.Should().BeNull();
    }

    // ══════════════════════════════════════════════════════════════════
    // 3. Remove lead from nurture campaign
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public async Task RemoveFromNurtureCampaign_ReturnsTrue_WhenSuccessful()
    {
        _leadSvc.Setup(s => s.RemoveFromNurtureCampaignAsync(1, 10, default))
            .ReturnsAsync(true);

        var result = await _leadSvc.Object.RemoveFromNurtureCampaignAsync(1, 10);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task RemoveFromNurtureCampaign_ReturnsFalse_WhenCampaignDoesNotMatch()
    {
        // Lead is on campaign 10 but ask to remove from campaign 5
        _leadSvc.Setup(s => s.RemoveFromNurtureCampaignAsync(1, 5, default))
            .ReturnsAsync(false);

        var result = await _leadSvc.Object.RemoveFromNurtureCampaignAsync(1, 5);

        result.Should().BeFalse();
    }

    // ══════════════════════════════════════════════════════════════════
    // 4. Full nurture cycle: Assign → Get → Remove
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public async Task FullNurtureCycle_Assign_Get_Remove_AllSucceed()
    {
        var campaign = new MarketingCampaign { Id = 10, Name = "Q1 Nurture" };

        _leadSvc.Setup(s => s.AssignToNurtureCampaignAsync(1, 10, default)).ReturnsAsync(true);
        _leadSvc.Setup(s => s.GetNurtureCampaignAsync(1, default)).ReturnsAsync(campaign);
        _leadSvc.Setup(s => s.RemoveFromNurtureCampaignAsync(1, 10, default)).ReturnsAsync(true);

        var assigned = await _leadSvc.Object.AssignToNurtureCampaignAsync(1, 10);
        var fetched = await _leadSvc.Object.GetNurtureCampaignAsync(1);
        var removed = await _leadSvc.Object.RemoveFromNurtureCampaignAsync(1, fetched!.Id);

        assigned.Should().BeTrue();
        fetched.Should().NotBeNull();
        fetched.Id.Should().Be(10);
        removed.Should().BeTrue();

        _leadSvc.Verify(s => s.AssignToNurtureCampaignAsync(1, 10, default), Times.Once);
        _leadSvc.Verify(s => s.GetNurtureCampaignAsync(1, default), Times.Once);
        _leadSvc.Verify(s => s.RemoveFromNurtureCampaignAsync(1, 10, default), Times.Once);
    }

    // ══════════════════════════════════════════════════════════════════
    // 5. Marketing campaign CRUD
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Campaign_GetById_ReturnsDto()
    {
        var dto = new CampaignDto { Id = 10, Name = "Q1 Nurture" };
        _campaignSvc.Setup(s => s.GetCampaignByIdAsync(10)).ReturnsAsync(dto);

        var result = await _campaignSvc.Object.GetCampaignByIdAsync(10);

        result.Should().NotBeNull();
        result!.Id.Should().Be(10);
    }

    [Fact]
    public async Task Campaign_GetById_ReturnsNull_WhenNotFound()
    {
        _campaignSvc.Setup(s => s.GetCampaignByIdAsync(999)).ReturnsAsync((CampaignDto?)null);

        var result = await _campaignSvc.Object.GetCampaignByIdAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Campaign_GetAll_ReturnsList()
    {
        var campaigns = new List<CampaignDto>
        {
            new() { Id = 1, Name = "Q1" },
            new() { Id = 2, Name = "Q2" }
        };
        _campaignSvc.Setup(s => s.GetAllCampaignsAsync()).ReturnsAsync(campaigns);

        var result = await _campaignSvc.Object.GetAllCampaignsAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Campaign_GetActive_ReturnsOnlyActive()
    {
        var campaigns = new List<CampaignDto> { new() { Id = 1, Name = "Active" } };
        _campaignSvc.Setup(s => s.GetActiveCampaignsAsync()).ReturnsAsync(campaigns);

        var result = await _campaignSvc.Object.GetActiveCampaignsAsync();

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task Campaign_Create_ReturnsNewId()
    {
        _campaignSvc.Setup(s => s.CreateCampaignAsync(It.IsAny<CreateCampaignDto>()))
            .ReturnsAsync(42);

        var result = await _campaignSvc.Object.CreateCampaignAsync(new CreateCampaignDto
        {
            Name = "New Campaign"
        });

        result.Should().Be(42);
    }

    [Fact]
    public async Task Campaign_Update_DoesNotThrow()
    {
        _campaignSvc.Setup(s => s.UpdateCampaignAsync(It.IsAny<int>(), It.IsAny<UpdateCampaignDto>()))
            .Returns(Task.CompletedTask);

        await _campaignSvc.Object.UpdateCampaignAsync(10, new UpdateCampaignDto());

        _campaignSvc.Verify(s => s.UpdateCampaignAsync(10, It.IsAny<UpdateCampaignDto>()), Times.Once);
    }

    [Fact]
    public async Task Campaign_Delete_DoesNotThrow()
    {
        _campaignSvc.Setup(s => s.DeleteCampaignAsync(10))
            .Returns(Task.CompletedTask);

        await _campaignSvc.Object.DeleteCampaignAsync(10);

        _campaignSvc.Verify(s => s.DeleteCampaignAsync(10), Times.Once);
    }

    // ══════════════════════════════════════════════════════════════════
    // 6. Campaign conversion tracking
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public async Task CampaignConversion_Create_ReturnsDto()
    {
        var dto = new CampaignConversionDto { Id = 1, CampaignId = 10 };
        _conversionSvc.Setup(s => s.CreateAsync(It.IsAny<CreateCampaignConversionDto>(), default))
            .ReturnsAsync(dto);

        var result = await _conversionSvc.Object.CreateAsync(new CreateCampaignConversionDto
        {
            CampaignId = 10
        });

        result.Should().NotBeNull();
        result.CampaignId.Should().Be(10);
        _conversionSvc.Verify(s => s.CreateAsync(It.IsAny<CreateCampaignConversionDto>(), default), Times.Once);
    }

    [Fact]
    public async Task CampaignConversion_GetById_ReturnsDto()
    {
        var dto = new CampaignConversionDto { Id = 1, CampaignId = 10 };
        _conversionSvc.Setup(s => s.GetByIdAsync(1, default)).ReturnsAsync(dto);

        var result = await _conversionSvc.Object.GetByIdAsync(1);

        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
    }

    [Fact]
    public async Task CampaignConversion_GetById_ReturnsNull_WhenNotFound()
    {
        _conversionSvc.Setup(s => s.GetByIdAsync(999, default)).ReturnsAsync((CampaignConversionDto?)null);

        var result = await _conversionSvc.Object.GetByIdAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task CampaignConversion_GetByCampaignId_ReturnsList()
    {
        var list = new List<CampaignConversionDto>
        {
            new() { Id = 1, CampaignId = 10 },
            new() { Id = 2, CampaignId = 10 }
        };
        _conversionSvc.Setup(s => s.GetByCampaignIdAsync(10, default)).ReturnsAsync(list);

        var result = await _conversionSvc.Object.GetByCampaignIdAsync(10);

        result.Should().HaveCount(2);
        result.Should().AllSatisfy(c => c.CampaignId.Should().Be(10));
    }

    [Fact]
    public async Task CampaignConversion_GetAll_ReturnsPaged()
    {
        var items = new List<CampaignConversionDto> { new() { Id = 1 }, new() { Id = 2 } };
        _conversionSvc.Setup(s => s.GetAllAsync(
                It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), default))
            .ReturnsAsync((items, 2));

        var result = await _conversionSvc.Object.GetAllAsync(null, 1, 20);
        var (list, total) = result;

        list.Should().HaveCount(2);
        total.Should().Be(2);
    }

    [Fact]
    public async Task CampaignConversion_Update_ReturnsUpdatedDto()
    {
        var dto = new CampaignConversionDto { Id = 1, CampaignId = 10 };
        _conversionSvc.Setup(s => s.UpdateAsync(1, It.IsAny<UpdateCampaignConversionDto>(), default))
            .ReturnsAsync(dto);

        var result = await _conversionSvc.Object.UpdateAsync(1, new UpdateCampaignConversionDto());

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CampaignConversion_Delete_ReturnsTrue()
    {
        _conversionSvc.Setup(s => s.DeleteAsync(1, default)).ReturnsAsync(true);

        var result = await _conversionSvc.Object.DeleteAsync(1);

        result.Should().BeTrue();
    }

    // ══════════════════════════════════════════════════════════════════
    // 7. Full lead → campaign → conversion workflow
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public async Task FullWorkflow_LeadAssigned_ConversionTracked_ThenRemoved()
    {
        var campaign = new MarketingCampaign { Id = 10, Name = "Autumn Nurture" };
        var conversion = new CampaignConversionDto { Id = 1, CampaignId = 10 };

        _leadSvc.Setup(s => s.AssignToNurtureCampaignAsync(1, 10, default)).ReturnsAsync(true);
        _leadSvc.Setup(s => s.GetNurtureCampaignAsync(1, default)).ReturnsAsync(campaign);
        _conversionSvc.Setup(s => s.CreateAsync(It.IsAny<CreateCampaignConversionDto>(), default)).ReturnsAsync(conversion);
        _leadSvc.Setup(s => s.RemoveFromNurtureCampaignAsync(1, 10, default)).ReturnsAsync(true);

        // 1. Assign
        var assigned = await _leadSvc.Object.AssignToNurtureCampaignAsync(1, 10);
        assigned.Should().BeTrue();

        // 2. Confirm campaign
        var fetchedCampaign = await _leadSvc.Object.GetNurtureCampaignAsync(1);
        fetchedCampaign.Should().NotBeNull();

        // 3. Track conversion
        var createdConversion = await _conversionSvc.Object.CreateAsync(
            new CreateCampaignConversionDto { CampaignId = fetchedCampaign!.Id });
        createdConversion.Should().NotBeNull();
        createdConversion.CampaignId.Should().Be(10);

        // 4. Remove from campaign post-conversion
        var removed = await _leadSvc.Object.RemoveFromNurtureCampaignAsync(1, 10);
        removed.Should().BeTrue();
    }
}
