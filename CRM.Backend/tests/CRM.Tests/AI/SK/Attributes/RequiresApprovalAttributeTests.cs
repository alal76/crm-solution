// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Infrastructure.AI.SK.Attributes;
using FluentAssertions;
using Xunit;

namespace CRM.Tests.AI.SK.Attributes;

#nullable enable

/// <summary>
/// Unit tests for <see cref="RequiresApprovalAttribute"/>.
/// Validates default values, custom tier, and attribute usage constraints.
/// </summary>
public class RequiresApprovalAttributeTests
{
    #region Constructor — Parameterless

    [Fact]
    public void DefaultConstructor_Tier_ShouldBeLow()
    {
        // Arrange & Act
        var attr = new RequiresApprovalAttribute();

        // Assert
        attr.Tier.Should().Be("low");
    }

    [Fact]
    public void DefaultConstructor_Description_ShouldBeEmpty()
    {
        var attr = new RequiresApprovalAttribute();
        attr.Description.Should().BeEmpty();
    }

    #endregion

    #region Constructor — With Tier

    [Fact]
    public void TierConstructor_ShouldSetTier()
    {
        var attr = new RequiresApprovalAttribute("standard");
        attr.Tier.Should().Be("standard");
    }

    [Fact]
    public void TierConstructor_HighTier_ShouldSetTier()
    {
        var attr = new RequiresApprovalAttribute("high");
        attr.Tier.Should().Be("high");
    }

    #endregion

    #region Property Assignment

    [Fact]
    public void Description_ShouldBeSettable()
    {
        var attr = new RequiresApprovalAttribute("low")
        {
            Description = "Updates the account record"
        };
        attr.Description.Should().Be("Updates the account record");
    }

    #endregion

    #region AttributeUsage Verification

    [Fact]
    public void AttributeUsage_ShouldTargetMethodsOnly()
    {
        // Arrange
        var usage = (AttributeUsageAttribute)Attribute.GetCustomAttribute(
            typeof(RequiresApprovalAttribute),
            typeof(AttributeUsageAttribute))!;

        // Assert
        usage.Should().NotBeNull();
        usage.ValidOn.Should().Be(AttributeTargets.Method);
    }

    [Fact]
    public void AttributeUsage_AllowMultiple_ShouldBeFalse()
    {
        var usage = (AttributeUsageAttribute)Attribute.GetCustomAttribute(
            typeof(RequiresApprovalAttribute),
            typeof(AttributeUsageAttribute))!;

        usage.AllowMultiple.Should().BeFalse();
    }

    #endregion
}
