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

using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using Xunit;

namespace CRM.Tests.Integration.Services;

/// <summary>
/// Unit tests for ActivityService - timeline and activity management
/// </summary>
public class ActivityServiceTests
{
    private readonly Mock<ILogger<ActivityService>> _mockLogger;

    public ActivityServiceTests()
    {
        _mockLogger = new Mock<ILogger<ActivityService>>();
    }

    private CrmDbContext CreateInMemoryContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new CrmDbContext(options, null!);
    }

    [Fact]
    public async Task GetActivitiesAsync_ReturnsAllActivities_WhenNoFilters()
    {
        // Arrange
        var dbName = $"GetActivitiesAsync_{Guid.NewGuid()}";
        using var context = CreateInMemoryContext(dbName);

        context.Activities.AddRange(
            new Activity { ActivityType = ActivityType.EmailSent, Title = "Email 1", ActivityDate = DateTime.UtcNow },
            new Activity { ActivityType = ActivityType.CallMade, Title = "Call 1", ActivityDate = DateTime.UtcNow },
            new Activity { ActivityType = ActivityType.ChatMessage, Title = "Chat 1", ActivityDate = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var service = new ActivityService(context, _mockLogger.Object);

        // Act
        var result = await service.GetActivitiesAsync();

        // Assert
        Assert.Equal(3, result.Count());
    }

    [Fact]
    public async Task GetActivitiesAsync_FiltersByActivityType()
    {
        // Arrange
        var dbName = $"GetActivitiesAsync_Type_{Guid.NewGuid()}";
        using var context = CreateInMemoryContext(dbName);

        context.Activities.AddRange(
            new Activity { ActivityType = ActivityType.EmailSent, Title = "Email 1", ActivityDate = DateTime.UtcNow },
            new Activity { ActivityType = ActivityType.ChatMessage, Title = "Chat 1", ActivityDate = DateTime.UtcNow },
            new Activity { ActivityType = ActivityType.ChatMessage, Title = "Chat 2", ActivityDate = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var service = new ActivityService(context, _mockLogger.Object);

        // Act
        var result = await service.GetActivitiesAsync(activityType: ActivityType.ChatMessage);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, a => Assert.Equal(ActivityType.ChatMessage, a.ActivityType));
    }

    [Fact]
    public async Task GetActivitiesAsync_FiltersByCustomerId()
    {
        // Arrange
        var dbName = $"GetActivitiesAsync_Customer_{Guid.NewGuid()}";
        using var context = CreateInMemoryContext(dbName);

        context.Activities.AddRange(
            new Activity { AccountId = 1, ActivityType = ActivityType.EmailSent, Title = "Email 1", ActivityDate = DateTime.UtcNow },
            new Activity { AccountId = 1, ActivityType = ActivityType.CallMade, Title = "Call 1", ActivityDate = DateTime.UtcNow },
            new Activity { AccountId = 2, ActivityType = ActivityType.EmailSent, Title = "Email 2", ActivityDate = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var service = new ActivityService(context, _mockLogger.Object);

        // Act
        var result = await service.GetActivitiesAsync(accountId: 1);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, a => Assert.Equal(1, a.AccountId));
    }

    [Fact]
    public async Task GetActivitiesAsync_FiltersByDateRange()
    {
        // Arrange
        var dbName = $"GetActivitiesAsync_Date_{Guid.NewGuid()}";
        using var context = CreateInMemoryContext(dbName);

        var now = DateTime.UtcNow;
        context.Activities.AddRange(
            new Activity { ActivityType = ActivityType.EmailSent, Title = "Email 1", ActivityDate = now.AddDays(-5) },
            new Activity { ActivityType = ActivityType.EmailSent, Title = "Email 2", ActivityDate = now.AddDays(-2) },
            new Activity { ActivityType = ActivityType.EmailSent, Title = "Email 3", ActivityDate = now }
        );
        await context.SaveChangesAsync();

        var service = new ActivityService(context, _mockLogger.Object);

        // Act
        var result = await service.GetActivitiesAsync(fromDate: now.AddDays(-3), toDate: now);

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task CreateAsync_CreatesActivity_WithTimestamps()
    {
        // Arrange
        var dbName = $"CreateAsync_{Guid.NewGuid()}";
        using var context = CreateInMemoryContext(dbName);
        var service = new ActivityService(context, _mockLogger.Object);

        var activity = new Activity
        {
            ActivityType = ActivityType.ChatMessage,
            Title = "New Chat Message",
            Description = "Customer inquiry",
            EntityType = "Contact",
            EntityId = 123
        };

        // Act
        var result = await service.CreateAsync(activity);

        // Assert
        Assert.True(result.Id > 0);
        Assert.NotEqual(default, result.CreatedAt);
        Assert.NotEqual(default, result.UpdatedAt);
        Assert.NotEqual(default, result.ActivityDate);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsActivity_WhenExists()
    {
        // Arrange
        var dbName = $"GetByIdAsync_{Guid.NewGuid()}";
        using var context = CreateInMemoryContext(dbName);

        var activity = new Activity
        {
            ActivityType = ActivityType.MeetingScheduled,
            Title = "Sales Meeting",
            ActivityDate = DateTime.UtcNow
        };
        context.Activities.Add(activity);
        await context.SaveChangesAsync();

        var service = new ActivityService(context, _mockLogger.Object);

        // Act
        var result = await service.GetByIdAsync(activity.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Sales Meeting", result!.Title);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotExists()
    {
        // Arrange
        var dbName = $"GetByIdAsync_NotFound_{Guid.NewGuid()}";
        using var context = CreateInMemoryContext(dbName);
        var service = new ActivityService(context, _mockLogger.Object);

        // Act
        var result = await service.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsTrue_WhenActivityDeleted()
    {
        // Arrange
        var dbName = $"DeleteAsync_{Guid.NewGuid()}";
        using var context = CreateInMemoryContext(dbName);

        var activity = new Activity { ActivityType = ActivityType.EmailSent, Title = "To Delete", ActivityDate = DateTime.UtcNow };
        context.Activities.Add(activity);
        await context.SaveChangesAsync();

        var service = new ActivityService(context, _mockLogger.Object);

        // Act
        var result = await service.DeleteAsync(activity.Id);

        // Assert
        Assert.True(result);
        Assert.Null(await context.Activities.FindAsync(activity.Id));
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenActivityNotFound()
    {
        // Arrange
        var dbName = $"DeleteAsync_NotFound_{Guid.NewGuid()}";
        using var context = CreateInMemoryContext(dbName);
        var service = new ActivityService(context, _mockLogger.Object);

        // Act
        var result = await service.DeleteAsync(999);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetByEntityAsync_ReturnsActivities_ForEntity()
    {
        // Arrange
        var dbName = $"GetByEntityAsync_{Guid.NewGuid()}";
        using var context = CreateInMemoryContext(dbName);

        context.Activities.AddRange(
            new Activity { EntityType = "Contact", EntityId = 100, ActivityType = ActivityType.ChatMessage, Title = "Chat 1", ActivityDate = DateTime.UtcNow },
            new Activity { EntityType = "Contact", EntityId = 100, ActivityType = ActivityType.EmailSent, Title = "Email 1", ActivityDate = DateTime.UtcNow },
            new Activity { EntityType = "Account", EntityId = 100, ActivityType = ActivityType.CallMade, Title = "Call 1", ActivityDate = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var service = new ActivityService(context, _mockLogger.Object);

        // Act
        var result = await service.GetByEntityAsync("Contact", 100);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, a => Assert.Equal("Contact", a.EntityType));
    }

    [Fact]
    public async Task GetAccountTimelineAsync_IncludesChatMessages()
    {
        // Arrange
        var dbName = $"GetAccountTimelineAsync_{Guid.NewGuid()}";
        using var context = CreateInMemoryContext(dbName);

        context.Activities.AddRange(
            new Activity { AccountId = 1, ActivityType = ActivityType.EmailSent, Title = "Email", ActivityDate = DateTime.UtcNow },
            new Activity { AccountId = 1, ActivityType = ActivityType.ChatMessage, Title = "Chat", ActivityDate = DateTime.UtcNow,
                Details = JsonSerializer.Serialize(new { chatwootConversationId = 123, channel = "web" }) },
            new Activity { AccountId = 2, ActivityType = ActivityType.CallMade, Title = "Call", ActivityDate = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var service = new ActivityService(context, _mockLogger.Object);

        // Act
        var result = await service.GetAccountTimelineAsync(1);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Contains(result, a => a.ActivityType == ActivityType.ChatMessage);
    }

    [Fact]
    public async Task GetOpportunityTimelineAsync_ReturnsRelatedActivities()
    {
        // Arrange
        var dbName = $"GetOpportunityTimelineAsync_{Guid.NewGuid()}";
        using var context = CreateInMemoryContext(dbName);

        context.Activities.AddRange(
            new Activity { OpportunityId = 1, ActivityType = ActivityType.MeetingScheduled, Title = "Meeting", ActivityDate = DateTime.UtcNow },
            new Activity { EntityType = "Opportunity", EntityId = 1, ActivityType = ActivityType.QuoteSent, Title = "Quote", ActivityDate = DateTime.UtcNow },
            new Activity { OpportunityId = 2, ActivityType = ActivityType.EmailSent, Title = "Email", ActivityDate = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var service = new ActivityService(context, _mockLogger.Object);

        // Act
        var result = await service.GetOpportunityTimelineAsync(1);

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetRecentAsync_ReturnsLimitedOrderedResults()
    {
        // Arrange
        var dbName = $"GetRecentAsync_{Guid.NewGuid()}";
        using var context = CreateInMemoryContext(dbName);

        var now = DateTime.UtcNow;
        context.Activities.AddRange(
            new Activity { ActivityType = ActivityType.EmailSent, Title = "Old", ActivityDate = now.AddDays(-10) },
            new Activity { ActivityType = ActivityType.EmailSent, Title = "Medium", ActivityDate = now.AddDays(-5) },
            new Activity { ActivityType = ActivityType.EmailSent, Title = "New", ActivityDate = now }
        );
        await context.SaveChangesAsync();

        var service = new ActivityService(context, _mockLogger.Object);

        // Act
        var result = (await service.GetRecentAsync(2)).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("New", result[0].Title);
        Assert.Equal("Medium", result[1].Title);
    }

    [Fact]
    public async Task GetStatsAsync_CalculatesCorrectCounts()
    {
        // Arrange
        var dbName = $"GetStatsAsync_{Guid.NewGuid()}";
        using var context = CreateInMemoryContext(dbName);

        context.Activities.AddRange(
            new Activity { ActivityType = ActivityType.EmailSent, Title = "Email 1", ActivityDate = DateTime.UtcNow },
            new Activity { ActivityType = ActivityType.EmailSent, Title = "Email 2", ActivityDate = DateTime.UtcNow },
            new Activity { ActivityType = ActivityType.CallMade, Title = "Call 1", ActivityDate = DateTime.UtcNow },
            new Activity { ActivityType = ActivityType.ChatMessage, Title = "Chat 1", ActivityDate = DateTime.UtcNow },
            new Activity { ActivityType = ActivityType.OpportunityWon, Title = "Won 1", ActivityDate = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var service = new ActivityService(context, _mockLogger.Object);

        // Act
        var stats = await service.GetStatsAsync();

        // Assert
        Assert.Equal(5, stats.TotalActivities);
        Assert.Equal(2, stats.EmailsSent);
        Assert.Equal(1, stats.CallsMade);
        Assert.Equal(1, stats.OpportunitiesWon);
        Assert.NotEmpty(stats.ActivitiesByType);
    }

    [Fact]
    public async Task GetChatConversationsAsync_GroupsByConversationId()
    {
        // Arrange
        var dbName = $"GetChatConversationsAsync_{Guid.NewGuid()}";
        using var context = CreateInMemoryContext(dbName);

        var now = DateTime.UtcNow;
        context.Activities.AddRange(
            new Activity
            {
                AccountId = 1,
                ActivityType = ActivityType.ChatMessage,
                Title = "Chat 1",
                ActivityDate = now.AddMinutes(-30),
                Details = JsonSerializer.Serialize(new { chatwootConversationId = 100, channel = "web" })
            },
            new Activity
            {
                AccountId = 1,
                ActivityType = ActivityType.ChatMessage,
                Title = "Chat 2",
                ActivityDate = now.AddMinutes(-15),
                Details = JsonSerializer.Serialize(new { chatwootConversationId = 100, channel = "web" })
            },
            new Activity
            {
                AccountId = 1,
                ActivityType = ActivityType.ChatMessage,
                Title = "Chat 3",
                ActivityDate = now,
                Details = JsonSerializer.Serialize(new { chatwootConversationId = 200, channel = "whatsapp" })
            }
        );
        await context.SaveChangesAsync();

        var service = new ActivityService(context, _mockLogger.Object);

        // Act
        var conversations = (await service.GetChatConversationsAsync(1)).ToList();

        // Assert
        Assert.Equal(2, conversations.Count);

        var conv100 = conversations.FirstOrDefault(c => c.ConversationId == "100");
        Assert.NotNull(conv100);
        Assert.Equal(2, conv100!.MessageCount);
        Assert.Equal("web", conv100.Channel);

        var conv200 = conversations.FirstOrDefault(c => c.ConversationId == "200");
        Assert.NotNull(conv200);
        Assert.Equal(1, conv200!.MessageCount);
        Assert.Equal("whatsapp", conv200.Channel);
    }

    [Fact]
    public async Task GetActivitiesAsync_RespectsLimit()
    {
        // Arrange
        var dbName = $"GetActivitiesAsync_Limit_{Guid.NewGuid()}";
        using var context = CreateInMemoryContext(dbName);

        for (int i = 0; i < 100; i++)
        {
            context.Activities.Add(new Activity
            {
                ActivityType = ActivityType.EmailSent,
                Title = $"Email {i}",
                ActivityDate = DateTime.UtcNow.AddMinutes(-i)
            });
        }
        await context.SaveChangesAsync();

        var service = new ActivityService(context, _mockLogger.Object);

        // Act
        var result = await service.GetActivitiesAsync(limit: 25);

        // Assert
        Assert.Equal(25, result.Count());
    }
}
