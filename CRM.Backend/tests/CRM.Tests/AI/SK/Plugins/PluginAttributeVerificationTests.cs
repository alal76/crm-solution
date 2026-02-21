// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.ComponentModel;
using System.Reflection;
using CRM.Infrastructure.AI.SK.Attributes;
using CRM.Infrastructure.AI.SK.Plugins;
using FluentAssertions;
using Microsoft.SemanticKernel;
using Xunit;

namespace CRM.Tests.AI.SK.Plugins;

#nullable enable

/// <summary>
/// Reflection-based tests to verify that plugin methods are correctly decorated
/// with [KernelFunction], [Description], and [RequiresApproval] attributes.
/// </summary>
public class PluginAttributeVerificationTests
{
    #region KernelFunction Attribute Tests

    [Theory]
    [InlineData(typeof(AccountPlugin), "GetAccountAsync")]
    [InlineData(typeof(AccountPlugin), "SearchAccountsAsync")]
    [InlineData(typeof(AccountPlugin), "GetAccountHealthAsync")]
    [InlineData(typeof(AccountPlugin), "GetRelatedContactsAsync")]
    [InlineData(typeof(AccountPlugin), "UpdateAccountAsync")]
    public void AccountPlugin_Methods_ShouldHaveKernelFunction(Type pluginType, string methodName)
    {
        AssertHasAttribute<KernelFunctionAttribute>(pluginType, methodName);
    }

    [Theory]
    [InlineData(typeof(LeadPlugin), "GetLeadAsync")]
    [InlineData(typeof(LeadPlugin), "SearchLeadsAsync")]
    [InlineData(typeof(LeadPlugin), "GetLeadScoreAsync")]
    [InlineData(typeof(LeadPlugin), "GetLeadStatsAsync")]
    [InlineData(typeof(LeadPlugin), "UpdateLeadScoreAsync")]
    [InlineData(typeof(LeadPlugin), "ConvertLeadAsync")]
    public void LeadPlugin_Methods_ShouldHaveKernelFunction(Type pluginType, string methodName)
    {
        AssertHasAttribute<KernelFunctionAttribute>(pluginType, methodName);
    }

    [Theory]
    [InlineData(typeof(ServiceRequestPlugin), "GetTicketAsync")]
    [InlineData(typeof(ServiceRequestPlugin), "SearchTicketsAsync")]
    [InlineData(typeof(ServiceRequestPlugin), "GetSLAStatusAsync")]
    [InlineData(typeof(ServiceRequestPlugin), "AssignTicketAsync")]
    [InlineData(typeof(ServiceRequestPlugin), "CloseTicketAsync")]
    [InlineData(typeof(ServiceRequestPlugin), "ResolveTicketAsync")]
    public void ServiceRequestPlugin_Methods_ShouldHaveKernelFunction(Type pluginType, string methodName)
    {
        AssertHasAttribute<KernelFunctionAttribute>(pluginType, methodName);
    }

    [Theory]
    [InlineData(typeof(SearchPlugin), "GlobalSearchAsync")]
    [InlineData(typeof(SearchPlugin), "SearchByTypeAsync")]
    public void SearchPlugin_Methods_ShouldHaveKernelFunction(Type pluginType, string methodName)
    {
        AssertHasAttribute<KernelFunctionAttribute>(pluginType, methodName);
    }

    #endregion

    #region Description Attribute Tests

    [Theory]
    [InlineData(typeof(AccountPlugin), "GetAccountAsync")]
    [InlineData(typeof(LeadPlugin), "GetLeadAsync")]
    [InlineData(typeof(ServiceRequestPlugin), "GetTicketAsync")]
    [InlineData(typeof(SearchPlugin), "GlobalSearchAsync")]
    public void ReadMethods_ShouldHaveDescription(Type pluginType, string methodName)
    {
        AssertHasAttribute<DescriptionAttribute>(pluginType, methodName);
    }

