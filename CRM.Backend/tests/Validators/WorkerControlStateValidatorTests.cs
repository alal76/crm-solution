// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System.ComponentModel.DataAnnotations;
using CRM.Infrastructure.Validation;
using Xunit;

namespace CRM.Tests.Validators
{
    /// <summary>
    /// Comprehensive unit tests for <see cref="WorkerControlStateValidator"/>.
    /// Tests validation of worker names and control states for background worker management.
    /// 
    /// Related: TODO-ARCH-013-004: Validate WorkerControlState values in API
    /// </summary>
    public class WorkerControlStateValidatorTests
    {
        // ═══════════════════════════════════════════════════════════════════════════
        // Validate(workerName, state) Tests
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Verifies that all known worker names with valid states pass validation.
        /// </summary>
        [Theory]
        [InlineData("RecurringBillingWorker", "Running")]
        [InlineData("DunningWorker", "Paused")]
        [InlineData("EmailSequenceWorker", "Stopped")]
        [InlineData("EscalationWorker", "Running")]
        [InlineData("SLAEnforcementWorker", "Paused")]
        public void Validate_ShouldNotThrow_WhenWorkerNameAndStateAreValid(string workerName, string state)
        {
            // Act & Assert
            var exception = Record.Exception(() => 
                WorkerControlStateValidator.Validate(workerName, state));

            Assert.Null(exception);
        }

        /// <summary>
        /// Verifies that all valid states are accepted.
        /// </summary>
        [Theory]
        [InlineData("Running")]
        [InlineData("Paused")]
        [InlineData("Stopped")]
        public void Validate_ShouldNotThrow_WhenStateIsValid(string state)
        {
            // Arrange
            var validWorkerName = "RecurringBillingWorker";

            // Act & Assert
            var exception = Record.Exception(() => 
                WorkerControlStateValidator.Validate(validWorkerName, state));

            Assert.Null(exception);
        }

        /// <summary>
        /// Verifies that worker name validation is case-insensitive.
        /// </summary>
        [Theory]
        [InlineData("recurringbillingworker")]
        [InlineData("DUNNINGWORKER")]
        [InlineData("EmailSequenceWorker")]
        [InlineData("escalationWORKER")]
        public void Validate_ShouldBeCaseInsensitive_ForWorkerName(string workerName)
        {
            // Arrange
            var validState = "Running";

            // Act & Assert
            var exception = Record.Exception(() => 
                WorkerControlStateValidator.Validate(workerName, validState));

            Assert.Null(exception);
        }

        /// <summary>
        /// Verifies that state validation is case-insensitive.
        /// </summary>
        [Theory]
        [InlineData("running")]
        [InlineData("PAUSED")]
        [InlineData("Stopped")]
        [InlineData("RuNnInG")]
        public void Validate_ShouldBeCaseInsensitive_ForState(string state)
        {
            // Arrange
            var validWorkerName = "RecurringBillingWorker";

            // Act & Assert
            var exception = Record.Exception(() => 
                WorkerControlStateValidator.Validate(validWorkerName, state));

            Assert.Null(exception);
        }

        /// <summary>
        /// Verifies that unknown worker names throw ValidationException.
        /// </summary>
        [Theory]
        [InlineData("UnknownWorker")]
        [InlineData("InvalidWorker")]
        [InlineData("FakeWorker")]
        [InlineData("")]
        public void Validate_ShouldThrowValidationException_WhenWorkerNameIsUnknown(string unknownWorker)
        {
            // Arrange
            var validState = "Running";

            // Act & Assert
            var exception = Assert.Throws<ValidationException>(() => 
                WorkerControlStateValidator.Validate(unknownWorker, validState));

            Assert.Contains("Unknown worker name", exception.Message);
            Assert.Contains("Valid worker names are:", exception.Message);
        }

