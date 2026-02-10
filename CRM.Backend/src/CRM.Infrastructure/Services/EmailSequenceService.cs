using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