    [Fact]
    public void AllKernelFunctionMethods_ShouldHaveDescription()
    {
        // Verify ALL methods with [KernelFunction] also have [Description]
        var pluginTypes = new[]
        {
            typeof(AccountPlugin),
            typeof(LeadPlugin),
            typeof(ServiceRequestPlugin),
            typeof(SearchPlugin)
        };

        foreach (var type in pluginTypes)
        {
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.GetCustomAttribute<KernelFunctionAttribute>() != null);

            foreach (var method in methods)
            {
                var desc = method.GetCustomAttribute<DescriptionAttribute>();
                desc.Should().NotBeNull(
                    $"{type.Name}.{method.Name} has [KernelFunction] but is missing [Description]");
            }
        }
    }

    #endregion

    #region RequiresApproval Attribute Tests

    [Fact]
    public void AccountPlugin_UpdateAccount_ShouldRequireApproval()
    {
        var attr = GetAttribute<RequiresApprovalAttribute>(typeof(AccountPlugin), "UpdateAccountAsync");
        attr.Should().NotBeNull();
        attr!.Tier.Should().Be("low");
    }

    [Fact]
    public void LeadPlugin_UpdateLeadScore_ShouldRequireApproval()
    {
        var attr = GetAttribute<RequiresApprovalAttribute>(typeof(LeadPlugin), "UpdateLeadScoreAsync");
        attr.Should().NotBeNull();
        attr!.Tier.Should().Be("low");
    }

    [Fact]
    public void LeadPlugin_ConvertLead_ShouldRequireStandardApproval()
    {
        var attr = GetAttribute<RequiresApprovalAttribute>(typeof(LeadPlugin), "ConvertLeadAsync");
        attr.Should().NotBeNull();
        attr!.Tier.Should().Be("standard");
    }

    [Fact]
    public void ServiceRequestPlugin_CloseTicket_ShouldRequireStandardApproval()
    {
        var attr = GetAttribute<RequiresApprovalAttribute>(typeof(ServiceRequestPlugin), "CloseTicketAsync");
        attr.Should().NotBeNull();
        attr!.Tier.Should().Be("standard");
    }

    [Fact]
    public void ServiceRequestPlugin_AssignTicket_ShouldRequireLowApproval()
    {
        var attr = GetAttribute<RequiresApprovalAttribute>(typeof(ServiceRequestPlugin), "AssignTicketAsync");
        attr.Should().NotBeNull();
        attr!.Tier.Should().Be("low");
    }

    [Fact]
    public void ReadOnlyMethods_ShouldNotRequireApproval()
    {
        // Read-only methods (Get*, Search*) should NOT have [RequiresApproval]
        var readMethodNames = new[] { "GetAccountAsync", "SearchAccountsAsync", "GetAccountHealthAsync", "GetRelatedContactsAsync" };

        foreach (var methodName in readMethodNames)
        {
            var attr = GetAttribute<RequiresApprovalAttribute>(typeof(AccountPlugin), methodName);
            attr.Should().BeNull(
                $"AccountPlugin.{methodName} is read-only and should not require approval");
        }
    }

    #endregion

    #region Helpers

    private static void AssertHasAttribute<TAttribute>(Type type, string methodName)
        where TAttribute : Attribute
    {
        var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
        method.Should().NotBeNull($"Method {type.Name}.{methodName} should exist");

        var attr = method!.GetCustomAttribute<TAttribute>();
        attr.Should().NotBeNull(
            $"{type.Name}.{methodName} should have [{typeof(TAttribute).Name}]");
    }

    private static TAttribute? GetAttribute<TAttribute>(Type type, string methodName)
        where TAttribute : Attribute
    {
        var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
        method.Should().NotBeNull($"Method {type.Name}.{methodName} should exist");
        return method!.GetCustomAttribute<TAttribute>();
    }

    #endregion
}
