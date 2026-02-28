// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using CRM.Core.Scripting;
using CRM.Core.Scripting.Workflow;
using CRM.Infrastructure.Scripting.Workflow;

namespace CRM.Tests.Integration;

/// <summary>
/// Integration tests for <see cref="YamlWdlParser"/>, <see cref="CelExpressionEvaluator"/>,
/// and <see cref="WorkflowOrchestrator"/>. Covers SARCH-091.
/// Tests cover: YAML parse (script, sequential, parallel, condition steps), validation,
/// CEL expression resolution, orchestrator success and failure/saga paths.
/// </summary>
public class WorkflowIntegrationTests
{
    // ── Helpers ────────────────────────────────────────────────────────────────

    private static YamlWdlParser CreateParser() => new();

    private static CelExpressionEvaluator CreateCelEvaluator() => new();

    private static IDistributedCache BuildCacheMock()
    {
        var mock = new Mock<IDistributedCache>();
        mock.Setup(c => c.SetAsync(
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        mock.Setup(c => c.GetAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        return mock.Object;
    }

    private static WorkflowOrchestrator CreateOrchestrator(IDistributedCache? cache = null)
    {
        var stateStore = cache ?? BuildCacheMock();
        var cel = new CelExpressionEvaluator();
        var engineMock = new Mock<ICompiledScriptEngine>();
        var toolMock = new Mock<IToolInvoker>();

        var executor = new WorkflowStepExecutor(
            engineMock.Object,
            toolMock.Object,
            cel,
            NullLogger<WorkflowStepExecutor>.Instance);

        return new WorkflowOrchestrator(
            executor,
            new YamlWdlParser(),
            stateStore,
            NullLogger<WorkflowOrchestrator>.Instance);
    }

    // ── YamlWdlParser — Parse ─────────────────────────────────────────────────

    [Fact]
    public void Parse_ShouldReturnWorkflowDefinition_WhenValidScriptStepYaml()
    {
        var parser = CreateParser();
        const string yaml = @"
id: wf-001
name: test-workflow
steps:
  - name: step1
    type: Script
    script: my-script-id";

        var def = parser.Parse(yaml);

        Assert.Equal("wf-001", def.Id);
        Assert.Equal("test-workflow", def.Name);
        Assert.Single(def.Steps);
        Assert.Equal("step1", def.Steps[0].Name);
        Assert.Equal(WorkflowStepType.Script, def.Steps[0].Type);
        Assert.Equal("my-script-id", def.Steps[0].Script);
    }

    [Fact]
    public void Parse_ShouldReturnWorkflowDefinition_WhenSequentialSteps()
    {
        var parser = CreateParser();
        const string yaml = @"
id: wf-002
name: sequential-workflow
steps:
  - name: step1
    type: Approval
  - name: step2
    type: Delay
    delaySeconds: 0";

        var def = parser.Parse(yaml);

        Assert.Equal(2, def.Steps.Count);
        Assert.Equal("step1", def.Steps[0].Name);
        Assert.Equal("step2", def.Steps[1].Name);
        Assert.Equal(WorkflowStepType.Approval, def.Steps[0].Type);
        Assert.Equal(WorkflowStepType.Delay, def.Steps[1].Type);
    }

    [Fact]
    public void Parse_ShouldReturnWorkflowDefinition_WhenParallelStep()
    {
        var parser = CreateParser();
        const string yaml = @"
id: wf-003
name: parallel-workflow
steps:
  - name: par-step
    type: Parallel
    parallelBranches:
      - branchA
      - branchB";

        var def = parser.Parse(yaml);

        Assert.Single(def.Steps);
        Assert.Equal(WorkflowStepType.Parallel, def.Steps[0].Type);
        Assert.NotNull(def.Steps[0].ParallelBranches);
        Assert.Equal(2, def.Steps[0].ParallelBranches!.Count);
        Assert.Contains("branchA", def.Steps[0].ParallelBranches);
    }

    [Fact]
    public void Parse_ShouldReturnWorkflowDefinition_WhenConditionStep()
    {
        var parser = CreateParser();
        const string yaml = @"
id: wf-004
name: condition-workflow
steps:
  - name: check
    type: Condition
    condition: ""status == 'active'""";

        var def = parser.Parse(yaml);

        Assert.Single(def.Steps);
        Assert.Equal(WorkflowStepType.Condition, def.Steps[0].Type);
        Assert.Equal("status == 'active'", def.Steps[0].Condition);
    }

    // ── YamlWdlParser — Validate ──────────────────────────────────────────────

    [Fact]
    public void Validate_ShouldReturnError_WhenIdIsMissing()
    {
        var parser = CreateParser();
        var def = new WorkflowDefinition
        {
            Id = string.Empty,
            Name = "test",
            Steps = new List<WorkflowStep>
            {
                new() { Name = "s1", Type = WorkflowStepType.Approval },
            },
        };

        var result = parser.Validate(def);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("id", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_ShouldReturnError_WhenNameIsMissing()
    {
        var parser = CreateParser();
        var def = new WorkflowDefinition
        {
            Id = "wf-999",
            Name = string.Empty,
            Steps = new List<WorkflowStep>
            {
                new() { Name = "s1", Type = WorkflowStepType.Approval },
            },
        };

        var result = parser.Validate(def);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("name", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_ShouldReturnError_WhenNoSteps()
    {
        var parser = CreateParser();
        var def = new WorkflowDefinition
        {
            Id = "wf-999",
            Name = "empty-workflow",
            Steps = new List<WorkflowStep>(),
        };

        var result = parser.Validate(def);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("step", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenAllRequiredFieldsPresent()
    {
        var parser = CreateParser();
        var def = new WorkflowDefinition
        {
            Id = "wf-good",
            Name = "good-workflow",
            Steps = new List<WorkflowStep>
            {
                new() { Name = "s1", Type = WorkflowStepType.Approval },
            },
        };

        var result = parser.Validate(def);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    // ── CelExpressionEvaluator ────────────────────────────────────────────────

    [Fact]
    public void CelEvaluator_ShouldResolveExpression_FromContext()
    {
        var cel = CreateCelEvaluator();
        // Resolver splits on '.' — first part "customer" is looked up, then "name" navigated
        var ctx = new Dictionary<string, object?>
        {
            ["customer"] = new Dictionary<string, object?> { ["name"] = "Alice" },
        };

        var resolved = cel.Resolve("Hello, ${customer.name}!", ctx);

        Assert.Equal("Hello, Alice!", resolved);
    }

    [Fact]
    public void CelEvaluator_ShouldEvaluateEqualityCondition_AsTrue()
    {
        var cel = CreateCelEvaluator();
        var ctx = new Dictionary<string, object?> { ["status"] = "active" };

        var conditionResult = cel.EvaluateCondition("status == 'active'", ctx);

        Assert.True(conditionResult);
    }

    // ── WorkflowOrchestrator ──────────────────────────────────────────────────

    [Fact]
    public async Task Orchestrator_ShouldSucceed_WhenRunningApprovalStep()
    {
        var orchestrator = CreateOrchestrator();
        const string yaml = @"
id: wf-t01
name: approval-test
steps:
  - name: approve
    type: Approval";

        var result = await orchestrator.RunAsync(yaml);

        Assert.True(result.Success);
        Assert.Single(result.StepResults);
        Assert.Equal("approve", result.StepResults[0].StepName);
        Assert.True(result.StepResults[0].Success);
    }

    [Fact]
    public async Task Orchestrator_ShouldSucceed_WhenRunningSequentialDelaySteps()
    {
        var orchestrator = CreateOrchestrator();
        const string yaml = @"
id: wf-t02
name: sequential-delay-test
steps:
  - name: step1
    type: Delay
    delaySeconds: 0
  - name: step2
    type: Delay
    delaySeconds: 0";

        var result = await orchestrator.RunAsync(yaml);

        Assert.True(result.Success);
        Assert.Equal(2, result.StepResults.Count);
        Assert.All(result.StepResults, r => Assert.True(r.Success));
    }

    [Fact]
    public async Task Orchestrator_ShouldFail_WhenScriptStepHasNoScriptId()
    {
        // Script step with no script field → executor returns failure immediately
        // (no ICompiledScriptEngine call needed — fails at guard clause)
        var orchestrator = CreateOrchestrator();
        const string yaml = @"
id: wf-t03
name: script-fail-test
steps:
  - name: bad-script
    type: Script";

        var result = await orchestrator.RunAsync(yaml);

        Assert.False(result.Success);
        Assert.Single(result.StepResults);
        Assert.False(result.StepResults[0].Success);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public async Task Orchestrator_ShouldRunSagaCompensation_WhenSecondStepFails()
    {
        // First step succeeds (Approval), second fails (Script with no script ID)
        // Verifies the failure path records both step results correctly
        var orchestrator = CreateOrchestrator();
        const string yaml = @"
id: wf-t04
name: saga-test
steps:
  - name: s1
    type: Approval
  - name: s2
    type: Script";

        var result = await orchestrator.RunAsync(yaml);

        Assert.False(result.Success);
        Assert.Equal(2, result.StepResults.Count);
        Assert.True(result.StepResults[0].Success,  "First step (Approval) should succeed");
        Assert.False(result.StepResults[1].Success, "Second step (Script/no-id) should fail");
        Assert.NotNull(result.Error);
    }
}
