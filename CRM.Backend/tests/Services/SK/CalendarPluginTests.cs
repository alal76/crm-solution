// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Text.Json;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.AI.SK.Plugins;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services.SK;

/// <summary>
/// Unit tests for the CalendarPlugin Semantic Kernel plugin.
/// </summary>
public class CalendarPluginTests
{
    private readonly Mock<IActivityService> _activityServiceMock;
    private readonly Mock<ILogger<CalendarPlugin>> _loggerMock;
    private readonly CalendarPlugin _sut;

    public CalendarPluginTests()
    {
        _activityServiceMock = new Mock<IActivityService>();
        _loggerMock = new Mock<ILogger<CalendarPlugin>>();
        _sut = new CalendarPlugin(_activityServiceMock.Object, _loggerMock.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenActivityServiceIsNull()
    {
        var act = () => new CalendarPlugin(null!, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("activityService");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenLoggerIsNull()
    {
        var act = () => new CalendarPlugin(_activityServiceMock.Object, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Plugin Metadata Tests

    [Fact]
    public void PluginName_ShouldReturn_Calendar()
    {
        _sut.PluginName.Should().Be("Calendar");
    }

    [Fact]
    public void Description_ShouldNotBeNullOrEmpty()
    {
        _sut.Description.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region GetActivitiesAsync Tests

    [Fact]
    public async Task GetActivitiesAsync_ShouldReturnSuccessJson_WhenActivitiesExist()
    {
        var activities = new List<ActivityDto>
        {
            new ActivityDto { Id = 1, ActivityType = (int)ActivityType.CallMade, Title = "Sales call", EntityType = "Account", EntityId = 1 }
        };
        _activityServiceMock
            .Setup(s => s.GetActivitiesAsync(
                It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>(),
                It.IsAny<int?>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<int>()))
            .ReturnsAsync(activities);

        var result = await _sut.GetActivitiesAsync(accountId: 1, daysBack: 30, limit: 25);

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task GetActivitiesAsync_ShouldReturnEmptySuccessJson_WhenNoActivities()
    {
        _activityServiceMock
            .Setup(s => s.GetActivitiesAsync(
                It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>(),
                It.IsAny<int?>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<int>()))
            .ReturnsAsync(Enumerable.Empty<ActivityDto>());

        var result = await _sut.GetActivitiesAsync();

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task GetActivitiesAsync_ShouldReturnErrorJson_WhenServiceThrows()
    {
        _activityServiceMock
            .Setup(s => s.GetActivitiesAsync(
                It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>(),
                It.IsAny<int?>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException("DB error"));

        var result = await _sut.GetActivitiesAsync();

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
    }

    #endregion

    #region GetUpcomingAsync Tests

    [Fact]
    public async Task GetUpcomingAsync_ShouldReturnSuccessJson()
    {
        var activities = new List<ActivityDto>
        {
            new ActivityDto { Id = 1, ActivityType = (int)ActivityType.MeetingScheduled, Title = "Product demo" }
        };
        _activityServiceMock
            .Setup(s => s.GetRecentAsync(It.IsAny<int>()))
            .ReturnsAsync(activities);

        var result = await _sut.GetUpcomingAsync(20);

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
        _activityServiceMock.Verify(s => s.GetRecentAsync(20), Times.Once);
    }

    [Fact]
    public async Task GetUpcomingAsync_ShouldReturnErrorJson_WhenServiceThrows()
    {
        _activityServiceMock
            .Setup(s => s.GetRecentAsync(It.IsAny<int>()))
            .ThrowsAsync(new Exception("Connection timeout"));

        var result = await _sut.GetUpcomingAsync();

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
    }

    #endregion

    #region GetActivityStatsAsync Tests

    [Fact]
    public async Task GetActivityStatsAsync_ShouldReturnSuccessJson()
    {
        var stats = new ActivityStats
        {
            TotalActivities = 50,
            EmailsSent = 20,
            CallsMade = 15,
            MeetingsCompleted = 10
        };
        _activityServiceMock
            .Setup(s => s.GetStatsAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(stats);

        var result = await _sut.GetActivityStatsAsync(30);

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task GetActivityStatsAsync_ShouldReturnErrorJson_WhenServiceThrows()
    {
        _activityServiceMock
            .Setup(s => s.GetStatsAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ThrowsAsync(new Exception("Analytics unavailable"));

        var result = await _sut.GetActivityStatsAsync();

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
    }

    #endregion

    #region LogActivityAsync Tests

    [Fact]
    public async Task LogActivityAsync_ShouldReturnSuccessJson_WithValidActivityType()
    {
        var created = new ActivityDto { Id = 42, ActivityType = (int)ActivityType.CallMade, Title = "Follow up call", EntityType = "Account", EntityId = 1 };
        _activityServiceMock
            .Setup(s => s.CreateAsync(It.IsAny<CreateActivityDto>()))
            .ReturnsAsync(created);

        var result = await _sut.LogActivityAsync("Follow up call", "Called about renewal", "CallMade", "Account", 1);

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
        doc.RootElement.GetProperty("data").GetProperty("success").GetBoolean().Should().BeTrue();
        doc.RootElement.GetProperty("data").GetProperty("activityId").GetInt32().Should().Be(42);
    }

    [Fact]
    public async Task LogActivityAsync_ShouldReturnSuccessWithSuccessFalse_WhenActivityTypeInvalid()
    {
        var result = await _sut.LogActivityAsync("Test", "Details", "InvalidType", "Account", 1);

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
        doc.RootElement.GetProperty("data").GetProperty("success").GetBoolean().Should().BeFalse();
        _activityServiceMock.Verify(s => s.CreateAsync(It.IsAny<CreateActivityDto>()), Times.Never);
    }

    [Theory]
    [InlineData("EmailSent")]
    [InlineData("CallMade")]
    [InlineData("MeetingScheduled")]
    [InlineData("TaskCompleted")]
    [InlineData("NoteAdded")]
    public async Task LogActivityAsync_ShouldAccept_AllValidActivityTypes(string activityType)
    {
        var created = new ActivityDto { Id = 1, ActivityType = 1, Title = "Test" };
        _activityServiceMock
            .Setup(s => s.CreateAsync(It.IsAny<CreateActivityDto>()))
            .ReturnsAsync(created);

        var result = await _sut.LogActivityAsync("Test", "Test details", activityType, "Account", 1);

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
        doc.RootElement.GetProperty("data").GetProperty("success").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task LogActivityAsync_ShouldReturnErrorJson_WhenServiceThrows()
    {
        _activityServiceMock
            .Setup(s => s.CreateAsync(It.IsAny<CreateActivityDto>()))
            .ThrowsAsync(new Exception("DB write error"));

        var result = await _sut.LogActivityAsync("Test", "Details", "CallMade", "Account", 1);

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
    }

    #endregion
}
