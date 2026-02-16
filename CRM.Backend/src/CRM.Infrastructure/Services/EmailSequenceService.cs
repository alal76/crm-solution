// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

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
            if (sequence == null) throw new ArgumentNullException(nameof(sequence));
            var existing = await _context.EmailSequences
                .FirstOrDefaultAsync(s => s.Id == sequence.Id && !s.IsDeleted, cancellationToken);
            if (existing == null) throw new InvalidOperationException($"Sequence {sequence.Id} not found");

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
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Updated email sequence {SequenceId}", sequence.Id);
            return existing;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var sequence = await _context.EmailSequences
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted, cancellationToken);
            if (sequence == null) return false;

            sequence.IsDeleted = true;
            sequence.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Soft-deleted email sequence {SequenceId}", id);
            return true;
        }

        public async Task<EmailSequence> CreateSequenceAsync(EmailSequence sequence, CancellationToken cancellationToken = default)
        {
            if (sequence == null) throw new ArgumentNullException(nameof(sequence));
            sequence.CreatedAt = DateTime.UtcNow;
            sequence.UpdatedAt = DateTime.UtcNow;
            _context.EmailSequences.Add(sequence);
            await _context.SaveChangesAsync(cancellationToken);
            return sequence;
        }

        public async Task<EmailSequenceEnrollment> EnrollContactAsync(int sequenceId, int contactId, int? enrolledById = null, CancellationToken cancellationToken = default)
        {
            var sequence = await _context.EmailSequences
                .Include(s => s.Enrollments)
                .FirstOrDefaultAsync(s => s.Id == sequenceId && !s.IsDeleted, cancellationToken);

            if (sequence == null) throw new InvalidOperationException("Sequence not found");

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
            sequence.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            return enrollment;
        }

        public async Task<bool> StartSequenceAsync(int sequenceId, CancellationToken cancellationToken = default)
        {
            var sequence = await _context.EmailSequences.FirstOrDefaultAsync(s => s.Id == sequenceId && !s.IsDeleted, cancellationToken);
            if (sequence == null) return false;
            sequence.Status = EmailSequenceStatus.Active;
            sequence.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Started email sequence {SequenceId}", sequenceId);
            return true;
        }

        public async Task<bool> StopSequenceAsync(int sequenceId, CancellationToken cancellationToken = default)
        {
            var sequence = await _context.EmailSequences.FirstOrDefaultAsync(s => s.Id == sequenceId && !s.IsDeleted, cancellationToken);
            if (sequence == null) return false;
            sequence.Status = EmailSequenceStatus.Paused;
            sequence.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Stopped (paused) email sequence {SequenceId}", sequenceId);
            return true;
        }

        public async Task<SequenceStatusDto> GetSequenceStatusAsync(int sequenceId, CancellationToken cancellationToken = default)
        {
            var sequence = await _context.EmailSequences
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == sequenceId && !s.IsDeleted, cancellationToken);

            if (sequence == null) throw new InvalidOperationException("Sequence not found");

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
    }
}
