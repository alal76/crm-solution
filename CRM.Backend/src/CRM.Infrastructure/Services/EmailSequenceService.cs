// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services
{
    public class EmailSequenceService : IEmailSequenceService
    {
        private readonly ICrmDbContext _context;
        private readonly ILogger<EmailSequenceService> _logger;

        public EmailSequenceService(ICrmDbContext context, ILogger<EmailSequenceService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<EmailSequence>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.EmailSequences
                .Include(s => s.Steps)
                .Where(s => !s.IsDeleted)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<EmailSequence?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.EmailSequences
                .Include(s => s.Steps)
                .Include(s => s.Enrollments)
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted, cancellationToken);
        }

        public async Task<EmailSequence> UpdateAsync(EmailSequence sequence, CancellationToken cancellationToken = default)
        {
            if (sequence == null)
                throw new ArgumentNullException(nameof(sequence));
            var existing = await _context.EmailSequences
                .FirstOrDefaultAsync(s => s.Id == sequence.Id && !s.IsDeleted, cancellationToken);
            if (existing == null)
                throw new InvalidOperationException($"Sequence {sequence.Id} not found");

            existing.Name = sequence.Name;
            existing.Description = sequence.Description;
            existing.FromEmail = sequence.FromEmail;
            existing.FromName = sequence.FromName;
            existing.ReplyToEmail = sequence.ReplyToEmail;
            existing.SendFromOwner = sequence.SendFromOwner;
            existing.Timezone = sequence.Timezone;
            existing.SendingDays = sequence.SendingDays;
            existing.SendingStartHour = sequence.SendingStartHour;
            existing.SendingEndHour = sequence.SendingEndHour;
            existing.MaxEmailsPerDay = sequence.MaxEmailsPerDay;
            existing.ThrottleMinutes = sequence.ThrottleMinutes;
            existing.ExitOnReply = sequence.ExitOnReply;
            existing.ExitOnMeetingBooked = sequence.ExitOnMeetingBooked;
            existing.ExitOnBounce = sequence.ExitOnBounce;
            existing.ExitOnUnsubscribe = sequence.ExitOnUnsubscribe;

            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Updated email sequence {SequenceId}", sequence.Id);
            return existing;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var sequence = await _context.EmailSequences
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted, cancellationToken);
            if (sequence == null)
                return false;

            sequence.IsDeleted = true;
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Soft-deleted email sequence {SequenceId}", id);
            return true;
        }

        public async Task<EmailSequence> CreateSequenceAsync(EmailSequence sequence, CancellationToken cancellationToken = default)
        {
            if (sequence == null)
                throw new ArgumentNullException(nameof(sequence));
            sequence.CreatedAt = DateTime.UtcNow;

            // Ensure child step entities have valid timestamps (default DateTime.MinValue is rejected by MariaDB)
            // Also ensure Template is non-null to satisfy DB NOT NULL constraint
            if (sequence.Steps != null)
            {
                foreach (var step in sequence.Steps)
                {
                    step.CreatedAt = DateTime.UtcNow;
                    step.Template ??= step.Subject ?? step.Body ?? string.Empty;
                }
            }

            _context.EmailSequences.Add(sequence);
            await _context.SaveChangesAsync(cancellationToken);
            return sequence;
        }

        public async Task<EmailSequenceEnrollment> EnrollContactAsync(int sequenceId, int contactId, int? enrolledById = null, CancellationToken cancellationToken = default)
        {
            var sequence = await _context.EmailSequences
                .Include(s => s.Enrollments)
                .FirstOrDefaultAsync(s => s.Id == sequenceId && !s.IsDeleted, cancellationToken);

            if (sequence == null)
                throw new InvalidOperationException("Sequence not found");

            // prevent duplicate enrollment for same contact
            var exists = await _context.EmailSequenceEnrollments
                .AnyAsync(e => e.EmailSequenceId == sequenceId && e.ContactId == contactId && !e.IsDeleted, cancellationToken);

            if (exists)
            {
                var existing = await _context.EmailSequenceEnrollments.FirstAsync(e => e.EmailSequenceId == sequenceId && e.ContactId == contactId, cancellationToken);
                return existing;
            }

            var enrollment = new EmailSequenceEnrollment
            {
                EmailSequenceId = sequenceId,
                ContactId = contactId,
                EnrolledById = enrolledById,
                EnrolledAt = DateTime.UtcNow,
                Status = EnrollmentStatus.Active
            };

            _context.EmailSequenceEnrollments.Add(enrollment);
            sequence.TotalEnrolled++;
            sequence.ActiveEnrollments++;

            await _context.SaveChangesAsync(cancellationToken);
            return enrollment;
        }

        public async Task<bool> StartSequenceAsync(int sequenceId, CancellationToken cancellationToken = default)
        {
            var sequence = await _context.EmailSequences.FirstOrDefaultAsync(s => s.Id == sequenceId && !s.IsDeleted, cancellationToken);
            if (sequence == null)
                return false;
            sequence.Status = EmailSequenceStatus.Active;
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Started email sequence {SequenceId}", sequenceId);
            return true;
        }

        public async Task<bool> StopSequenceAsync(int sequenceId, CancellationToken cancellationToken = default)
        {
            var sequence = await _context.EmailSequences.FirstOrDefaultAsync(s => s.Id == sequenceId && !s.IsDeleted, cancellationToken);
            if (sequence == null)
                return false;
            sequence.Status = EmailSequenceStatus.Paused;
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Stopped (paused) email sequence {SequenceId}", sequenceId);
            return true;
        }

        public async Task<SequenceStatusDto> GetSequenceStatusAsync(int sequenceId, CancellationToken cancellationToken = default)
        {
            var sequence = await _context.EmailSequences
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == sequenceId && !s.IsDeleted, cancellationToken);

            if (sequence == null)
                throw new InvalidOperationException("Sequence not found");

            var dto = new SequenceStatusDto
            {
                SequenceId = sequence.Id,
                Name = sequence.Name,
                Status = sequence.Status,
                TotalEnrolled = sequence.TotalEnrolled,
                ActiveEnrollments = sequence.ActiveEnrollments,
                TotalCompleted = sequence.TotalCompleted,
                TotalEmailsSent = sequence.TotalEmailsSent
            };

            return dto;
        }

        public async Task<bool> ResumeSequenceAsync(int sequenceId, CancellationToken cancellationToken = default)
        {
            var sequence = await _context.EmailSequences
                .FirstOrDefaultAsync(s => s.Id == sequenceId && !s.IsDeleted, cancellationToken);
            if (sequence == null)
                return false;

            sequence.Status = EmailSequenceStatus.Active;
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Resumed email sequence {SequenceId}", sequenceId);
            return true;
        }

        public async Task<bool> PauseSequenceAsync(int sequenceId, CancellationToken cancellationToken = default)
        {
            var sequence = await _context.EmailSequences
                .FirstOrDefaultAsync(s => s.Id == sequenceId && !s.IsDeleted, cancellationToken);
            if (sequence == null)
                return false;

            sequence.Status = EmailSequenceStatus.Paused;
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Paused email sequence {SequenceId}", sequenceId);
            return true;
        }

        public async Task<bool> CompleteSequenceAsync(int sequenceId, CancellationToken cancellationToken = default)
        {
            var sequence = await _context.EmailSequences
                .FirstOrDefaultAsync(s => s.Id == sequenceId && !s.IsDeleted, cancellationToken);
            if (sequence == null)
                return false;

            sequence.Status = EmailSequenceStatus.Archived;
            sequence.IsActive = false;
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Completed email sequence {SequenceId}", sequenceId);
            return true;
        }

        public async Task<int> EnrollContactsAsync(int sequenceId, List<int> contactIds, int? enrolledById = null, CancellationToken cancellationToken = default)
        {
            if (contactIds == null || contactIds.Count == 0)
                return 0;

            var successCount = 0;
            foreach (var contactId in contactIds.Distinct())
            {
                await EnrollContactAsync(sequenceId, contactId, enrolledById, cancellationToken);
                successCount++;
            }

            return successCount;
        }

        public async Task<bool> UnenrollContactAsync(int sequenceId, int contactId, CancellationToken cancellationToken = default)
        {
            var enrollment = await _context.EmailSequenceEnrollments
                .FirstOrDefaultAsync(e => e.EmailSequenceId == sequenceId && e.ContactId == contactId && !e.IsDeleted, cancellationToken);
            if (enrollment == null)
                return false;

            enrollment.Status = EnrollmentStatus.Removed;
            enrollment.CompletedAt = DateTime.UtcNow;
            enrollment.ExitReason = SequenceExitCondition.OnUnsubscribe;
            enrollment.IsDeleted = true;

            var sequence = await _context.EmailSequences
                .FirstOrDefaultAsync(s => s.Id == sequenceId && !s.IsDeleted, cancellationToken);
            if (sequence != null && sequence.ActiveEnrollments > 0)
            {
                sequence.ActiveEnrollments -= 1;
            }

            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Unenrolled contact {ContactId} from sequence {SequenceId}", contactId, sequenceId);
            return true;
        }

        public async Task<EmailSequenceStepExecution> ExecuteSequenceStepAsync(int stepId, int enrollmentId, CancellationToken cancellationToken = default)
        {
            var step = await _context.EmailSequenceSteps
                .FirstOrDefaultAsync(s => s.Id == stepId && !s.IsDeleted, cancellationToken);
            if (step == null)
                throw new InvalidOperationException("Step not found");

            var enrollment = await _context.EmailSequenceEnrollments
                .FirstOrDefaultAsync(e => e.Id == enrollmentId && !e.IsDeleted, cancellationToken);
            if (enrollment == null)
                throw new InvalidOperationException("Enrollment not found");

            var execution = new EmailSequenceStepExecution
            {
                EmailSequenceStepId = stepId,
                EmailSequenceEnrollmentId = enrollmentId,
                ScheduledAt = DateTime.UtcNow,
                ExecutedAt = DateTime.UtcNow,
                Success = true
            };

            _context.EmailSequenceStepExecutions.Add(execution);

            enrollment.CurrentStepId = stepId;
            enrollment.CurrentStepIndex = step.StepOrder;
            enrollment.LastStepExecutedAt = DateTime.UtcNow;
            enrollment.StepsCompleted += 1;

            step.ExecutionCount += 1;
            if (step.StepType == EmailStepType.Email)
            {
                step.EmailsSent += 1;
                enrollment.EmailsSent += 1;
            }

            await _context.SaveChangesAsync(cancellationToken);
            return execution;
        }

        public async Task<bool> SkipStepAsync(int stepId, int enrollmentId, CancellationToken cancellationToken = default)
        {
            var enrollment = await _context.EmailSequenceEnrollments
                .FirstOrDefaultAsync(e => e.Id == enrollmentId && !e.IsDeleted, cancellationToken);
            if (enrollment == null)
                return false;

            enrollment.CurrentStepId = stepId;
            enrollment.LastStepExecutedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public Task<bool> EvaluateConditionAsync(int conditionId, int contactId, CancellationToken cancellationToken = default)
        {
            // Placeholder until explicit condition entities are implemented.
            return Task.FromResult(true);
        }

        public async Task<bool> RecordStepDeliveryAsync(int stepId, int enrollmentId, string deliveryStatus, CancellationToken cancellationToken = default)
        {
            var execution = await _context.EmailSequenceStepExecutions
                .FirstOrDefaultAsync(e => e.EmailSequenceStepId == stepId && e.EmailSequenceEnrollmentId == enrollmentId, cancellationToken);

            if (execution == null)
            {
                execution = new EmailSequenceStepExecution
                {
                    EmailSequenceStepId = stepId,
                    EmailSequenceEnrollmentId = enrollmentId,
                    ScheduledAt = DateTime.UtcNow,
                    ExecutedAt = DateTime.UtcNow
                };
                _context.EmailSequenceStepExecutions.Add(execution);
            }

            execution.Success = !string.Equals(deliveryStatus, "failed", StringComparison.OrdinalIgnoreCase);
            execution.MessageId = deliveryStatus;

            var step = await _context.EmailSequenceSteps.FirstOrDefaultAsync(s => s.Id == stepId, cancellationToken);
            var enrollment = await _context.EmailSequenceEnrollments.FirstOrDefaultAsync(e => e.Id == enrollmentId, cancellationToken);
            if (step != null)
            {
                step.ExecutionCount += 1;
                step.EmailsSent += 1;
            }
            if (enrollment != null)
            {
                enrollment.EmailsSent += 1;
            }

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> RecordOpeningAsync(int stepId, int enrollmentId, CancellationToken cancellationToken = default)
        {
            var execution = await _context.EmailSequenceStepExecutions
                .FirstOrDefaultAsync(e => e.EmailSequenceStepId == stepId && e.EmailSequenceEnrollmentId == enrollmentId, cancellationToken);

            if (execution != null)
            {
                execution.Opens += 1;
            }

            var enrollment = await _context.EmailSequenceEnrollments.FirstOrDefaultAsync(e => e.Id == enrollmentId, cancellationToken);
            if (enrollment != null)
            {
                enrollment.TotalOpens += 1;
            }

            var step = await _context.EmailSequenceSteps.FirstOrDefaultAsync(s => s.Id == stepId, cancellationToken);
            if (step != null)
            {
                step.Opens += 1;
            }

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> RecordClickAsync(int stepId, int enrollmentId, string url, CancellationToken cancellationToken = default)
        {
            var execution = await _context.EmailSequenceStepExecutions
                .FirstOrDefaultAsync(e => e.EmailSequenceStepId == stepId && e.EmailSequenceEnrollmentId == enrollmentId, cancellationToken);

            if (execution != null)
            {
                execution.Clicks += 1;
            }

            var enrollment = await _context.EmailSequenceEnrollments.FirstOrDefaultAsync(e => e.Id == enrollmentId, cancellationToken);
            if (enrollment != null)
            {
                enrollment.TotalClicks += 1;
            }

            var step = await _context.EmailSequenceSteps.FirstOrDefaultAsync(s => s.Id == stepId, cancellationToken);
            if (step != null)
            {
                step.Clicks += 1;
            }

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<SequenceAnalyticsDto> GetSequenceAnalyticsAsync(int sequenceId, CancellationToken cancellationToken = default)
        {
            var sequence = await _context.EmailSequences
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == sequenceId && !s.IsDeleted, cancellationToken);

            if (sequence == null)
                throw new InvalidOperationException("Sequence not found");

            var totalEnrolled = sequence.TotalEnrolled == 0 ? 1 : sequence.TotalEnrolled;
            return new SequenceAnalyticsDto
            {
                SequenceId = sequence.Id,
                TotalEnrolled = sequence.TotalEnrolled,
                Completed = sequence.TotalCompleted,
                Unsubscribed = sequence.TotalUnsubscribes,
                AverageOpenRate = (decimal)sequence.TotalOpens / totalEnrolled,
                AverageClickRate = (decimal)sequence.TotalClicks / totalEnrolled,
                TotalConverted = sequence.TotalMeetingsBooked
            };
        }

        public async Task<EnrollmentProgressDto> GetRecipientProgressAsync(int enrollmentId, CancellationToken cancellationToken = default)
        {
            var enrollment = await _context.EmailSequenceEnrollments
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == enrollmentId && !e.IsDeleted, cancellationToken);

            if (enrollment == null)
                throw new InvalidOperationException("Enrollment not found");

            return new EnrollmentProgressDto
            {
                EnrollmentId = enrollment.Id,
                SequenceId = enrollment.EmailSequenceId,
                ContactId = enrollment.ContactId ?? 0,
                CurrentStepIndex = enrollment.CurrentStepIndex,
                StartedAt = enrollment.EnrolledAt,
                CompletedAt = enrollment.CompletedAt,
                IsActive = enrollment.Status == EnrollmentStatus.Active,
                OpenCount = enrollment.TotalOpens,
                ClickCount = enrollment.TotalClicks
            };
        }

        /// <summary>
        /// Advances all NurtureEnrollments whose NextStepAt is due. Called by the background service.
        /// </summary>
        public async Task<bool> ProcessDueStepsAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var dueEnrollments = await _context.NurtureEnrollments
                .Include(e => e.Sequence)
                    .ThenInclude(s => s.Steps.OrderBy(step => step.StepOrder))
                .Where(e => !e.IsCompleted && !e.IsUnsubscribed && !e.IsDeleted && e.NextStepAt.HasValue && e.NextStepAt <= now)
                .ToListAsync(cancellationToken);

            if (dueEnrollments.Count == 0)
            {
                return false;
            }

            _logger.LogInformation("Processing {Count} due nurture sequence steps", dueEnrollments.Count);

            foreach (var enrollment in dueEnrollments)
            {
                var steps = enrollment.Sequence?.Steps?.OrderBy(s => s.StepOrder).ToList();
                if (steps == null || steps.Count == 0)
                {
                    enrollment.IsCompleted = true;
                    enrollment.CompletedAt = now;
                    enrollment.UpdatedAt = now;
                    continue;
                }

                var nextStepIndex = enrollment.CurrentStep + 1;
                if (nextStepIndex >= steps.Count)
                {
                    enrollment.IsCompleted = true;
                    enrollment.CompletedAt = now;
                    enrollment.NextStepAt = null;
                    enrollment.UpdatedAt = now;
                    continue;
                }

                var nextStep = steps[nextStepIndex];
                var delayHours = nextStep.DelayDays * 24 + nextStep.DelayHours;

                enrollment.CurrentStep = nextStepIndex;
                enrollment.NextStepAt = delayHours > 0 ? now.AddHours(delayHours) : now.AddMinutes(5);
                enrollment.UpdatedAt = now;

                _logger.LogDebug("Advanced enrollment {EnrollmentId} to step {Step} for {Email}", enrollment.Id, nextStepIndex, enrollment.EnrolleeEmail);
            }

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}

