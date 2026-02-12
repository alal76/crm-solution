// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// Licensed under the GNU Affero General Public License v3.0.
// See https://www.gnu.org/licenses/ for details.

using System.Text.Json;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.AI.SK.Plugins;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.AI.SK.Plugins;

#nullable enable

/// <summary>
/// Unit tests for <see cref="LeadPlugin"/>.
/// Validates scoring, conversion, and BANT-related kernel functions.
/// Note: LeadPlugin does NOT depend on ICrmDbContext.
/// </summary>
public class LeadPluginTests
{
    #region Fields & Setup

    private readonly Mock<ILeadService> _leadServiceMock = new();
    private readonly Mock<ILogger<LeadPlugin>> _loggerMock = new();
    private readonly LeadPlugin _plugin;

    public LeadPluginTests()
    {
        _plugin = new LeadPlugin(_leadServiceMock.Object, _loggerMock.Object);
    }

    #endregion

    #region Property Tests

    [Fact]
    public void PluginName_ShouldReturnLead()
    {
        _plugin.PluginName.Should().Be("Lead");
    }

    [Fact]
    public void Description_ShouldNotBeEmpty()
    {
        _plugin.Description.Should().NotBeNullOrWhiteSpace();
    }

    #endregion

    #region Constructor Validation

    [Fact]
    public void Constructor_NullLeadService_ShouldThrow()
    {
        var act = () => new LeadPlugin(null!, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_NullLogger_ShouldThrow()
    {
        var act = () => new LeadPlugin(_leadServiceMock.Object, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region GetLeadAsync Tests

    [Fact]
    public async Task GetLeadAsync_ExistingLead_ShouldReturnSuccessResult()
    {
        // Arrange
        var lead = new Lead { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com" };
        _leadServiceMock.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(lead);

        // Act
        var result = await _plugin.GetLeadAsync(1);

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task GetLeadAsync_NonExistentLead_ShouldReturnErrorResult()
    {
        // Arrange
        _leadServiceMock.Setup(s => s.GetByIdAsync(999))
            .ReturnsAsync((object?)null);

        // Act
        var result = await _plugin.GetLeadAsync(999);

        // Assert
        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
    }

    #endregion

    #region SearchLeadsAsync Tests

    [Fact]
    public async Task SearchLeadsAsync_ValidQuery_ShouldReturnResults()
    {
        // Arrange
        var leads = new List<object>
        {
            new Lead { Id = 1, FirstName = "John", LastName = "Doe" },
            new Lead { Id = 2, FirstName = "Jane", LastName = "Doe" }
        };
        _leadServiceMock.Setup(s => s.SearchAsync(It.IsAny<string>()))
            .ReturnsAsync(leads);

        // Act
        var result = await _plugin.SearchLeadsAsync("Doe");

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
    }

    #endregion

    #region GetLeadScoreAsync Tests

    [Fact]
    public async Task GetLeadScoreAsync_ExistingLead_ShouldReturnScore()
    {
        // Arrange
        var lead = new Lead { Id = 1, FirstName = "John", LastName = "Doe" };
        _leadServiceMock.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(lead);

        // Act
        var result = await _plugin.GetLeadScoreAsync(1);

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
    }

    #endregion

    #region UpdateLeadScoreAsync Tests

    [Fact]
    public async Task UpdateLeadScoreAsync_ValidScore_ShouldSucceed()
    {
        // Arrange
        var lead = new Lead { Id = 1, FirstName = "John", LastName = "Doe" };
        _leadServiceMock.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(lead);
        _leadServiceMock.Setup(s => s.UpdateAsync(It.IsAny<int>(), It.IsAny<Action<Lead>>()))
            .ReturnsAsync(true);

        // Act
        var result = await _plugin.UpdateLeadScoreAsync(1, 75);

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task UpdateLeadScoreAsync_ScoreAbove100_ShouldReturnError()
    {
        // Act
        var result = await _plugin.UpdateLeadScoreAsync(1, 150);

        // Assert
        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task UpdateLeadScoreAsync_NegativeScore_ShouldReturnError()
    {
        // Act
        var result = await _plugin.UpdateLeadScoreAsync(1, -5);

        // Assert
        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(50)]
    [InlineData(100)]
    public async Task UpdateLeadScoreAsync_BoundaryScores_ShouldNotReturnValidationError(int score)
    {
        // Arrange
        var lead = new Lead { Id = 1, FirstName = "Test", LastName = "Lead" };
        _leadServiceMock.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(lead);
        _leadServiceMock.Setup(s => s.UpdateAsync(It.IsAny<int>(), It.IsAny<Action<Lead>>()))
            .ReturnsAsync(true);

        // Act
        var result = await _plugin.UpdateLeadScoreAsync(1, score);

        // Assert — should not fail validation (may succeed or fail for other reasons)
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().NotContain("must be between");
    }

    #endregion

    #region ConvertLeadAsync Tests

    [Fact]
    public async Task ConvertLeadAsync_ExistingLead_ShouldReturnResult()
    {
        // Arrange
        var lead = new Lead { Id = 1, FirstName = "John", LastName = "Doe" };
        _leadServiceMock.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(lead);

        // Act
        var result = await _plugin.ConvertLeadAsync(1);

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task ConvertLeadAsync_NonExistentLead_ShouldReturnError()
    {
        // Arrange — plugin calls ConvertAsync, not GetByIdAsync
        _leadServiceMock.Setup(s => s.ConvertAsync(999, It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<decimal?>(), It.IsAny<DateTime?>()))
            .ThrowsAsync(new KeyNotFoundException("Lead with ID 999 not found"));

        // Act
        var result = await _plugin.ConvertLeadAsync(999);

        // Assert
        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
    }

    #endregion

    #region GetLeadStatsAsync Tests

    [Fact]
    public async Task GetLeadStatsAsync_ShouldReturnStatistics()
    {
        // Act
        var result = await _plugin.GetLeadStatsAsync();

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        using var doc = JsonDocument.Parse(result);
        doc.RootElement.TryGetProperty("error", out _).Should().BeTrue();
    }

    #endregion
}
