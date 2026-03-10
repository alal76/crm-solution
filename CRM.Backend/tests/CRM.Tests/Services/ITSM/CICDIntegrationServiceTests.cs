// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Interfaces.ITSM;
using CRM.Infrastructure.Services.ITSM;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services.ITSM;

/// <summary>
/// Unit tests for CICDIntegrationService (TCOV-004).
/// Uses an in-memory pipeline list; no external dependencies.
/// </summary>
public class CICDIntegrationServiceTests
{
    private readonly Mock<ILogger<CICDIntegrationService>> _mockLogger;
    private readonly CICDIntegrationService _service;

    public CICDIntegrationServiceTests()
    {
        _mockLogger = new Mock<ILogger<CICDIntegrationService>>();
        _service = new CICDIntegrationService(_mockLogger.Object);
    }

    [Fact]
    public void Constructor_ShouldCreateService_WithValidLogger()
    {
        _service.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateDeploymentChangeAsync_ShouldReturnSuccessResult()
    {
        var request = new DeploymentChangeRequestDto
        {
            PipelineId = "1",
            PipelineName = "TestPipeline",
            BuildNumber = "1.0.0",
            CommitHash = "abc123",
            CommitMessage = "Fix bug",
            Author = "dev@example.com",
            Branch = "main",
            Environment = "dev",
            Version = "1.0.0",
            DeploymentType = CRM.Core.Interfaces.ITSM.DeploymentType.Standard,
            AutoApprove = true
        };

        var result = await _service.CreateDeploymentChangeAsync(request);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.ChangeNumber.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateDeploymentChangeAsync_ShouldAutoApprove_ForDevEnvironment()
    {
        var request = new DeploymentChangeRequestDto
        {
            PipelineName = "Test",
            BuildNumber = "2.0.0",
            Environment = "dev",
            AutoApprove = true,
            DeploymentType = CRM.Core.Interfaces.ITSM.DeploymentType.Standard
        };

        var result = await _service.CreateDeploymentChangeAsync(request);

        result.IsApproved.Should().BeTrue();
    }

    [Fact]
    public async Task CreateDeploymentChangeAsync_ShouldRequireApproval_ForProductionEnvironment()
    {
        var request = new DeploymentChangeRequestDto
        {
            PipelineName = "Test",
            BuildNumber = "3.0.0",
            Environment = "production",
            AutoApprove = false,
            DeploymentType = CRM.Core.Interfaces.ITSM.DeploymentType.Standard
        };

        var result = await _service.CreateDeploymentChangeAsync(request);

        result.IsApproved.Should().BeFalse();
        result.Status.Should().Be("pending_approval");
    }

    [Fact]
    public async Task CreateDeploymentChangeAsync_ShouldGenerateChangeNumber()
    {
        var request = new DeploymentChangeRequestDto
        {
            PipelineName = "Test",
            BuildNumber = "4.0.0",
            Environment = "staging",
            DeploymentType = CRM.Core.Interfaces.ITSM.DeploymentType.Standard
        };

        var result = await _service.CreateDeploymentChangeAsync(request);

        result.ChangeNumber.Should().StartWith("CHG-");
    }

    [Fact]
    public async Task CreateDeploymentChangeAsync_ShouldAutoApprove_ForHotfixDeployment()
    {
        var request = new DeploymentChangeRequestDto
        {
            PipelineName = "Hotfix Pipeline",
            BuildNumber = "5.0.1",
            Environment = "production",
            AutoApprove = true,
            DeploymentType = CRM.Core.Interfaces.ITSM.DeploymentType.Hotfix
        };

        var result = await _service.CreateDeploymentChangeAsync(request);

        result.IsApproved.Should().BeTrue();
    }
}
