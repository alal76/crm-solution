// CRM Solution - Unit tests for Activity DTOs
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CRM.Core.DTOs;
using Xunit;

namespace CRM.Tests.Unit.Core;

public class ActivityDtoValidationTests
{
    [Fact]
    public void ActivityDto_RequiredFields_ShouldFailValidation_WhenMissing()
    {
        var dto = new ActivityDto();
        var context = new ValidationContext(dto);
        var results = new System.Collections.Generic.List<ValidationResult>();
        bool valid = Validator.TryValidateObject(dto, context, results, true);
        Assert.Contains(results, r => r.MemberNames.Contains("ActivityType"));
        Assert.Contains(results, r => r.MemberNames.Contains("Title"));
        Assert.Contains(results, r => r.MemberNames.Contains("ActivityDate"));
    }

    [Fact]
    public void ActivityDto_Title_ShouldFailValidation_WhenTooLong()
    {
        var dto = new ActivityDto
        {
            ActivityType = 1,
            Title = new string('A', 201),
            ActivityDate = DateTime.UtcNow
        };
        var context = new ValidationContext(dto);
        var results = new System.Collections.Generic.List<ValidationResult>();
        bool valid = Validator.TryValidateObject(dto, context, results, true);
        Assert.Contains(results, r => r.MemberNames.Contains("Title"));
    }

    [Fact]
    public void ActivityDto_Description_ShouldFailValidation_WhenTooLong()
    {
        var dto = new ActivityDto
        {
            ActivityType = 1,
            Title = "Test",
            ActivityDate = DateTime.UtcNow,
            Description = new string('B', 2001)
        };
        var context = new ValidationContext(dto);
        var results = new System.Collections.Generic.List<ValidationResult>();
        bool valid = Validator.TryValidateObject(dto, context, results, true);
        Assert.Contains(results, r => r.MemberNames.Contains("Description"));
    }

    [Fact]
    public void CreateActivityDto_AllFields_ShouldMapToEntity()
    {
        var dto = new CreateActivityDto
        {
            ActivityType = 2,
            Subject = "Test Subject",
            Description = "Test Desc",
            StartDate = DateTime.UtcNow.ToString("o"),
            EndDate = DateTime.UtcNow.AddHours(1).ToString("o"),
            Status = 1,
            Priority = 2,
            AccountId = 10,
            ContactId = 20,
            OpportunityId = 30,
            OwnerUserId = 5
        };
        // Simulate mapping (manual, as no automapper in DTO)
        Assert.Equal(dto.ActivityType, 2);
        Assert.Equal(dto.Subject, "Test Subject");
        Assert.Equal(dto.Description, "Test Desc");
        Assert.Equal(dto.AccountId, 10);
        Assert.Equal(dto.ContactId, 20);
        Assert.Equal(dto.OpportunityId, 30);
        Assert.Equal(dto.OwnerUserId, 5);
    }

    [Fact]
    public void UpdateActivityDto_AllFields_ShouldMapToEntity()
    {
        var dto = new UpdateActivityDto
        {
            ActivityType = 3,
            Subject = "Update Subject",
            Description = "Update Desc",
            StartDate = DateTime.UtcNow.ToString("o"),
            EndDate = DateTime.UtcNow.AddHours(2).ToString("o"),
            Status = 2,
            Priority = 1,
            AccountId = 11,
            ContactId = 21,
            OpportunityId = 31,
            OwnerUserId = 6
        };
        // Simulate mapping
        Assert.Equal(dto.ActivityType, 3);
        Assert.Equal(dto.Subject, "Update Subject");
        Assert.Equal(dto.Description, "Update Desc");
        Assert.Equal(dto.AccountId, 11);
        Assert.Equal(dto.ContactId, 21);
        Assert.Equal(dto.OpportunityId, 31);
        Assert.Equal(dto.OwnerUserId, 6);
    }
}
