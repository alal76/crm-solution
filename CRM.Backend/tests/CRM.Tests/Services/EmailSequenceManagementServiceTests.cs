// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for EmailSequenceManagementService (TCOV-013).
/// </summary>
public class EmailSequenceManagementServiceTests : ServiceTestFixtureBase<EmailSequenceManagementService>
{
    private readonly Mock<ICrmDbContext> _mockDbContext;
    private readonly EmailSequenceManagementService _service;

    public EmailSequenceManagementServiceTests()
    {
        _mockDbContext = new Mock<ICrmDbContext>();
        SetupEmptyDbSets();
        _service = new EmailSequenceManagementService(_mockDbContext.Object, MockLogger.Object);
    }

    private void SetupEmptyDbSets()
    {
        _mockDbContext.Setup(c => c.EmailSequences)
            .Returns(MockDbSetFactory.CreateMockDbSet(new List<CRM.Core.Entities.EmailSequence>()).Object);
        _mockDbContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    [Fact]
    public void Constructor_ShouldCreateService_WithValidDependencies()
    {
        _service.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenContextIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new EmailSequenceManagementService(null!, MockLogger.Object));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenLoggerIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new EmailSequenceManagementService(_mockDbContext.Object, null!));
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmpty_WhenNoSequencesExist()
    {
        var result = await _service.GetAllAsync();
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenSequenceDoesNotExist()
    {
        var result = await _service.GetByIdAsync(999);
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenNameIsEmpty()
    {
        var dto = new CRM.Core.Dtos.CreateEmailSequenceDto { Name = "" };
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(dto));
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenSequenceNotFound()
    {
        var dto = new CRM.Core.Dtos.UpdateEmailSequenceDto { Name = "Updated" };
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateAsync(999, dto));
    }
}
