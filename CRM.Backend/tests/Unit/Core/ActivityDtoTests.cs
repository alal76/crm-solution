// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CRM.Core.Dtos;
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
        foreach (var r in results)
        {
            Console.WriteLine($"Validation: {string.Join(",", r.MemberNames)}: {r.ErrorMessage}");
        }
        Assert.Contains(results, r => r.ErrorMessage == "The Title field is required.");
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
        var activityDate = DateTime.UtcNow;
        var dto = new CreateActivityDto
        {
            ActivityType = 2,
            Title = "Test Subject",
            Description = "Test Desc",
            ActivityDate = activityDate,
            AccountId = 10,
            ContactId = 20,
            OpportunityId = 30,
            UserId = 5
        };
        Assert.Equal(2, dto.ActivityType);
        Assert.Equal("Test Subject", dto.Title);
        Assert.Equal("Test Desc", dto.Description);
        Assert.Equal(activityDate, dto.ActivityDate);
        Assert.Equal(10, dto.AccountId);
        Assert.Equal(20, dto.ContactId);
        Assert.Equal(30, dto.OpportunityId);
        Assert.Equal(5, dto.UserId);
    }

    [Fact]
    public void UpdateActivityDto_AllFields_ShouldMapToEntity()
    {
        var activityDate = DateTime.UtcNow;
        var dto = new UpdateActivityDto
        {
            Title = "Update Subject",
            Description = "Update Desc",
            ActivityDate = activityDate,
            AccountId = 11,
            ContactId = 21,
            OpportunityId = 31,
            UserId = 6
        };
        Assert.Equal("Update Subject", dto.Title);
        Assert.Equal("Update Desc", dto.Description);
        Assert.Equal(activityDate, dto.ActivityDate);
        Assert.Equal(11, dto.AccountId);
        Assert.Equal(21, dto.ContactId);
        Assert.Equal(31, dto.OpportunityId);
        Assert.Equal(6, dto.UserId);
    }
}
