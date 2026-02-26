// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Enums;
using FluentAssertions;
using Xunit;

namespace CRM.Tests.Unit.Core;

public class ScriptLanguageEnumTests
{
    [Fact]
    public void ScriptLanguage_ShouldMatchExpectedNumericValues()
    {
        ((int)ScriptLanguage.JavaScript).Should().Be(0);
        ((int)ScriptLanguage.Python).Should().Be(1);
        ((int)ScriptLanguage.CSharp).Should().Be(2);
    }

    [Fact]
    public void ScriptLanguage_ShouldContainAllDefinedValues()
    {
        var values = Enum.GetValues<ScriptLanguage>();
        values.Should().BeEquivalentTo(new[]
        {
            ScriptLanguage.JavaScript,
            ScriptLanguage.Python,
            ScriptLanguage.CSharp
        }, options => options.WithStrictOrdering());
    }
}
