using System;
using System.Collections.Generic;
using CRM.Core.Validation;
using FluentAssertions;
using Xunit;

namespace CRM.Tests.Validation;

public class UiConfigurationValidatorTests
{
    [Fact]
    public void ValidateModuleName_ShouldThrow_WhenEmpty()
    {
        var action = () => UiConfigurationValidator.ValidateModuleName(" ");

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ValidateModuleName_ShouldAllow_CustomModule()
    {
        var action = () => UiConfigurationValidator.ValidateModuleName("CustomModule");

        action.Should().NotThrow();
    }

    [Fact]
    public void EnsureUniqueKeys_ShouldThrow_WhenDuplicatesExist()
    {
        var keys = new List<string> { "alpha", "beta", "Alpha" };

        var action = () => UiConfigurationValidator.EnsureUniqueKeys(keys, "Test key");

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void EnsureNonNegativeOrders_ShouldThrow_WhenNegative()
    {
        var items = new List<(string Id, int Order)>
        {
            ("one", 0),
            ("two", -1)
        };

        var action = () => UiConfigurationValidator.EnsureNonNegativeOrders(items, "Test order");

        action.Should().Throw<ArgumentException>();
    }
}
