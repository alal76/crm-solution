// CRM Solution - Integration tests for ActivityService using DTOs
using System;
using System.Threading.Tasks;
using CRM.Core.DTOs;
using CRM.Core.Entities;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Integration.Services;

public class ActivityServiceDtoIntegrationTests
{
    private readonly Mock<ILogger<ActivityService>> _mockLogger = new();

    private CrmDbContext CreateInMemoryContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new CrmDbContext(options, null!);
    }

    [Fact]
    public async Task CreateActivity_FromCreateActivityDto_ShouldPersistAndMapFields()
    {
        var dbName = $"CreateActivityDto_{Guid.NewGuid()}";
        using var context = CreateInMemoryContext(dbName);
        var service = new ActivityService(context, _mockLogger.Object);

        var dto = new CreateActivityDto
        {
            ActivityType = 2,
            Subject = "Integration Subject",
            Description = "Integration Desc",
            StartDate = DateTime.UtcNow.ToString("o"),
            EndDate = DateTime.UtcNow.AddHours(1).ToString("o"),
            Status = 1,
            Priority = 2,
            AccountId = 10,
            ContactId = 20,
            OpportunityId = 30,
            OwnerUserId = 5
        };
        // Manual mapping (simulate controller logic)
        var entity = new Activity
        {
            ActivityType = (ActivityType)dto.ActivityType,
            Title = dto.Subject ?? string.Empty,
            Description = dto.Description,
            ActivityDate = DateTime.Parse(dto.StartDate ?? DateTime.UtcNow.ToString("o")),
            AccountId = dto.AccountId,
            ContactId = dto.ContactId,
            OpportunityId = dto.OpportunityId,
            UserId = dto.OwnerUserId
        };
        var result = await service.CreateAsync(entity);
        Assert.NotNull(result);
        Assert.Equal(dto.Subject, result.Title);
        Assert.Equal(dto.Description, result.Description);
        Assert.Equal(dto.AccountId, result.AccountId);
        Assert.Equal(dto.ContactId, result.ContactId);
        Assert.Equal(dto.OpportunityId, result.OpportunityId);
        Assert.Equal(dto.OwnerUserId, result.UserId);
    }

    [Fact]
    public async Task UpdateActivity_FromUpdateActivityDto_ShouldUpdateFields()
    {
        var dbName = $"UpdateActivityDto_{Guid.NewGuid()}";
        using var context = CreateInMemoryContext(dbName);
        var service = new ActivityService(context, _mockLogger.Object);
        var entity = new Activity
        {
            ActivityType = ActivityType.EmailSent,
            Title = "Old Title",
            Description = "Old Desc",
            ActivityDate = DateTime.UtcNow,
            AccountId = 1,
            ContactId = 2,
            OpportunityId = 3,
            UserId = 4
        };
        context.Activities.Add(entity);
        await context.SaveChangesAsync();
        var dto = new UpdateActivityDto
        {
            ActivityType = 3,
            Subject = "Updated Title",
            Description = "Updated Desc",
            StartDate = DateTime.UtcNow.ToString("o"),
            EndDate = DateTime.UtcNow.AddHours(2).ToString("o"),
            Status = 2,
            Priority = 1,
            AccountId = 11,
            ContactId = 21,
            OpportunityId = 31,
            OwnerUserId = 6
        };
        // Manual mapping (simulate controller logic)
        entity.ActivityType = (ActivityType)dto.ActivityType;
        entity.Title = dto.Subject ?? entity.Title;
        entity.Description = dto.Description ?? entity.Description;
        entity.AccountId = dto.AccountId;
        entity.ContactId = dto.ContactId;
        entity.OpportunityId = dto.OpportunityId;
        entity.UserId = dto.OwnerUserId;
        await context.SaveChangesAsync();
        var updated = await service.GetByIdAsync(entity.Id);
        Assert.NotNull(updated);
        Assert.Equal(dto.Subject, updated!.Title);
        Assert.Equal(dto.Description, updated.Description);
        Assert.Equal(dto.AccountId, updated.AccountId);
        Assert.Equal(dto.ContactId, updated.ContactId);
        Assert.Equal(dto.OpportunityId, updated.OpportunityId);
        Assert.Equal(dto.OwnerUserId, updated.UserId);
    }
}
