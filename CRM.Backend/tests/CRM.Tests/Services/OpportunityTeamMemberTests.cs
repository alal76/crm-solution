// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

// TODO-CRM003-08: Unit tests for opportunity team member entity and split commission DTOs.

using System.ComponentModel.DataAnnotations;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using FluentAssertions;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Tests for the opportunity team / commission split feature (TODO-CRM003-08).
/// Covers entity defaults, enum values, and DTO validation rules.
/// </summary>
public class OpportunityTeamMemberTests
{
    [Fact]
    public void OpportunityTeamMember_DefaultRole_IsOther()
    {
        var member = new OpportunityTeamMember();

        member.Role.Should().Be(OpportunityTeamRole.Other,
            "default role should be 'Other' when not explicitly set");
    }

    [Fact]
    public void OpportunityTeamMember_DefaultSplitPercentage_IsZero()
    {
        var member = new OpportunityTeamMember();

        member.SplitPercentage.Should().Be(0m);
    }

    [Fact]
    public void OpportunityTeamRole_Enum_HasExpectedValues()
    {
        // Core roles must be stable — matches spec and API contract
        ((int)OpportunityTeamRole.AccountExecutive).Should().Be(0);
        ((int)OpportunityTeamRole.SDR).Should().Be(1);
        ((int)OpportunityTeamRole.SalesEngineer).Should().Be(2);
        ((int)OpportunityTeamRole.AccountManager).Should().Be(3);
        ((int)OpportunityTeamRole.SalesManager).Should().Be(4);
        ((int)OpportunityTeamRole.Partner).Should().Be(5);
        ((int)OpportunityTeamRole.ExecutiveSponsor).Should().Be(6);
        ((int)OpportunityTeamRole.CustomerSuccess).Should().Be(7);
        ((int)OpportunityTeamRole.Other).Should().Be(99);
    }

    [Fact]
    public void CreateTeamMemberDto_Validates_SplitPercentage_TooHigh()
    {
        var dto = new CreateTeamMemberDto { UserId = 1, SplitPercentage = 101m };

        var results = new List<ValidationResult>();
        var context = new ValidationContext(dto);
        var isValid = Validator.TryValidateObject(dto, context, results, validateAllProperties: true);

        isValid.Should().BeFalse("SplitPercentage of 101 exceeds the allowed range 0-100");
        results.Should().ContainSingle(r =>
            r.MemberNames.Contains(nameof(CreateTeamMemberDto.SplitPercentage)));
    }

    [Fact]
    public void CreateTeamMemberDto_Validates_SplitPercentage_ValidRange()
    {
        var dto = new CreateTeamMemberDto { UserId = 1, SplitPercentage = 50m };

        var results = new List<ValidationResult>();
        var context = new ValidationContext(dto);
        var isValid = Validator.TryValidateObject(dto, context, results, validateAllProperties: true);

        isValid.Should().BeTrue("SplitPercentage of 50 is within the valid 0-100 range");
    }
}
