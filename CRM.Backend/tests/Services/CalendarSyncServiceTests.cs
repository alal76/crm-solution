// CRM Solution - Customer Relationship Management System
// Calendar Sync Service Unit Tests

using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for CalendarSyncService
/// Covers: Calendar sync, event management, provider integration
/// </summary>
public class CalendarSyncServiceTests
{
    private readonly Mock<IRepository<CalendarEvent>> _mockEventRepository;
    private readonly Mock<IRepository<CalendarSync>> _mockSyncRepository;
    private readonly Mock<IRepository<User>> _mockUserRepository;
    private readonly Mock<ICrmDbContext> _mockDbContext;
    private readonly Mock<ILogger<CalendarSyncService>> _mockLogger;
    private readonly CalendarSyncService _service;

    public CalendarSyncServiceTests()
    {
        _mockEventRepository = new Mock<IRepository<CalendarEvent>>();
        _mockSyncRepository = new Mock<IRepository<CalendarSync>>();
        _mockUserRepository = new Mock<IRepository<User>>();
        _mockDbContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<CalendarSyncService>>();

        _service = new CalendarSyncService(
            _mockEventRepository.Object,
            _mockSyncRepository.Object,
            _mockUserRepository.Object,
            _mockDbContext.Object,
            _mockLogger.Object);
    }

    #region Event Creation Tests

    [Fact]
    public async Task CreateEventAsync_ValidEvent_ReturnsCreatedEvent()
    {
        // Arrange
        var createDto = new CreateCalendarEventDto
        {
            Title = "Meeting with Client",
            Start = DateTime.UtcNow.AddHours(1),
            End = DateTime.UtcNow.AddHours(2),
            UserId = 1
        };

        _mockEventRepository.Setup(r => r.AddAsync(It.IsAny<CalendarEvent>()))
            .ReturnsAsync((CalendarEvent e) => { e.Id = 1; return e; });

        // Act
        var result = await _service.CreateEventAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Title.Should().Be("Meeting with Client");
    }

    [Fact]
    public async Task CreateEventAsync_EndBeforeStart_ThrowsException()
    {
        // Arrange
        var createDto = new CreateCalendarEventDto
        {
            Title = "Invalid Event",
            Start = DateTime.UtcNow.AddHours(2),
            End = DateTime.UtcNow.AddHours(1), // End before start
            UserId = 1
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.CreateEventAsync(createDto));
    }

    [Fact]
    public async Task CreateEventAsync_AllDayEvent_SetsCorrectTimes()
    {
        // Arrange
        var createDto = new CreateCalendarEventDto
        {
            Title = "All Day Event",
            Start = DateTime.UtcNow.Date,
            IsAllDay = true,
            UserId = 1
        };

        _mockEventRepository.Setup(r => r.AddAsync(It.IsAny<CalendarEvent>()))
            .ReturnsAsync((CalendarEvent e) => { e.Id = 1; return e; });

        // Act
        var result = await _service.CreateEventAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.IsAllDay.Should().BeTrue();
    }

    [Fact]
    public async Task CreateEventAsync_RecurringEvent_SetsRecurrenceRule()
    {
        // Arrange
        var createDto = new CreateCalendarEventDto
        {
            Title = "Weekly Meeting",
            Start = DateTime.UtcNow,
            End = DateTime.UtcNow.AddHours(1),
            UserId = 1,
            RecurrenceRule = "FREQ=WEEKLY;BYDAY=MO"
        };

        _mockEventRepository.Setup(r => r.AddAsync(It.IsAny<CalendarEvent>()))
            .ReturnsAsync((CalendarEvent e) => { e.Id = 1; return e; });

        // Act
        var result = await _service.CreateEventAsync(createDto);

        // Assert
        result.RecurrenceRule.Should().Be("FREQ=WEEKLY;BYDAY=MO");
    }

    #endregion

    #region Get Events Tests

    [Fact]
    public async Task GetEventsAsync_ReturnsAllEvents()
    {
        // Arrange
        var events = new List<CalendarEvent>
        {
            new CalendarEvent { Id = 1, Title = "Event 1" },
            new CalendarEvent { Id = 2, Title = "Event 2" }
        };

        _mockEventRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(events);

        // Act
        var result = await _service.GetEventsAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetEventsByUserAsync_ReturnsUserEvents()
    {
        // Arrange
        var events = new List<CalendarEvent>
        {
            new CalendarEvent { Id = 1, Title = "User Event", UserId = 1 }
        };

        _mockEventRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<CalendarEvent, bool>>>()))
            .ReturnsAsync(events);

