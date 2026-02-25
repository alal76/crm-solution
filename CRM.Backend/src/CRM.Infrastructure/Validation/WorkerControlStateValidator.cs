// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.ComponentModel.DataAnnotations;

namespace CRM.Infrastructure.Validation;

/// <summary>
/// Validates worker names and states passed to the worker control API.
///
/// TODO-ARCH-013-004: Validate WorkerControlState values in API
/// </summary>
public static class WorkerControlStateValidator
{
    /// <summary>
    /// Recognised background worker names.
    /// </summary>
    public static readonly IReadOnlyList<string> KnownWorkerNames = new[]
    {
        "RecurringBillingWorker",
        "DunningWorker",
        "EmailSequenceWorker",
        "EscalationWorker",
        "SLAEnforcementWorker"
    };

    /// <summary>
    /// Recognised worker control states.
    /// </summary>
    public static readonly IReadOnlyList<string> KnownStates = new[]
    {
        "Running",
        "Paused",
        "Stopped"
    };

    /// <summary>
    /// Validates that <paramref name="workerName"/> and <paramref name="state"/>
    /// are both in the recognised lists.
    /// </summary>
    /// <param name="workerName">Name of the worker to control.</param>
    /// <param name="state">Desired control state.</param>
    /// <exception cref="ValidationException">
    /// Thrown when <paramref name="workerName"/> or <paramref name="state"/>
    /// is not in the known values list.
    /// </exception>
    public static void Validate(string workerName, string state)
    {
        if (!KnownWorkerNames.Contains(workerName, StringComparer.OrdinalIgnoreCase))
        {
            throw new ValidationException(
                $"Unknown worker name '{workerName}'. " +
                $"Valid worker names are: {string.Join(", ", KnownWorkerNames)}.");
        }

        if (!KnownStates.Contains(state, StringComparer.OrdinalIgnoreCase))
        {
            throw new ValidationException(
                $"Unknown worker state '{state}'. " +
                $"Valid states are: {string.Join(", ", KnownStates)}.");
        }
    }

    /// <summary>
    /// Validates only the <paramref name="state"/> string (no worker-name check).
    /// Used for global (all-workers) control commands.
    /// </summary>
    /// <param name="state">Desired control state.</param>
    /// <exception cref="ValidationException">
    /// Thrown when <paramref name="state"/> is not a known value.
    /// </exception>
    public static void ValidateState(string state)
    {
        if (!KnownStates.Contains(state, StringComparer.OrdinalIgnoreCase))
        {
            throw new ValidationException(
                $"Unknown worker state '{state}'. " +
                $"Valid states are: {string.Join(", ", KnownStates)}.");
        }
    }
}
