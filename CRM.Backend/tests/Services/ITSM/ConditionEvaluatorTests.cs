// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Infrastructure.Services.ITSM;
using FluentAssertions;
using Xunit;
using CoreServiceRequest = CRM.Core.Entities.ServiceRequest;

namespace CRM.Tests.Services.ITSM;

/// <summary>
/// Tests for ConditionEvaluator — covers AND/OR grouping, nested conditions,
/// simple field comparisons, and edge cases.
/// TODO-SD005-012: Complex Condition Expression Support.
/// </summary>
public class ConditionEvaluatorTests
{
    private readonly ConditionEvaluator _evaluator = new();

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static CoreServiceRequest MakeRequest(
        ServiceRequestPriority priority = ServiceRequestPriority.Medium,
        ServiceRequestStatus status = ServiceRequestStatus.New,
        int? categoryId = null,
        DateTime? dueDate = null,
        string subject = "Test Subject")
    {
        var sr = new CoreServiceRequest
        {
            TicketNumber = "TEST-001",
            Subject = subject,
            Priority = priority,
            CategoryId = categoryId,
            DueDate = dueDate
        };
        sr.ChangeStatus(status);
        return sr;
    }

    // -------------------------------------------------------------------------
    // Null / empty JSON
    // -------------------------------------------------------------------------

