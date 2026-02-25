// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Threading;
using System.Threading.Tasks;
using CRM.Infrastructure.Services.Saga;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for SagaOrchestrator.
/// Verifies successful saga completion and failure-triggered compensation.
/// </summary>
public class SagaOrchestratorTests
{
    private readonly SagaOrchestrator _orchestrator;

    public SagaOrchestratorTests()
    {
        var mockLogger = new Mock<ILogger<SagaOrchestrator>>();
        _orchestrator = new SagaOrchestrator(mockLogger.Object);
    }

    // ─────────────────────────────────────────────────────────────────────
    // Test 1 – all steps succeed → Success=true, CompletedSteps=count
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task StartSagaAsync_ShouldSucceed_WhenAllStepsReturn_Success()
    {
        // Arrange
        var definition = new SagaDefinition
        {
            Name = "CreateOrderSaga",
            Steps =
            {
                new SagaStep
                {
                    Name = "ReserveInventory",
                    Order = 0,
                    Execute = (ctx, ct) => Task.FromResult(SagaStepResult.Succeeded())
                },
                new SagaStep
                {
                    Name = "ChargePayment",
                    Order = 1,
                    Execute = (ctx, ct) => Task.FromResult(SagaStepResult.Succeeded())
                }
            }
        };

        // Act
        var result = await _orchestrator.StartSagaAsync(definition);

        // Assert
        result.Success.Should().BeTrue();
        result.CompletedSteps.Should().Be(2);
        result.Status.Should().Be(SagaStatus.Completed);
    }

    // ─────────────────────────────────────────────────────────────────────
    // Test 2 – a step fails → Success=false and compensation runs
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task StartSagaAsync_ShouldFail_WhenAStepReturnsFailure()
    {
        // Arrange
        bool compensationRan = false;

        var definition = new SagaDefinition
        {
            Name = "FailingSaga",
            Steps =
            {
                new SagaStep
                {
                    Name = "Step1_OK",
                    Order = 0,
                    Execute = (ctx, ct) => Task.FromResult(SagaStepResult.Succeeded()),
                    Compensate = (ctx, ct) =>
                    {
                        compensationRan = true;
                        return Task.CompletedTask;
                    }
                },
                new SagaStep
                {
                    Name = "Step2_Fails",
                    Order = 1,
                    Execute = (ctx, ct) => Task.FromResult(SagaStepResult.Failed("Payment gateway unreachable"))
                }
            }
        };

        // Act
        var result = await _orchestrator.StartSagaAsync(definition);

        // Assert
        result.Success.Should().BeFalse();
        result.Status.Should().BeOneOf(SagaStatus.Failed, SagaStatus.Compensated);
        compensationRan.Should().BeTrue("Step1 should have been compensated after Step2 failed");
    }

    // ─────────────────────────────────────────────────────────────────────
    // Test 3 – GetSagaStateAsync returns null for an unknown ID
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetSagaStateAsync_ShouldReturnNull_WhenSagaIdIsUnknown()
    {
        // Act
        var state = await _orchestrator.GetSagaStateAsync("nonexistent-id-00000");

        // Assert
        state.Should().BeNull();
    }
}
