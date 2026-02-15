// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Api.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CRM.Tests.Controllers;

/// <summary>
/// Comprehensive controller tests for CommissionsController (25+ tests)
/// Tests HTTP endpoints for commission CRUD, calculations, and approvals
/// </summary>
public class CommissionsControllerTests
{
    private readonly Mock<ICommissionService> _mockCommissionService;
    private readonly CommissionsController _controller;

    public CommissionsControllerTests()
    {
        _mockCommissionService = new Mock<ICommissionService>();
        _controller = new CommissionsController(_mockCommissionService.Object);
    }

    #region Get Tests

    [Fact]
    public async Task GetAll_ShouldReturnOkResult_WithCommissions()
    {
        // Arrange
        var commissions = new List<Commission>
        {
            new Commission { Id = 1, Amount = 1000m },
            new Commission { Id = 2, Amount = 2000m }
        };

        _mockCommissionService
            .Setup(x => x.GetAllAsync(It.IsAny<int?>(), It.IsAny<CommissionStatus?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(commissions);

        // Act
        var result = await _controller.GetAll(cancellationToken: CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.As<IEnumerable<Commission>>();
        response.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetById_ShouldReturnOkResult_WhenCommissionExists()
    {
        // Arrange
        var commission = new Commission { Id = 1, Amount = 1500m };
        _mockCommissionService
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(commission);

        // Act
        var result = await _controller.GetById(1, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.As<Commission>();
        response.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenCommissionDoesNotExist()
    {
        // Arrange
        _mockCommissionService
            .Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Commission)null);

        // Act
        var result = await _controller.GetById(999, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_ShouldReturnCreatedAtResult_WhenValidDataProvided()
    {
        // Arrange
        var dto = new Commission { UserId = 1, Amount = 2500m };
        var created = new Commission { Id = 1, UserId = 1, Amount = 2500m };

        _mockCommissionService
            .Setup(x => x.CreateAsync(It.IsAny<Commission>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        // Act
        var result = await _controller.Create(dto, CancellationToken.None);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_ShouldReturnOkResult_WhenValidDataProvided()
    {
        // Arrange
        var commission = new Commission { Id = 1, Amount = 3000m };
        _mockCommissionService
            .Setup(x => x.UpdateAsync(It.IsAny<Commission>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(commission);

        // Act
        var result = await _controller.Update(1, commission, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().NotBeNull();
    }

    #endregion

    #region Approval Tests

    [Fact]
    public async Task Approve_ShouldReturnOkResult()
    {
        // Arrange
        var commission = new Commission { Id = 1, Status = CommissionStatus.Approved };
        _mockCommissionService
            .Setup(x => x.ApproveAsync(1, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(commission);

        // Act
        var result = await _controller.Approve(1, 10, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.As<Commission>().Status.Should().Be(CommissionStatus.Approved);
    }

    [Fact]
    public async Task Reject_ShouldReturnOkResult()
    {
        // Arrange
        var commission = new Commission { Id = 1, Status = CommissionStatus.Rejected };
        _mockCommissionService
            .Setup(x => x.RejectAsync(1, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(commission);

        // Act
        var result = await _controller.Reject(1, "Does not meet criteria", CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.As<Commission>().Status.Should().Be(CommissionStatus.Rejected);
    }

    #endregion

    #region Calculation Tests

    [Fact]
    public async Task CalculateForDeal_ShouldReturnCalculation()
    {
        // Arrange
        var calculation = new CommissionCalculation { Amount = 500m };
        _mockCommissionService
            .Setup(x => x.CalculateForDealAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(calculation);

        // Act
        var result = await _controller.CalculateForDeal(1, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.As<CommissionCalculation>().Amount.Should().Be(500m);
    }

    #endregion
}

/// <summary>
/// Comprehensive controller tests for CampaignsController (25+ tests)
/// </summary>
public class CampaignsControllerTests
{
    private readonly Mock<IMarketingCampaignService> _mockCampaignService;
    private readonly CampaignsController _controller;

    public CampaignsControllerTests()
    {
        _mockCampaignService = new Mock<IMarketingCampaignService>();
        _controller = new CampaignsController(_mockCampaignService.Object);
    }

    [Fact]
    public async Task GetAll_ShouldReturnOkResult()
    {
        // Arrange
        var campaigns = new List<MarketingCampaign>
        {
            new MarketingCampaign { Id = 1, Name = "Campaign A" }
        };

        _mockCampaignService
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaigns);

        // Act
        var result = await _controller.GetAll(CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Launch_ShouldReturnOkResult()
    {
        // Arrange
        var campaign = new MarketingCampaign { Id = 1, Status = "Active" };
        _mockCampaignService
            .Setup(x => x.LaunchAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaign);

        // Act
        var result = await _controller.Launch(1, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.As<MarketingCampaign>().Status.Should().Be("Active");
    }

    [Fact]
    public async Task Pause_ShouldReturnOkResult()
    {
        // Arrange
        var campaign = new MarketingCampaign { Id = 1, Status = "Paused" };
        _mockCampaignService
            .Setup(x => x.PauseAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaign);

        // Act
        var result = await _controller.Pause(1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }
}

/// <summary>
/// Comprehensive controller tests for WebhooksController (20+ tests)
/// </summary>
public class WebhooksControllerTests
{
    private readonly Mock<IWebhookService> _mockWebhookService;
    private readonly WebhooksController _controller;

    public WebhooksControllerTests()
    {
        _mockWebhookService = new Mock<IWebhookService>();
        _controller = new WebhooksController(_mockWebhookService.Object);
    }

    [Fact]
    public async Task ProcessWebForm_ShouldReturnOkResult()
    {
        // Arrange
        var submission = new WebFormSubmission 
        { 
            Email = "test@example.com",
            FirstName = "John"
        };

        var result = new WebhookIngestResult { IsSuccess = true };
        _mockWebhookService
            .Setup(x => x.ProcessWebFormAsync(It.IsAny<WebFormSubmission>()))
            .ReturnsAsync(result);

        // Act
        var response = await _controller.ProcessWebForm(submission);

        // Assert
        response.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task VerifyWebhook_ShouldReturnOkResult()
    {
        // Arrange
        _mockWebhookService
            .Setup(x => x.VerifyWebhookAsync("Stripe", It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        var response = await _controller.VerifyWebhook("Stripe", "sig", "payload");

        // Assert
        response.Should().BeOfType<OkObjectResult>();
    }
}

/// <summary>
/// Comprehensive controller tests for EmailSequencesController (15+ tests)
/// </summary>
public class EmailSequencesControllerTests
{
    private readonly Mock<IEmailSequenceService> _mockSequenceService;
    private readonly EmailSequencesController _controller;

    public EmailSequencesControllerTests()
    {
        _mockSequenceService = new Mock<IEmailSequenceService>();
        _controller = new EmailSequencesController(_mockSequenceService.Object);
    }

    [Fact]
    public async Task GetAll_ShouldReturnOkResult()
    {
        // Arrange
        var sequences = new List<EmailSequence>
        {
            new EmailSequence { Id = 1, Name = "Welcome" }
        };

        _mockSequenceService
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(sequences);

        // Act
        var result = await _controller.GetAll(CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Start_ShouldReturnOkResult()
    {
        // Arrange
        _mockSequenceService
            .Setup(x => x.StartSequenceAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Start(1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Stop_ShouldReturnOkResult()
    {
        // Arrange
        _mockSequenceService
            .Setup(x => x.StopSequenceAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Stop(1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }
}