        /// <summary>
        /// Verifies that unknown states throw ValidationException.
        /// </summary>
        [Theory]
        [InlineData("Active")]
        [InlineData("Inactive")]
        [InlineData("Disabled")]
        [InlineData("Turbo")]
        [InlineData("")]
        public void Validate_ShouldThrowValidationException_WhenStateIsInvalid(string invalidState)
        {
            // Arrange
            var validWorkerName = "DunningWorker";

            // Act & Assert
            var exception = Assert.Throws<ValidationException>(() => 
                WorkerControlStateValidator.Validate(validWorkerName, invalidState));

            Assert.Contains("Unknown worker state", exception.Message);
            Assert.Contains("Valid states are:", exception.Message);
        }

        /// <summary>
        /// Verifies that exception message includes all valid worker names.
        /// </summary>
        [Fact]
        public void Validate_ShouldIncludeAllValidWorkerNames_InException()
        {
            // Arrange
            var unknownWorker = "InvalidWorker";
            var validState = "Running";

            // Act
            var exception = Assert.Throws<ValidationException>(() => 
                WorkerControlStateValidator.Validate(unknownWorker, validState));

            // Assert - verify all known worker names are mentioned
            Assert.Contains("RecurringBillingWorker", exception.Message);
            Assert.Contains("DunningWorker", exception.Message);
            Assert.Contains("EmailSequenceWorker", exception.Message);
            Assert.Contains("EscalationWorker", exception.Message);
            Assert.Contains("SLAEnforcementWorker", exception.Message);
        }

        /// <summary>
        /// Verifies that exception message includes all valid states.
        /// </summary>
        [Fact]
        public void Validate_ShouldIncludeAllValidStates_InException()
        {
            // Arrange
            var validWorkerName = "DunningWorker";
            var invalidState = "InvalidState";

            // Act
            var exception = Assert.Throws<ValidationException>(() => 
                WorkerControlStateValidator.Validate(validWorkerName, invalidState));

            // Assert - verify all known states are mentioned
            Assert.Contains("Running", exception.Message);
            Assert.Contains("Paused", exception.Message);
            Assert.Contains("Stopped", exception.Message);
        }

