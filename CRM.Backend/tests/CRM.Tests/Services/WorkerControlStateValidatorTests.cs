// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.ComponentModel.DataAnnotations;
using CRM.Infrastructure.Validation;
using FluentAssertions;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for <see cref="WorkerControlStateValidator"/>.
///
/// TODO-ARCH-013-004: Validate WorkerControlState values in API
/// </summary>
public class WorkerControlStateValidatorTests
{
    // -------------------------------------------------------------------------
    // Validate(workerName, state) — happy path
    // -------------------------------------------------------------------------

    [Fact]
    public void Validate_ShouldNotThrow_WhenWorkerNameAndStateAreValid()
    {
        // Arrange — pick a known worker and a known state.
        const string workerName = "RecurringBillingWorker";
        const string state = "Running";

        // Act & Assert — must complete without throwing.
        var act = () => WorkerControlStateValidator.Validate(workerName, state);
        act.Should().NotThrow();
    }

    // -------------------------------------------------------------------------
    // Validate(workerName, state) — unknown worker name
    // -------------------------------------------------------------------------

    [Fact]
    public void Validate_ShouldThrowValidationException_WhenWorkerNameIsUnknown()
    {
        // Arrange
        const string unknownWorker = "SomeUnknownWorker";
        const string state = "Running";

        // Act & Assert
        var act = () => WorkerControlStateValidator.Validate(unknownWorker, state);
        act.Should()
            .Throw<ValidationException>()
            .WithMessage($"*{unknownWorker}*");
    }

    // -------------------------------------------------------------------------
    // Validate(workerName, state) — valid worker, invalid state
    // -------------------------------------------------------------------------

    [Fact]
    public void Validate_ShouldThrowValidationException_WhenStateIsInvalid()
    {
        // Arrange
        const string workerName = "DunningWorker";
        const string invalidState = "Turbo";

        // Act & Assert
        var act = () => WorkerControlStateValidator.Validate(workerName, invalidState);
        act.Should()
            .Throw<ValidationException>()
            .WithMessage($"*{invalidState}*");
    }

    // -------------------------------------------------------------------------
    // ValidateState(state) — invalid state-only variant
    // -------------------------------------------------------------------------

    [Fact]
    public void ValidateState_ShouldThrowValidationException_WhenStateIsInvalid()
    {
        // Arrange
        const string invalidState = "Exploding";

        // Act & Assert
        var act = () => WorkerControlStateValidator.ValidateState(invalidState);
        act.Should()
            .Throw<ValidationException>()
            .WithMessage($"*{invalidState}*");
    }
}
