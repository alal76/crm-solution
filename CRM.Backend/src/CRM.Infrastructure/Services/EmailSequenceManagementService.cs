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

using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Core.Ports.Input;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Implementation of IEmailSequenceManagementService for email sequence operations.
/// Handles sequence CRUD, enrollment, and execution.
/// </summary>
public class EmailSequenceManagementService : IEmailSequenceManagementService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<EmailSequenceManagementService> _logger;

    public EmailSequenceManagementService(ICrmDbContext context, ILogger<EmailSequenceManagementService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Sequence CRUD

    public async Task<IEnumerable<EmailSequenceDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var sequences = await _context.EmailSequences
            .Where(e => !e.IsDeleted)
            .OrderBy(e => e.Name)
            .ToListAsync(cancellationToken);

        return sequences.Select(MapToDto);
    }

    public async Task<EmailSequenceDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var sequence = await _context.EmailSequences
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted, cancellationToken);

        return sequence != null ? MapToDto(sequence) : null;
    }

    public async Task<EmailSequenceDto> CreateAsync(CreateEmailSequenceDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Sequence name is required", nameof(dto.Name));

        var sequence = new EmailSequence
        {
            Name = dto.Name,
            Description = dto.Description,
            Status = "Draft",
            SequenceType = dto.SequenceType,
            DefaultFromName = dto.DefaultFromName,
            DefaultFromEmail = dto.DefaultFromEmail,
            DefaultReplyTo = dto.DefaultReplyTo,
            OwnerId = dto.OwnerId,
            CampaignId = dto.CampaignId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.EmailSequences.Add(sequence);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Email sequence '{SequenceName}' created with ID {SequenceId}", sequence.Name, sequence.Id);
        return await GetByIdAsync(sequence.Id, cancellationToken) ?? throw new InvalidOperationException("Creation failed");
    }

    public async Task<EmailSequenceDto> UpdateAsync(int id, UpdateEmailSequenceDto dto, CancellationToken cancellationToken = default)
    {
        var sequence = await _context.EmailSequences
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted, cancellationToken);

        if (sequence == null)
            throw new InvalidOperationException($"Email sequence {id} not found");

        if (!string.IsNullOrWhiteSpace(dto.Name))
            sequence.Name = dto.Name;

        if (dto.Description != null)
            sequence.Description = dto.Description;

        if (dto.DefaultFromName != null)
            sequence.DefaultFromName = dto.DefaultFromName;

        if (dto.DefaultFromEmail != null)
            sequence.DefaultFromEmail = dto.DefaultFromEmail;

        sequence.UpdatedAt = DateTime.UtcNow;
        _context.EmailSequences.Update(sequence);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Email sequence {SequenceId} updated", id);
        return await GetByIdAsync(sequence.Id, cancellationToken) ?? throw new InvalidOperationException("Update failed");
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var sequence = await _context.EmailSequences
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted, cancellationToken);

        if (sequence == null)
            return false;

        sequence.IsDeleted = true;
        sequence.UpdatedAt = DateTime.UtcNow;
        _context.EmailSequences.Update(sequence);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Email sequence {SequenceId} deleted", id);
        return true;
    }

    #endregion

    #region Steps

    public async Task<EmailSequenceStepDto> AddStepAsync(int sequenceId, CreateEmailSequenceStepDto dto, CancellationToken cancellationToken = default)
    {
        var sequence = await _context.EmailSequences
            .FirstOrDefaultAsync(e => e.Id == sequenceId && !e.IsDeleted, cancellationToken);

        if (sequence == null)
            throw new InvalidOperationException($"Email sequence {sequenceId} not found");

        var step = new EmailSequenceStep
        {
            EmailSequenceId = sequenceId,
            Subject = dto.Subject,
            BodyHtml = dto.BodyHtml,
            DelayDays = dto.DelayDays ?? 0,
            DelayHours = dto.DelayHours ?? 0,
            StepOrder = dto.StepOrder,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.EmailSequenceSteps.Add(step);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Step added to sequence {SequenceId}", sequenceId);
        return MapStepToDto(step);
    }

    public async Task<EmailSequenceStepDto> UpdateStepAsync(int sequenceId, int stepId, CreateEmailSequenceStepDto dto, CancellationToken cancellationToken = default)
    {
        var step = await _context.EmailSequenceSteps
            .FirstOrDefaultAsync(s => s.Id == stepId && s.EmailSequenceId == sequenceId && !s.IsDeleted, cancellationToken);

        if (step == null)
            throw new InvalidOperationException($"Step {stepId} not found in sequence {sequenceId}");

        step.Subject = dto.Subject;
        step.BodyHtml = dto.BodyHtml;
        step.DelayDays = dto.DelayDays ?? 0;
        step.DelayHours = dto.DelayHours ?? 0;
        step.StepOrder = dto.StepOrder;
        step.UpdatedAt = DateTime.UtcNow;

        _context.EmailSequenceSteps.Update(step);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Step {StepId} updated", stepId);
        return MapStepToDto(step);
    }

    public async Task<bool> RemoveStepAsync(int sequenceId, int stepId, CancellationToken cancellationToken = default)
    {
        var step = await _context.EmailSequenceSteps
            .FirstOrDefaultAsync(s => s.Id == stepId && s.EmailSequenceId == sequenceId && !s.IsDeleted, cancellationToken);

        if (step == null)
            return false;

        step.IsDeleted = true;
        step.UpdatedAt = DateTime.UtcNow;
        _context.EmailSequenceSteps.Update(step);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Step {StepId} removed from sequence {SequenceId}", stepId, sequenceId);
        return true;
    }

    public async Task<bool> ReorderStepsAsync(int sequenceId, List<int> stepOrder, CancellationToken cancellationToken = default)
    {
        var steps = await _context.EmailSequenceSteps
            .Where(s => s.EmailSequenceId == sequenceId && !s.IsDeleted)
            .ToListAsync(cancellationToken);

        for (int i = 0; i < stepOrder.Count; i++)
        {
            var step = steps.FirstOrDefault(s => s.Id == stepOrder[i]);
            if (step != null)
            {
                step.StepOrder = i + 1;
                step.UpdatedAt = DateTime.UtcNow;
            }
        }

        _context.EmailSequenceSteps.UpdateRange(steps);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Steps in sequence {SequenceId} reordered", sequenceId);
        return true;
    }

    #endregion

    #region Enrollments

    public async Task<EmailSequenceEnrollmentDto> EnrollAsync(int sequenceId, CreateEmailSequenceEnrollmentDto dto, CancellationToken cancellationToken = default)
    {
        var sequence = await _context.EmailSequences
            .FirstOrDefaultAsync(e => e.Id == sequenceId && !e.IsDeleted, cancellationToken);

        if (sequence == null)
            throw new InvalidOperationException($"Email sequence {sequenceId} not found");

        var contact = await _context.Contacts
            .FirstOrDefaultAsync(c => c.Id == dto.ContactId && !c.IsDeleted, cancellationToken);

        if (contact == null)
            throw new InvalidOperationException($"Contact {dto.ContactId} not found");

        var enrollment = new EmailSequenceEnrollment
        {
            EmailSequenceId = sequenceId,
            ContactId = dto.ContactId,
            Status = "Active",
            EnrolledAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.EmailSequenceEnrollments.Add(enrollment);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Contact {ContactId} enrolled in sequence {SequenceId}", dto.ContactId, sequenceId);
        return MapEnrollmentToDto(enrollment);
    }

    public async Task<List<EmailSequenceEnrollmentDto>> GetEnrollmentsAsync(int sequenceId, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var enrollments = await _context.EmailSequenceEnrollments
            .Where(e => e.EmailSequenceId == sequenceId && !e.IsDeleted)
            .Include(e => e.Contact)
            .OrderByDescending(e => e.EnrolledAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return enrollments.Select(MapEnrollmentToDto).ToList();
    }

    public async Task<bool> PauseEnrollmentAsync(int sequenceId, int enrollmentId, CancellationToken cancellationToken = default)
    {
        var enrollment = await _context.EmailSequenceEnrollments
            .FirstOrDefaultAsync(e => e.Id == enrollmentId && e.EmailSequenceId == sequenceId && !e.IsDeleted, cancellationToken);

        if (enrollment == null)
            return false;

        enrollment.Status = "Paused";
        enrollment.UpdatedAt = DateTime.UtcNow;
        _context.EmailSequenceEnrollments.Update(enrollment);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> ResumeEnrollmentAsync(int sequenceId, int enrollmentId, CancellationToken cancellationToken = default)
    {
        var enrollment = await _context.EmailSequenceEnrollments
            .FirstOrDefaultAsync(e => e.Id == enrollmentId && e.EmailSequenceId == sequenceId && !e.IsDeleted, cancellationToken);

        if (enrollment == null)
            return false;

        enrollment.Status = "Active";
        enrollment.UpdatedAt = DateTime.UtcNow;
        _context.EmailSequenceEnrollments.Update(enrollment);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> ExitEnrollmentAsync(int sequenceId, int enrollmentId, string? reason = null, CancellationToken cancellationToken = default)
    {
        var enrollment = await _context.EmailSequenceEnrollments
            .FirstOrDefaultAsync(e => e.Id == enrollmentId && e.EmailSequenceId == sequenceId && !e.IsDeleted, cancellationToken);

        if (enrollment == null)
            return false;

        enrollment.IsDeleted = true;
        enrollment.Status = "Exited";
        enrollment.UpdatedAt = DateTime.UtcNow;
        _context.EmailSequenceEnrollments.Update(enrollment);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Enrollment {EnrollmentId} exited. Reason: {Reason}", enrollmentId, reason);
        return true;
    }

    #endregion

    #region Execution & Analytics

    public async Task<EmailSequenceAnalyticsDto> GetAnalyticsAsync(int sequenceId, CancellationToken cancellationToken = default)
    {
        var enrollments = await _context.EmailSequenceEnrollments
            .Where(e => e.EmailSequenceId == sequenceId && !e.IsDeleted)
            .ToListAsync(cancellationToken);

        var analytics = new EmailSequenceAnalyticsDto
        {
            SequenceId = sequenceId,
            TotalEnrolled = enrollments.Count,
            ActiveEnrollments = enrollments.Count(e => e.Status == "Active"),
            PausedEnrollments = enrollments.Count(e => e.Status == "Paused"),
            CompletedEnrollments = enrollments.Count(e => e.Status == "Completed"),
            CalculatedAt = DateTime.UtcNow
        };

        return analytics;
    }

    public async Task<EmailSequenceExecutionResultDto> ExecuteAsync(int sequenceId, CancellationToken cancellationToken = default)
    {
        var sequence = await _context.EmailSequences
            .FirstOrDefaultAsync(e => e.Id == sequenceId && !e.IsDeleted, cancellationToken);

        if (sequence == null)
            throw new InvalidOperationException($"Email sequence {sequenceId} not found");

        sequence.Status = "Active";
        sequence.UpdatedAt = DateTime.UtcNow;
        _context.EmailSequences.Update(sequence);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Email sequence {SequenceId} executed", sequenceId);

        return new EmailSequenceExecutionResultDto
        {
            SequenceId = sequenceId,
            Success = true,
            ExecutedAt = DateTime.UtcNow
        };
    }

    public async Task<int> DuplicateAsync(int sequenceId, string newName, CancellationToken cancellationToken = default)
    {
        var original = await _context.EmailSequences
            .Include(e => e.Steps)
            .FirstOrDefaultAsync(e => e.Id == sequenceId && !e.IsDeleted, cancellationToken);

        if (original == null)
            throw new InvalidOperationException($"Email sequence {sequenceId} not found");

        var copy = new EmailSequence
        {
            Name = newName,
            Description = $"Copy of {original.Name}",
            Status = "Draft",
            SequenceType = original.SequenceType,
            DefaultFromName = original.DefaultFromName,
            DefaultFromEmail = original.DefaultFromEmail,
            DefaultReplyTo = original.DefaultReplyTo,
            OwnerId = original.OwnerId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.EmailSequences.Add(copy);
        await _context.SaveChangesAsync(cancellationToken);

        // Copy steps
        foreach (var step in original.Steps.Where(s => !s.IsDeleted))
        {
            var stepCopy = new EmailSequenceStep
            {
                EmailSequenceId = copy.Id,
                Subject = step.Subject,
                BodyHtml = step.BodyHtml,
                DelayDays = step.DelayDays,
                DelayHours = step.DelayHours,
                StepOrder = step.StepOrder,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.EmailSequenceSteps.Add(stepCopy);
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Email sequence {SequenceId} duplicated as {NewSequenceName}", sequenceId, newName);
        return copy.Id;
    }

    #endregion

    #region Helpers

    private EmailSequenceDto MapToDto(EmailSequence sequence)
    {
        return new EmailSequenceDto
        {
            Id = sequence.Id,
            Name = sequence.Name,
            Description = sequence.Description,
            Status = sequence.Status,
            SequenceType = sequence.SequenceType,
            DefaultFromName = sequence.DefaultFromName,
            DefaultFromEmail = sequence.DefaultFromEmail,
            DefaultReplyTo = sequence.DefaultReplyTo,
            OwnerId = sequence.OwnerId,
            CampaignId = sequence.CampaignId,
            CreatedAt = sequence.CreatedAt,
            UpdatedAt = sequence.UpdatedAt
        };
    }

    private EmailSequenceStepDto MapStepToDto(EmailSequenceStep step)
    {
        return new EmailSequenceStepDto
        {
            Id = step.Id,
            EmailSequenceId = step.EmailSequenceId,
            Subject = step.Subject,
            BodyHtml = step.BodyHtml,
            DelayDays = step.DelayDays,
            DelayHours = step.DelayHours,
            StepOrder = step.StepOrder
        };
    }

    private EmailSequenceEnrollmentDto MapEnrollmentToDto(EmailSequenceEnrollment enrollment)
    {
        return new EmailSequenceEnrollmentDto
        {
            Id = enrollment.Id,
            EmailSequenceId = enrollment.EmailSequenceId,
            ContactId = enrollment.ContactId,
            Status = enrollment.Status,
            EnrolledAt = enrollment.EnrolledAt,
            CreatedAt = enrollment.CreatedAt
        };
    }

    #endregion
}

/// <summary>
/// DTO for email sequence enrollment request.
/// </summary>
public class CreateEmailSequenceEnrollmentDto
{
    public int ContactId { get; set; }
}

/// <summary>
/// DTO for email sequence enrollment response.
/// </summary>
public class EmailSequenceEnrollmentDto
{
    public int Id { get; set; }
    public int EmailSequenceId { get; set; }
    public int ContactId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime EnrolledAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for email sequence analytics response.
/// </summary>
public class EmailSequenceAnalyticsDto
{
    public int SequenceId { get; set; }
    public int TotalEnrolled { get; set; }
    public int ActiveEnrollments { get; set; }
    public int PausedEnrollments { get; set; }
    public int CompletedEnrollments { get; set; }
    public DateTime CalculatedAt { get; set; }
}

/// <summary>
/// DTO for email sequence execution result.
/// </summary>
public class EmailSequenceExecutionResultDto
{
    public int SequenceId { get; set; }
    public bool Success { get; set; }
    public DateTime ExecutedAt { get; set; }
}

/// <summary>
/// DTO for email sequence step request.
/// </summary>
public class CreateEmailSequenceStepDto
{
    public string Subject { get; set; } = string.Empty;
    public string BodyHtml { get; set; } = string.Empty;
    public int? DelayDays { get; set; }
    public int? DelayHours { get; set; }
    public int StepOrder { get; set; }
}

/// <summary>
/// DTO for email sequence step response.
/// </summary>
public class EmailSequenceStepDto
{
    public int Id { get; set; }
    public int EmailSequenceId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string BodyHtml { get; set; } = string.Empty;
    public int DelayDays { get; set; }
    public int DelayHours { get; set; }
    public int StepOrder { get; set; }
}