    [Fact]
    public void Evaluate_NullJson_ShouldReturnTrue()
    {
        var result = _evaluator.Evaluate(null, MakeRequest());
        result.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_EmptyJson_ShouldReturnTrue()
    {
        var result = _evaluator.Evaluate("   ", MakeRequest());
        result.Should().BeTrue();
    }

    // -------------------------------------------------------------------------
    // Simple field comparisons
    // -------------------------------------------------------------------------

    [Fact]
    public void Evaluate_SimplePriorityEq_ShouldMatchCorrectly()
    {
        var json = """{"field":"Priority","op":"eq","value":"High"}""";
        var sr = MakeRequest(priority: ServiceRequestPriority.High);

        _evaluator.Evaluate(json, sr).Should().BeTrue();
        _evaluator.Evaluate(json, MakeRequest(priority: ServiceRequestPriority.Low)).Should().BeFalse();
    }

    [Fact]
    public void Evaluate_SimplePriorityGte_ShouldMatchCorrectly()
    {
        // Critical(3) >= High(2) = true
        var json = """{"field":"Priority","op":"gte","value":"High"}""";

        _evaluator.Evaluate(json, MakeRequest(priority: ServiceRequestPriority.Critical)).Should().BeTrue();
        _evaluator.Evaluate(json, MakeRequest(priority: ServiceRequestPriority.High)).Should().BeTrue();
        _evaluator.Evaluate(json, MakeRequest(priority: ServiceRequestPriority.Low)).Should().BeFalse();
    }

    [Fact]
    public void Evaluate_SimpleStatusEq_ShouldMatchCorrectly()
    {
        var json = """{"field":"Status","op":"eq","value":"Escalated"}""";

        _evaluator.Evaluate(json, MakeRequest(status: ServiceRequestStatus.Escalated)).Should().BeTrue();
        _evaluator.Evaluate(json, MakeRequest(status: ServiceRequestStatus.New)).Should().BeFalse();
    }

    [Fact]
    public void Evaluate_SimpleCategoryIdEq_ShouldMatchCorrectly()
    {
        var json = """{"field":"CategoryId","op":"eq","value":"5"}""";

        _evaluator.Evaluate(json, MakeRequest(categoryId: 5)).Should().BeTrue();
        _evaluator.Evaluate(json, MakeRequest(categoryId: 9)).Should().BeFalse();
        _evaluator.Evaluate(json, MakeRequest(categoryId: null)).Should().BeFalse();
    }

    [Fact]
    public void Evaluate_SimpleSubjectContains_ShouldMatchCaseInsensitive()
    {
        var json = """{"field":"Subject","op":"contains","value":"network"}""";

        _evaluator.Evaluate(json, MakeRequest(subject: "Network outage in DC1")).Should().BeTrue();
        _evaluator.Evaluate(json, MakeRequest(subject: "Printer jam")).Should().BeFalse();
    }

    // -------------------------------------------------------------------------
    // AND compound condition
    // -------------------------------------------------------------------------

    [Fact]
    public void Evaluate_AndCondition_ShouldRequireAllChildren()
    {
        var json = """
        {
            "operator": "AND",
            "conditions": [
                {"field":"Priority","op":"eq","value":"Critical"},
                {"field":"Status","op":"eq","value":"New"}
            ]
        }
        """;

        var matching = MakeRequest(priority: ServiceRequestPriority.Critical, status: ServiceRequestStatus.New);
        var partialMatch = MakeRequest(priority: ServiceRequestPriority.Critical, status: ServiceRequestStatus.InProgress);
        var noMatch = MakeRequest(priority: ServiceRequestPriority.Low, status: ServiceRequestStatus.New);

        _evaluator.Evaluate(json, matching).Should().BeTrue();
        _evaluator.Evaluate(json, partialMatch).Should().BeFalse();
        _evaluator.Evaluate(json, noMatch).Should().BeFalse();
    }

    // -------------------------------------------------------------------------
    // OR compound condition
    // -------------------------------------------------------------------------

    [Fact]
    public void Evaluate_OrCondition_ShouldMatchWhenAnyChildMatches()
    {
        var json = """
        {
            "operator": "OR",
            "conditions": [
                {"field":"Priority","op":"eq","value":"Critical"},
                {"field":"Priority","op":"eq","value":"High"}
            ]
        }
        """;

        _evaluator.Evaluate(json, MakeRequest(priority: ServiceRequestPriority.Critical)).Should().BeTrue();
        _evaluator.Evaluate(json, MakeRequest(priority: ServiceRequestPriority.High)).Should().BeTrue();
        _evaluator.Evaluate(json, MakeRequest(priority: ServiceRequestPriority.Low)).Should().BeFalse();
    }

    // -------------------------------------------------------------------------
    // Nested AND / OR
    // -------------------------------------------------------------------------

    [Fact]
    public void Evaluate_NestedCondition_ShouldEvaluateRecursively()
    {
        // ( Priority=Critical ) AND ( Status=New OR Status=Open )
        var json = """
        {
            "operator": "AND",
            "conditions": [
                {"field":"Priority","op":"eq","value":"Critical"},
                {
                    "operator": "OR",
                    "conditions": [
                        {"field":"Status","op":"eq","value":"New"},
                        {"field":"Status","op":"eq","value":"Open"}
                    ]
                }
            ]
        }
        """;

        _evaluator.Evaluate(json, MakeRequest(priority: ServiceRequestPriority.Critical, status: ServiceRequestStatus.New)).Should().BeTrue();
        _evaluator.Evaluate(json, MakeRequest(priority: ServiceRequestPriority.Critical, status: ServiceRequestStatus.Open)).Should().BeTrue();
        // Critical priority but wrong status
        _evaluator.Evaluate(json, MakeRequest(priority: ServiceRequestPriority.Critical, status: ServiceRequestStatus.Resolved)).Should().BeFalse();
        // Wrong priority
        _evaluator.Evaluate(json, MakeRequest(priority: ServiceRequestPriority.High, status: ServiceRequestStatus.New)).Should().BeFalse();
    }

    // -------------------------------------------------------------------------
    // EvaluateNode direct API
    // -------------------------------------------------------------------------

    [Fact]
    public void EvaluateNode_SimpleNode_ShouldWork()
    {
        var node = new JsonConditionNode { Field = "Priority", Op = "eq", Value = "High" };
        var sr = MakeRequest(priority: ServiceRequestPriority.High);

        _evaluator.EvaluateNode(node, sr).Should().BeTrue();
    }

    [Fact]
    public void EvaluateNode_CompoundAndNoChildren_ShouldReturnTrue()
    {
        var node = new JsonConditionNode
        {
            Operator = "AND",
            Conditions = new List<JsonConditionNode>()
        };

        _evaluator.EvaluateNode(node, MakeRequest()).Should().BeTrue();
    }

    // -------------------------------------------------------------------------
    // Invalid JSON
    // -------------------------------------------------------------------------

    [Fact]
    public void Evaluate_InvalidJson_ShouldReturnFalse()
    {
        var result = _evaluator.Evaluate("{not valid json}", MakeRequest());
        result.Should().BeFalse();
    }
}
