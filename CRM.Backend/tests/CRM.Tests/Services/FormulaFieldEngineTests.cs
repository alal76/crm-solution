// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Collections.Generic;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for FormulaFieldEngine.
/// Verifies arithmetic evaluation, field token substitution, and formula validation.
/// </summary>
public class FormulaFieldEngineTests
{
    private readonly FormulaFieldEngine _engine;

    public FormulaFieldEngineTests()
    {
        var mockLogger = new Mock<ILogger<FormulaFieldEngine>>();
        _engine = new FormulaFieldEngine(mockLogger.Object);
    }

    // ─────────────────────────────────────────────────────────────────────
    // Test 1 – arithmetic: multiply two numeric fields
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public void Evaluate_ShouldReturnArithmeticProduct_WhenMultiplyingTwoFields()
    {
        // Arrange
        var formula = "{Amount} * {Quantity}";
        var fields = new Dictionary<string, object?>
        {
            ["Amount"] = 100.0,
            ["Quantity"] = 5.0
        };

        // Act
        var result = _engine.Evaluate(formula, fields);

        // Assert
        result.Success.Should().BeTrue();
        result.NumericValue.Should().BeApproximately(500.0, 0.001);
    }

    // ─────────────────────────────────────────────────────────────────────
    // Test 2 – literal arithmetic without field references
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public void Evaluate_ShouldReturnSum_WhenAddingTwoLiterals()
    {
        // Arrange
        var formula = "10 + 5";

        // Act
        var result = _engine.Evaluate(formula, new Dictionary<string, object?>());

        // Assert
        result.Success.Should().BeTrue();
        result.NumericValue.Should().BeApproximately(15.0, 0.001);
    }

    // ─────────────────────────────────────────────────────────────────────
    // Test 3 – validation detects an empty formula
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public void Validate_ShouldReturnInvalid_WhenFormulaIsEmpty()
    {
        // Act
        var result = _engine.Validate(string.Empty);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("empty"));
    }
}
