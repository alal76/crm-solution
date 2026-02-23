// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Dtos.ITSM;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Core.Interfaces.ITSM;
using CRM.Infrastructure.Services.ITSM;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for SLAPolicyAdminService.
/// Covers CRUD operations, policy assignment, applicable policies, and error handling.
/// </summary>
public class SLAPolicyAdminServiceTests
{
    private readonly Mock<ICrmDbContext> _mockDbContext;
    private readonly Mock<ILogger<SLAPolicyAdminService>> _mockLogger;
    private readonly SLAPolicyAdminService _service;

    private readonly List<SLAPolicy> _policies;
    private readonly List<ServiceRequest> _serviceRequests;

    public SLAPolicyAdminServiceTests()
    {
        _mockDbContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<SLAPolicyAdminService>>();

        _policies = new List<SLAPolicy>();
        _serviceRequests = new List<ServiceRequest>();

        SetupMockDbSets();

        _service = new SLAPolicyAdminService(_mockDbContext.Object, _mockLogger.Object);
    }

    private void SetupMockDbSets()
    {
        var mockPolicies = MockDbSetFactory.CreateMockDbSet(_policies);
        var mockServiceRequests = MockDbSetFactory.CreateMockDbSet(_serviceRequests);

        _mockDbContext.Setup(c => c.SLAPolicies).Returns(mockPolicies.Object);
        _mockDbContext.Setup(c => c.ServiceRequests).Returns(mockServiceRequests.Object);
        _mockDbContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    private void RefreshMockDbSets()
    {
        var mockPolicies = MockDbSetFactory.CreateMockDbSet(_policies);
        var mockServiceRequests = MockDbSetFactory.CreateMockDbSet(_serviceRequests);

        _mockDbContext.Setup(c => c.SLAPolicies).Returns(mockPolicies.Object);
        _mockDbContext.Setup(c => c.ServiceRequests).Returns(mockServiceRequests.Object);
    }

    // ========================================================================
    // Constructor Tests
    // ========================================================================

    [Fact]
    public void Constructor_ShouldCreateInstance_WithValidDependencies()
    {
        _service.Should().NotBeNull();
    }

    // ========================================================================
    // GetByIdAsync Tests
    // ========================================================================

    [Fact]
    public async Task GetByIdAsync_ShouldReturnPolicy_WhenFound()
    {
        // Arrange
        _policies.Add(CreateTestPolicy(1, "Standard SLA"));
        RefreshMockDbSets();

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Standard SLA");
        result.ResponseTimeHours.Should().Be(4);   // 240 min / 60
        result.ResolutionTimeHours.Should().Be(24); // 1440 min / 60
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        // Act
        var result = await _service.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenPolicyIsDeleted()
    {
        // Arrange
        var policy = CreateTestPolicy(1, "Deleted SLA");
        policy.IsDeleted = true;
        _policies.Add(policy);
        RefreshMockDbSets();

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        result.Should().BeNull();
    }

    // ========================================================================
    // GetAllAsync Tests
    // ========================================================================

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllNonDeletedPolicies()
    {
        // Arrange
        _policies.Add(CreateTestPolicy(1, "Policy A"));
        _policies.Add(CreateTestPolicy(2, "Policy B"));
        var deletedPolicy = CreateTestPolicy(3, "Deleted Policy");
        deletedPolicy.IsDeleted = true;
        _policies.Add(deletedPolicy);
        RefreshMockDbSets();

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoPolicies()
    {
        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    // ========================================================================
    // CreateAsync Tests
    // ========================================================================

    [Fact]
    public async Task CreateAsync_ShouldCreatePolicy_WithValidData()
    {
        // Arrange
        var dto = new CreateSLAPolicyDto
        {
            Name = "Premium SLA",
            Description = "Premium support SLA",
            ResponseTimeHours = 2,
            ResolutionTimeHours = 8,
            BusinessHoursOnly = true,
            IsActive = true
        };

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Premium SLA");
        result.ResponseTimeHours.Should().Be(2);
        result.ResolutionTimeHours.Should().Be(8);
        result.BusinessHoursOnly.Should().BeTrue();
        result.IsActive.Should().BeTrue();
        _mockDbContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenNameIsEmpty()
    {
        // Arrange
        var dto = new CreateSLAPolicyDto
        {
            Name = "",
            ResponseTimeHours = 4,
            ResolutionTimeHours = 24
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateAsync(dto));
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenNameIsWhitespace()
    {
        // Arrange
        var dto = new CreateSLAPolicyDto
        {
            Name = "   ",
            ResponseTimeHours = 4,
            ResolutionTimeHours = 24
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateAsync(dto));
    }

    // ========================================================================
    // UpdateAsync Tests
    // ========================================================================

    [Fact]
    public async Task UpdateAsync_ShouldUpdatePolicy_WhenFound()
    {
        // Arrange
        _policies.Add(CreateTestPolicy(1, "Original SLA"));
        RefreshMockDbSets();

        var dto = new UpdateSLAPolicyDto
        {
            Name = "Updated SLA",
            ResponseTimeHours = 1,
            ResolutionTimeHours = 4
        };

        // Act
        var result = await _service.UpdateAsync(1, dto);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Updated SLA");
        result.ResponseTimeHours.Should().Be(1);
        result.ResolutionTimeHours.Should().Be(4);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenPolicyNotFound()
    {
        // Arrange
        var dto = new UpdateSLAPolicyDto { Name = "X" };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.UpdateAsync(999, dto));
    }

    [Fact]
    public async Task UpdateAsync_ShouldOnlyUpdateProvidedFields()
    {
        // Arrange
        _policies.Add(CreateTestPolicy(1, "Original SLA"));
        RefreshMockDbSets();

        var dto = new UpdateSLAPolicyDto
        {
            Name = "Updated Name Only"
            // ResponseTimeHours and ResolutionTimeHours are null, should not change
        };

        // Act
        var result = await _service.UpdateAsync(1, dto);

        // Assert
        result.Name.Should().Be("Updated Name Only");
        result.ResponseTimeHours.Should().Be(4);   // unchanged (240/60)
        result.ResolutionTimeHours.Should().Be(24); // unchanged (1440/60)
    }

    // ========================================================================
    // DeleteAsync Tests
    // ========================================================================

    [Fact]
    public async Task DeleteAsync_ShouldSoftDelete_WhenPolicyExists()
    {
        // Arrange
        _policies.Add(CreateTestPolicy(1, "Delete Me"));
        RefreshMockDbSets();

        // Act
        await _service.DeleteAsync(1);

        // Assert
        _policies[0].IsDeleted.Should().BeTrue();
        _policies[0].UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        _mockDbContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrow_WhenPolicyNotFound()
    {
        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.DeleteAsync(999));
    }

    // ========================================================================
    // AssignPolicyAsync Tests
    // ========================================================================

    [Fact]
    public async Task AssignPolicyAsync_ShouldReturnInstance_WhenBothExist()
    {
        // Arrange
        _policies.Add(CreateTestPolicy(1, "Assign SLA"));
        _serviceRequests.Add(new ServiceRequest { Id = 10, Title = "Test Ticket", CreatedAt = DateTime.UtcNow });
        RefreshMockDbSets();

        // Act
        var result = await _service.AssignPolicyAsync(1, 10);

        // Assert
        result.Should().NotBeNull();
        result.PolicyId.Should().Be(1);
        result.ServiceRequestId.Should().Be(10);
    }

    [Fact]
    public async Task AssignPolicyAsync_ShouldThrow_WhenPolicyNotFound()
    {
        // Arrange
        _serviceRequests.Add(new ServiceRequest { Id = 10, Title = "Test Ticket", CreatedAt = DateTime.UtcNow });
        RefreshMockDbSets();

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.AssignPolicyAsync(999, 10));
    }

    [Fact]
    public async Task AssignPolicyAsync_ShouldThrow_WhenServiceRequestNotFound()
    {
        // Arrange
        _policies.Add(CreateTestPolicy(1, "Assign SLA"));
        RefreshMockDbSets();

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.AssignPolicyAsync(1, 999));
    }

    // ========================================================================
    // GetApplicablePoliciesAsync Tests
    // ========================================================================

    [Fact]
    public async Task GetApplicablePoliciesAsync_ShouldReturnActiveNonDeletedPolicies()
    {
        // Arrange
        var active = CreateTestPolicy(1, "Active SLA");
        active.IsActive = true;
        _policies.Add(active);

        var inactive = CreateTestPolicy(2, "Inactive SLA");
        inactive.IsActive = false;
        _policies.Add(inactive);

        RefreshMockDbSets();

        // Act
        var result = await _service.GetApplicablePoliciesAsync(null, null);

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Active SLA");
    }

    // ========================================================================
    // Helper Methods
    // ========================================================================

    private static SLAPolicy CreateTestPolicy(int id, string name)
    {
        return new SLAPolicy
        {
            Id = id,
            Name = name,
            Description = $"Test policy: {name}",
            Priority = ServicePriority.Medium,
            InitialResponseTimeMinutes = 240,  // 4 hours
            ResolutionTimeMinutes = 1440,       // 24 hours
            WorkingHoursOnly = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}