        // Act
        var result = await _service.GetEventsByUserAsync(1);

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetEventsByDateRangeAsync_ReturnsEventsInRange()
    {
        // Arrange
        var start = DateTime.UtcNow;
        var end = DateTime.UtcNow.AddDays(7);
        var events = new List<CalendarEvent>
        {
            new CalendarEvent { Id = 1, Start = start.AddDays(1) },
            new CalendarEvent { Id = 2, Start = start.AddDays(3) }
        };

        _mockEventRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<CalendarEvent, bool>>>()))
            .ReturnsAsync(events);

        // Act
        var result = await _service.GetEventsByDateRangeAsync(1, start, end);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetEventByIdAsync_ExistingEvent_ReturnsEvent()
    {
        // Arrange
        var @event = new CalendarEvent { Id = 1, Title = "Test Event" };

        _mockEventRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(@event);

        // Act
        var result = await _service.GetEventByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("Test Event");
    }

    [Fact]
    public async Task GetEventByIdAsync_NonExistingEvent_ReturnsNull()
    {
        // Arrange
        _mockEventRepository.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((CalendarEvent?)null);

        // Act
        var result = await _service.GetEventByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Update Event Tests

    [Fact]
    public async Task UpdateEventAsync_ValidEvent_ReturnsUpdatedEvent()
    {
        // Arrange
        var existingEvent = new CalendarEvent { Id = 1, Title = "Old Title" };
        var updateDto = new UpdateCalendarEventDto { Id = 1, Title = "New Title" };

        _mockEventRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existingEvent);

        _mockEventRepository.Setup(r => r.UpdateAsync(It.IsAny<CalendarEvent>()))
            .ReturnsAsync((CalendarEvent e) => e);

        // Act
        var result = await _service.UpdateEventAsync(updateDto);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("New Title");
    }

    [Fact]
    public async Task UpdateEventAsync_NonExistingEvent_ReturnsNull()
    {
        // Arrange
        var updateDto = new UpdateCalendarEventDto { Id = 999 };

        _mockEventRepository.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((CalendarEvent?)null);

        // Act
        var result = await _service.UpdateEventAsync(updateDto);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Delete Event Tests

    [Fact]
    public async Task DeleteEventAsync_ExistingEvent_ReturnsTrue()
    {
        // Arrange
        _mockEventRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new CalendarEvent { Id = 1 });

        _mockEventRepository.Setup(r => r.DeleteAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _service.DeleteEventAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteEventAsync_NonExistingEvent_ReturnsFalse()
    {
        // Arrange
        _mockEventRepository.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((CalendarEvent?)null);

        // Act
        var result = await _service.DeleteEventAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Sync Tests

    [Fact]
    public async Task SetupSyncAsync_ValidProvider_ReturnsSyncConfig()
    {
        // Arrange
        var request = new SetupCalendarSyncRequest
        {
            UserId = 1,
            Provider = "google",
            AccessToken = "token123"
        };

        _mockSyncRepository.Setup(r => r.AddAsync(It.IsAny<CalendarSync>()))
            .ReturnsAsync((CalendarSync s) => { s.Id = 1; return s; });

        // Act
        var result = await _service.SetupSyncAsync(request);

        // Assert
        result.Should().NotBeNull();
        result!.Provider.Should().Be("google");
    }

    [Fact]
    public async Task GetSyncStatusAsync_ExistingSync_ReturnsStatus()
    {
        // Arrange
        var sync = new CalendarSync
        {
            Id = 1,
            UserId = 1,
            Provider = "google",
            LastSyncAt = DateTime.UtcNow.AddHours(-1),
            IsActive = true
        };

        _mockSyncRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<CalendarSync, bool>>>()))
            .ReturnsAsync(new List<CalendarSync> { sync });

        // Act
        var result = await _service.GetSyncStatusAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task SyncEventsAsync_ValidUserId_SyncsEvents()
    {
        // Arrange
        var sync = new CalendarSync
        {
            Id = 1,
            UserId = 1,
            Provider = "google",
            IsActive = true
        };

        _mockSyncRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<CalendarSync, bool>>>()))
            .ReturnsAsync(new List<CalendarSync> { sync });

        _mockSyncRepository.Setup(r => r.UpdateAsync(It.IsAny<CalendarSync>()))
            .ReturnsAsync((CalendarSync s) => s);

        // Act
        var result = await _service.SyncEventsAsync(1);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task DisableSyncAsync_ExistingSync_DisablesSync()
    {
        // Arrange
        var sync = new CalendarSync { Id = 1, UserId = 1, IsActive = true };

        _mockSyncRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<CalendarSync, bool>>>()))
            .ReturnsAsync(new List<CalendarSync> { sync });

        _mockSyncRepository.Setup(r => r.UpdateAsync(It.IsAny<CalendarSync>()))
            .ReturnsAsync((CalendarSync s) => { s.IsActive = false; return s; });

        // Act
        var result = await _service.DisableSyncAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Conflict Detection Tests

    [Fact]
    public async Task CheckConflictsAsync_ConflictingEvents_ReturnsConflicts()
    {
        // Arrange
        var existingEvents = new List<CalendarEvent>
        {
            new CalendarEvent
            {
                Id = 1,
                Start = DateTime.UtcNow,
                End = DateTime.UtcNow.AddHours(2)
            }
        };

        var newEvent = new CalendarEvent
        {
            Start = DateTime.UtcNow.AddHours(1),
            End = DateTime.UtcNow.AddHours(3),
            UserId = 1
        };

        _mockEventRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<CalendarEvent, bool>>>()))
            .ReturnsAsync(existingEvents);

        // Act
        var result = await _service.CheckConflictsAsync(newEvent);

        // Assert
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CheckConflictsAsync_NoConflicts_ReturnsEmpty()
    {
        // Arrange
        var existingEvents = new List<CalendarEvent>
        {
            new CalendarEvent
            {
                Id = 1,
                Start = DateTime.UtcNow.AddHours(5),
                End = DateTime.UtcNow.AddHours(6)
            }
        };

        var newEvent = new CalendarEvent
        {
            Start = DateTime.UtcNow,
            End = DateTime.UtcNow.AddHours(1),
            UserId = 1
        };

        _mockEventRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<CalendarEvent, bool>>>()))
            .ReturnsAsync(existingEvents);

        // Act
        var result = await _service.CheckConflictsAsync(newEvent);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region Attendee Tests

    [Fact]
    public async Task AddAttendeeAsync_ValidIds_AddsAttendee()
    {
        // Arrange
        var @event = new CalendarEvent
        {
            Id = 1,
            Attendees = new List<CalendarEventAttendee>()
        };

        _mockEventRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(@event);

        _mockEventRepository.Setup(r => r.UpdateAsync(It.IsAny<CalendarEvent>()))
            .ReturnsAsync((CalendarEvent e) => e);

        // Act
        var result = await _service.AddAttendeeAsync(1, "attendee@test.com");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task RemoveAttendeeAsync_ExistingAttendee_RemovesAttendee()
    {
        // Arrange
        var @event = new CalendarEvent
        {
            Id = 1,
            Attendees = new List<CalendarEventAttendee>
            {
                new CalendarEventAttendee { Email = "attendee@test.com" }
            }
        };

        _mockEventRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(@event);

        _mockEventRepository.Setup(r => r.UpdateAsync(It.IsAny<CalendarEvent>()))
            .ReturnsAsync((CalendarEvent e) => e);

        // Act
        var result = await _service.RemoveAttendeeAsync(1, "attendee@test.com");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetAttendeeEventsAsync_ReturnsAttendeeEvents()
    {
        // Arrange
        var events = new List<CalendarEvent>
        {
            new CalendarEvent
            {
                Id = 1,
                Attendees = new List<CalendarEventAttendee>
                {
                    new CalendarEventAttendee { Email = "user@test.com" }
                }
            }
        };

        _mockEventRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(events);

        // Act
        var result = await _service.GetAttendeeEventsAsync("user@test.com");

        // Assert
        result.Should().NotBeEmpty();
    }

    #endregion

    #region Reminder Tests

    [Fact]
    public async Task SetReminderAsync_ValidEvent_SetsReminder()
    {
        // Arrange
        var @event = new CalendarEvent { Id = 1 };

        _mockEventRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(@event);

        _mockEventRepository.Setup(r => r.UpdateAsync(It.IsAny<CalendarEvent>()))
            .ReturnsAsync((CalendarEvent e) => e);

        // Act
        var result = await _service.SetReminderAsync(1, TimeSpan.FromMinutes(15));

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetUpcomingRemindersAsync_ReturnsReminders()
    {
        // Arrange
        var events = new List<CalendarEvent>
        {
            new CalendarEvent
            {
                Id = 1,
                Start = DateTime.UtcNow.AddMinutes(10),
                ReminderMinutes = 15,
                UserId = 1
            }
        };

        _mockEventRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<CalendarEvent, bool>>>()))
            .ReturnsAsync(events);

        // Act
        var result = await _service.GetUpcomingRemindersAsync(1);

        // Assert
        result.Should().NotBeEmpty();
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task GetCalendarStatisticsAsync_ReturnsStats()
    {
        // Arrange
        var events = new List<CalendarEvent>
        {
            new CalendarEvent { Id = 1, UserId = 1, Start = DateTime.UtcNow },
            new CalendarEvent { Id = 2, UserId = 1, Start = DateTime.UtcNow.AddDays(-5) }
        };

        _mockEventRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<CalendarEvent, bool>>>()))
            .ReturnsAsync(events);

        // Act
        var result = await _service.GetCalendarStatisticsAsync(1);

        // Assert
        result.TotalEvents.Should().Be(2);
    }

    #endregion
}

// Supporting classes for tests
public class CreateCalendarEventDto
{
    public string Title { get; set; } = string.Empty;
    public DateTime Start { get; set; }
    public DateTime? End { get; set; }
    public int UserId { get; set; }
    public bool IsAllDay { get; set; }
    public string? RecurrenceRule { get; set; }
}

public class UpdateCalendarEventDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public DateTime? Start { get; set; }
    public DateTime? End { get; set; }
}

public class SetupCalendarSyncRequest
{
    public int UserId { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
}
