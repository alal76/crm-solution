// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CRM.Core.Dtos;
using CRM.Core.Entities;

namespace CRM.Core.Interfaces
{
    /// <summary>
    /// Email Sequence Service - Drip campaign automation with conditional branching.
    ///
    /// Features:
    /// - Create sequences with multiple steps (3-10 emails)
    /// - Each step can have delay + condition + next email
    /// - Automatic recipient enrollment (manual or via filter)
    /// - Track recipient progress through sequence
    /// - Record opens/clicks for engagement metrics
    /// - Conditional branching (IF opened THEN send X ELSE send Y)
    ///
    /// SPEC: PHASE 7 - Email Sequence Service (20 hours)
    /// </summary>
    public interface IEmailSequenceService
    {
        // CRUD Operations
        Task<IEnumerable<EmailSequence>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<EmailSequence?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<EmailSequence> CreateSequenceAsync(EmailSequence sequence, CancellationToken cancellationToken = default);

        Task<EmailSequence> UpdateAsync(EmailSequence sequence, CancellationToken cancellationToken = default);

        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

        // Lifecycle Operations
        Task<bool> StartSequenceAsync(int sequenceId, CancellationToken cancellationToken = default);

        Task<bool> StopSequenceAsync(int sequenceId, CancellationToken cancellationToken = default);

        Task<bool> ResumeSequenceAsync(int sequenceId, CancellationToken cancellationToken = default);

        Task<bool> PauseSequenceAsync(int sequenceId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Mark a sequence as complete (no more emails will be sent).
        /// </summary>
        Task<bool> CompleteSequenceAsync(int sequenceId, CancellationToken cancellationToken = default);

        // Enrollment Operations
        Task<EmailSequenceEnrollment> EnrollContactAsync(int sequenceId, int contactId, int? enrolledById = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Bulk enroll multiple contacts in a sequence.
        /// </summary>
        Task<int> EnrollContactsAsync(int sequenceId, List<int> contactIds, int? enrolledById = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Unenroll a contact and stop sending them emails in this sequence.
        /// </summary>
        Task<bool> UnenrollContactAsync(int sequenceId, int contactId, CancellationToken cancellationToken = default);

        // Step Execution
        Task<EmailSequenceStepExecution> ExecuteSequenceStepAsync(int stepId, int enrollmentId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Skip to next step without sending current email (e.g., if condition not met).
        /// </summary>
        Task<bool> SkipStepAsync(int stepId, int enrollmentId, CancellationToken cancellationToken = default);

        // Condition Evaluation
        /// <summary>
        /// Evaluate if a condition is met for a contact.
        /// Conditions: EmailOpened, EmailClicked, TimeDelay, CustomFieldMatches, BooleanLogic
        /// </summary>
        Task<bool> EvaluateConditionAsync(int conditionId, int contactId, CancellationToken cancellationToken = default);

        // Tracking
        /// <summary>
        /// Record that a step was delivered to a contact.
        /// </summary>
        Task<bool> RecordStepDeliveryAsync(int stepId, int enrollmentId, string deliveryStatus, CancellationToken cancellationToken = default);

        /// <summary>
        /// Record that a contact opened an email from a sequence step.
        /// </summary>
        Task<bool> RecordOpeningAsync(int stepId, int enrollmentId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Record that a contact clicked a link in a sequence step email.
        /// </summary>
        Task<bool> RecordClickAsync(int stepId, int enrollmentId, string url, CancellationToken cancellationToken = default);

        // Status & Analytics
        Task<SequenceStatusDto> GetSequenceStatusAsync(int sequenceId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get detailed analytics for a sequence (opens, clicks, conversions, etc).
        /// </summary>
        Task<SequenceAnalyticsDto> GetSequenceAnalyticsAsync(int sequenceId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get recipient progress through a sequence.
        /// </summary>
        Task<EnrollmentProgressDto> GetRecipientProgressAsync(int enrollmentId, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Analytics for a sequence including engagement metrics.
    /// </summary>
    public class SequenceAnalyticsDto
    {
        public int SequenceId { get; set; }
        public int TotalEnrolled { get; set; }
        public int Completed { get; set; }
        public int Unsubscribed { get; set; }
        public decimal AverageOpenRate { get; set; }
        public decimal AverageClickRate { get; set; }
        public int TotalConverted { get; set; }
    }

    /// <summary>
    /// Progress of a recipient through a sequence.
    /// </summary>
    public class EnrollmentProgressDto
    {
        public int EnrollmentId { get; set; }
        public int SequenceId { get; set; }
        public int ContactId { get; set; }
        public int CurrentStepIndex { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public bool IsActive { get; set; }
        public int OpenCount { get; set; }
        public int ClickCount { get; set; }
    }
}
