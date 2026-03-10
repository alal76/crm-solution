// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using CRM.Tests.Helpers;
using FluentAssertions;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for CloudDeploymentService (TCOV-002).
/// </summary>
public class CloudDeploymentServiceTests : ServiceTestFixtureBase<CloudDeploymentService>
{
    private readonly Mock<ICrmDbContext> _mockDbContext;
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly CloudDeploymentService _service;

    public CloudDeploymentServiceTests()
    {
        _mockDbContext = new Mock<ICrmDbContext>();
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _service = new CloudDeploymentService(_mockDbContext.Object, MockLogger.Object, _mockHttpClientFactory.Object);
    }

    [Fact]
    public void Constructor_ShouldCreateService_WithValidDependencies()
    {
        _service.Should().NotBeNull();
    }

    [Fact]
    public async Task GetProvidersAsync_ShouldReturnEmpty_WhenNoProvidersExist()
    {
        _mockDbContext.Setup(c => c.CloudProviders)
            .Returns(MockDbSetFactory.CreateMockDbSet(new List<CloudProvider>()).Object);

        var result = await _service.GetProvidersAsync();

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetProviderByIdAsync_ShouldReturnNull_WhenProviderNotFound()
    {
        _mockDbContext.Setup(c => c.CloudProviders)
            .Returns(MockDbSetFactory.CreateMockDbSet(new List<CloudProvider>()).Object);

        var result = await _service.GetProviderByIdAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetProvidersAsync_ShouldReturnProvider_WhenProviderExists()
    {
        var providers = new List<CloudProvider>
        {
            new() { Id = 1, Name = "Test AWS", IsDeleted = false, ProviderType = CRM.Core.Entities.CloudProviderType.AWS }
        };
        _mockDbContext.Setup(c => c.CloudProviders)
            .Returns(MockDbSetFactory.CreateMockDbSet(providers).Object);

        var result = await _service.GetProvidersAsync();

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task DeleteProviderAsync_ShouldReturnFalse_WhenProviderNotFound()
    {
        _mockDbContext.Setup(c => c.CloudProviders)
            .Returns(MockDbSetFactory.CreateMockDbSet(new List<CloudProvider>()).Object);

        var result = await _service.DeleteProviderAsync(999);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetDeploymentsAsync_ShouldReturnEmpty_WhenNoDeploymentsExist()
    {
        _mockDbContext.Setup(c => c.CloudDeployments)
            .Returns(MockDbSetFactory.CreateMockDbSet(new List<CloudDeployment>()).Object);

        var result = await _service.GetDeploymentsAsync();

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}