        /// <summary>
        /// Verifies that both invalid worker name and state are caught (worker name checked first).
        /// </summary>
        [Fact]
        public void Validate_ShouldThrowForWorkerName_WhenBothAreInvalid()
        {
            // Arrange
            var unknownWorker = "FakeWorker";
            var invalidState = "InvalidState";

            // Act & Assert - expects worker name to be validated first
            var exception = Assert.Throws<ValidationException>(() => 
                WorkerControlStateValidator.Validate(unknownWorker, invalidState));

            Assert.Contains("Unknown worker name", exception.Message);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // ValidateState(state) Tests
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Verifies that ValidateState accepts all valid states.
        /// </summary>
        [Theory]
        [InlineData("Running")]
        [InlineData("Paused")]
        [InlineData("Stopped")]
        public void ValidateState_ShouldNotThrow_WhenStateIsValid(string state)
        {
            // Act & Assert
            var exception = Record.Exception(() => 
                WorkerControlStateValidator.ValidateState(state));

            Assert.Null(exception);
        }

        /// <summary>
        /// Verifies that ValidateState is case-insensitive.
        /// </summary>
        [Theory]
        [InlineData("running")]
        [InlineData("PAUSED")]
        [InlineData("Stopped")]
        [InlineData("RuNnInG")]
        public void ValidateState_ShouldBeCaseInsensitive(string state)
        {
            // Act & Assert
            var exception = Record.Exception(() => 
                WorkerControlStateValidator.ValidateState(state));

            Assert.Null(exception);
        }

        /// <summary>
        /// Verifies that ValidateState throws for invalid states.
        /// </summary>
        [Theory]
        [InlineData("Active")]
        [InlineData("Inactive")]
        [InlineData("Enabled")]
        [InlineData("Disabled")]
        [InlineData("Exploding")]
        [InlineData("")]
        public void ValidateState_ShouldThrowValidationException_WhenStateIsInvalid(string invalidState)
        {
            // Act & Assert
            var exception = Assert.Throws<ValidationException>(() => 
                WorkerControlStateValidator.ValidateState(invalidState));

            Assert.Contains("Unknown worker state", exception.Message);
            Assert.Contains("Valid states are:", exception.Message);
        }

        /// <summary>
        /// Verifies that ValidateState exception includes all valid states.
        /// </summary>
        [Fact]
        public void ValidateState_ShouldIncludeAllValidStates_InException()
        {
            // Arrange
            var invalidState = "InvalidState";

            // Act
            var exception = Assert.Throws<ValidationException>(() => 
                WorkerControlStateValidator.ValidateState(invalidState));

            // Assert
            Assert.Contains("Running", exception.Message);
            Assert.Contains("Paused", exception.Message);
            Assert.Contains("Stopped", exception.Message);
        }

        /// <summary>
        /// Verifies that ValidateState does not mention worker names in error.
        /// </summary>
        [Fact]
        public void ValidateState_ShouldNotMentionWorkerNames_InException()
        {
            // Arrange
            var invalidState = "InvalidState";

            // Act
            var exception = Assert.Throws<ValidationException>(() => 
                WorkerControlStateValidator.ValidateState(invalidState));

            // Assert - should not contain worker names
            Assert.DoesNotContain("RecurringBillingWorker", exception.Message);
            Assert.DoesNotContain("DunningWorker", exception.Message);
            Assert.DoesNotContain("worker name", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // KnownWorkerNames and KnownStates Tests
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Verifies that KnownWorkerNames contains expected workers.
        /// </summary>
        [Fact]
        public void KnownWorkerNames_ShouldContainExpectedWorkers()
        {
            // Assert
            Assert.Contains("RecurringBillingWorker", WorkerControlStateValidator.KnownWorkerNames);
            Assert.Contains("DunningWorker", WorkerControlStateValidator.KnownWorkerNames);
            Assert.Contains("EmailSequenceWorker", WorkerControlStateValidator.KnownWorkerNames);
            Assert.Contains("EscalationWorker", WorkerControlStateValidator.KnownWorkerNames);
            Assert.Contains("SLAEnforcementWorker", WorkerControlStateValidator.KnownWorkerNames);
        }

        /// <summary>
        /// Verifies that KnownWorkerNames has correct count.
        /// </summary>
        [Fact]
        public void KnownWorkerNames_ShouldHaveExpectedCount()
        {
            // Assert
            Assert.Equal(5, WorkerControlStateValidator.KnownWorkerNames.Count);
        }

        /// <summary>
        /// Verifies that KnownStates contains expected states.
        /// </summary>
        [Fact]
        public void KnownStates_ShouldContainExpectedStates()
        {
            // Assert
            Assert.Contains("Running", WorkerControlStateValidator.KnownStates);
            Assert.Contains("Paused", WorkerControlStateValidator.KnownStates);
            Assert.Contains("Stopped", WorkerControlStateValidator.KnownStates);
        }

        /// <summary>
        /// Verifies that KnownStates has correct count.
        /// </summary>
        [Fact]
        public void KnownStates_ShouldHaveExpectedCount()
        {
            // Assert
            Assert.Equal(3, WorkerControlStateValidator.KnownStates.Count);
        }

        /// <summary>
        /// Verifies that KnownWorkerNames is read-only (cannot be modified).
        /// </summary>
        [Fact]
        public void KnownWorkerNames_ShouldBeReadOnly()
        {
            // Assert
            Assert.IsAssignableFrom<IReadOnlyList<string>>(WorkerControlStateValidator.KnownWorkerNames);
        }

        /// <summary>
        /// Verifies that KnownStates is read-only (cannot be modified).
        /// </summary>
        [Fact]
        public void KnownStates_ShouldBeReadOnly()
        {
            // Assert
            Assert.IsAssignableFrom<IReadOnlyList<string>>(WorkerControlStateValidator.KnownStates);
        }
    }
}
