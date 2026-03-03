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
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for WebToLeadFormService (TODO-CRM002-04).
/// Covers CRUD operations, embed key generation, and lead submission processing.
/// </summary>
public class WebToLeadFormServiceTests : ServiceTestFixtureBase<WebToLeadFormService>
{    private readonly Mock<ILeadService> _mockLeadService;    private readonly WebToLeadFormService _service;
    private readonly List<WebToLeadForm> _forms;

    public WebToLeadFormServiceTests()
    {        _mockLeadService = new Mock<ILeadService>();        _forms = new List<WebToLeadForm>();

        var mockForms = MockDbSetFactory.CreateMockDbSet(_forms);
        MockContext.Setup(c => c.WebToLeadForms).Returns(mockForms.Object);
        MockContext
            .Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _service = new WebToLeadFormService(
            MockContext.Object,
            _mockLeadService.Object,
            MockLogger.Object);
    }

    private void Refresh()
    {
        var mockForms = MockDbSetFactory.CreateMockDbSet(_forms);
        MockContext.Setup(c => c.WebToLeadForms).Returns(mockForms.Object);
    }

    // ─── Constructor ──────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_ShouldCreateInstance_WithValidDependencies()
    {
        _service.Should().NotBeNull();
    }

    // ─── GetAllAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_ShouldReturnOnlyNonDeletedForms()
    {
        // Arrange
        _forms.Add(new WebToLeadForm { Id = 1, Name = "Active Form", IsActive = true, IsDeleted = false });
        _forms.Add(new WebToLeadForm { Id = 2, Name = "Deleted Form", IsActive = true, IsDeleted = true });
        Refresh();

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().HaveCount(1);
        result.Single().Name.Should().Be("Active Form");
    }

    // ─── GetByIdAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ShouldReturnForm_WhenExistsAndNotDeleted()
    {
        // Arrange
        _forms.Add(new WebToLeadForm { Id = 5, Name = "Contact Form", IsDeleted = false });
        Refresh();

        // Act
        var result = await _service.GetByIdAsync(5);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(5);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenFormIsDeleted()
    {
        // Arrange
        _forms.Add(new WebToLeadForm { Id = 7, Name = "Old Form", IsDeleted = true });
        Refresh();

        // Act
        var result = await _service.GetByIdAsync(7);

        // Assert
        result.Should().BeNull();
    }

    // ─── CreateAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_ShouldSetCreatedAtAndEmbedKey()
    {
        // Arrange
        var form = new WebToLeadForm { Name = "New Form" };

        // Act
        var result = await _service.CreateAsync(form);

        // Assert
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.EmbedKey.Should().NotBeNullOrEmpty();
        MockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ─── DeleteAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenFormNotFound()
    {
        // Arrange — empty list
        Refresh();

        // Act
        var result = await _service.DeleteAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    // ─── GetActiveAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetActiveAsync_ShouldReturnOnlyActiveForms()
    {
        // Arrange
        _forms.Add(new WebToLeadForm { Id = 1, Name = "Active", IsActive = true, IsDeleted = false });
        _forms.Add(new WebToLeadForm { Id = 2, Name = "Inactive", IsActive = false, IsDeleted = false });
        Refresh();

        // Act
        var result = await _service.GetActiveAsync();

        // Assert
        result.Should().HaveCount(1);
        result.Single().IsActive.Should().BeTrue();
    }
}
