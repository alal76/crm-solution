// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

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
            Status = EmailSequenceStatus.Draft,
            FromName = dto.DefaultFromName,
            FromEmail = dto.DefaultFromEmail,
            ReplyToEmail = dto.DefaultReplyTo,
            OwnerId = dto.OwnerId,
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
            sequence.FromName = dto.DefaultFromName;

        if (dto.DefaultFromEmail != null)
            sequence.FromEmail = dto.DefaultFromEmail;

        if (dto.DefaultReplyTo != null)
            sequence.ReplyToEmail = dto.DefaultReplyTo;

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

    public async Task<CRM.Core.Dtos.EmailSequenceStepDto> AddStepAsync(int sequenceId, CRM.Core.Dtos.CreateEmailSequenceStepDto dto, CancellationToken cancellationToken = default)
    {
        var sequence = await _context.EmailSequences
            .FirstOrDefaultAsync(e => e.Id == sequenceId && !e.IsDeleted, cancellationToken);

        if (sequence == null)
            throw new InvalidOperationException($"Email sequence {sequenceId} not found");

        var step = new EmailSequenceStep
        {
            EmailSequenceId = sequenceId,
            Name = dto.Name,
            StepType = EmailStepType.Email,
            Subject = dto.Subject,
            Body = dto.HtmlContent,
            BodyPlainText = dto.TextContent,
            DelayDays = dto.DelayDays,
            DelayHours = dto.DelayHours,
            DelayMinutes = dto.DelayMinutes,
            StepOrder = 1,
            TimingMode = StepTimingMode.Delay,
            SpecificTime = dto.SpecificTime?.ToString(@"hh\:mm"),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.EmailSequenceSteps.Add(step);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Step added to sequence {SequenceId}", sequenceId);
        return await Task.FromResult(MapStepToDto(step));
    }

    public async Task<CRM.Core.Dtos.EmailSequenceStepDto> UpdateStepAsync(int sequenceId, int stepId, CRM.Core.Dtos.CreateEmailSequenceStepDto dto, CancellationToken cancellationToken = default)
    {
        var step = await _context.EmailSequenceSteps
            .FirstOrDefaultAsync(s => s.Id == stepId && s.EmailSequenceId == sequenceId && !s.IsDeleted, cancellationToken);

        if (step == null)
            throw new InvalidOperationException($"Step {stepId} not found in sequence {sequenceId}");

        step.Name = dto.Name;
        step.Subject = dto.Subject;
        step.Body = dto.HtmlContent;
        step.BodyPlainText = dto.TextContent;
        step.DelayDays = dto.DelayDays;
        step.DelayHours = dto.DelayHours;
        step.DelayMinutes = dto.DelayMinutes;
        step.SpecificTime = dto.SpecificTime?.ToString(@"hh\:mm");
        step.UpdatedAt = DateTime.UtcNow;

        _context.EmailSequenceSteps.Update(step);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Step {StepId} updated", stepId);
        return await Task.FromResult(MapStepToDto(step));
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

    public async Task<CRM.Core.Dtos.EmailSequenceEnrollmentDto> EnrollAsync(int sequenceId, CRM.Core.Dtos.CreateEmailSequenceEnrollmentDto dto, CancellationToken cancellationToken = default)
    {
        var sequence = await _context.EmailSequences
            .FirstOrDefaultAsync(e => e.Id == sequenceId && !e.IsDeleted, cancellationToken);

        if (sequence == null)
            throw new InvalidOperationException($"Email sequence {sequenceId} not found");

        var contact = await _context.Contacts
            .FirstOrDefaultAsync(c => c.Id == dto.ContactId, cancellationToken);

        if (contact == null)
            throw new InvalidOperationException($"Contact {dto.ContactId} not found");

        var enrollment = new EmailSequenceEnrollment
        {
            EmailSequenceId = sequenceId,
            ContactId = dto.ContactId,
            Status = EnrollmentStatus.Active,
            EnrolledAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.EmailSequenceEnrollments.Add(enrollment);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Contact {ContactId} enrolled in sequence {SequenceId}", dto.ContactId, sequenceId);
        return await Task.FromResult(MapEnrollmentToDto(enrollment));
    }

    public async Task<System.Collections.Generic.List<CRM.Core.Dtos.EmailSequenceEnrollmentDto>> GetEnrollmentsAsync(int sequenceId, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var enrollments = await _context.EmailSequenceEnrollments
            .Where(e => e.EmailSequenceId == sequenceId && !e.IsDeleted)
            .Include(e => e.Contact)
            .OrderByDescending(e => e.EnrolledAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return await Task.FromResult(enrollments.Select(MapEnrollmentToDto).ToList());
    }

    public async Task<bool> PauseEnrollmentAsync(int sequenceId, int enrollmentId, CancellationToken cancellationToken = default)
    {
        var enrollment = await _context.EmailSequenceEnrollments
            .FirstOrDefaultAsync(e => e.Id == enrollmentId && e.EmailSequenceId == sequenceId && !e.IsDeleted, cancellationToken);

        if (enrollment == null)
            return false;

        enrollment.Status = EnrollmentStatus.Paused;
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

        enrollment.Status = EnrollmentStatus.Active;
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
        enrollment.Status = EnrollmentStatus.Removed;
        enrollment.UpdatedAt = DateTime.UtcNow;
        _context.EmailSequenceEnrollments.Update(enrollment);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Enrollment {EnrollmentId} exited. Reason: {Reason}", enrollmentId, reason);
        return true;
    }

    #endregion

    #region Execution & Analytics

    public async Task<CRM.Core.Dtos.EmailSequenceAnalyticsDto> GetAnalyticsAsync(int sequenceId, CancellationToken cancellationToken = default)
    {
        var enrollments = await _context.EmailSequenceEnrollments
            .Where(e => e.EmailSequenceId == sequenceId && !e.IsDeleted)
            .ToListAsync(cancellationToken);

        var analytics = new CRM.Core.Dtos.EmailSequenceAnalyticsDto
        {
            SequenceId = sequenceId,
            TotalEnrolled = enrollments.Count,
            TotalActive = enrollments.Count(e => e.Status == EnrollmentStatus.Active),
            TotalCompleted = enrollments.Count(e => e.Status == EnrollmentStatus.Completed)
        };

        return await Task.FromResult(analytics);
    }

    public async Task<CRM.Core.Dtos.EmailSequenceExecutionResultDto> ExecuteAsync(int sequenceId, CancellationToken cancellationToken = default)
    {
        var sequence = await _context.EmailSequences
            .FirstOrDefaultAsync(e => e.Id == sequenceId && !e.IsDeleted, cancellationToken);

        if (sequence == null)
            throw new InvalidOperationException($"Email sequence {sequenceId} not found");

        sequence.Status = EmailSequenceStatus.Active;
        sequence.UpdatedAt = DateTime.UtcNow;
        _context.EmailSequences.Update(sequence);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Email sequence {SequenceId} executed", sequenceId);

        var result = new CRM.Core.Dtos.EmailSequenceExecutionResultDto
        {
            SequenceId = sequenceId,
            EmailsSent = 0,
            EnrollmentsProcessed = 0,
            Errors = 0,
            Status = "Active",
            ExecutedAt = DateTime.UtcNow,
            Duration = TimeSpan.Zero
        };
        return await Task.FromResult(result);
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
            Status = EmailSequenceStatus.Draft,
            FromName = original.FromName,
            FromEmail = original.FromEmail,
            ReplyToEmail = original.ReplyToEmail,
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
                Name = step.Name,
                StepType = step.StepType,
                Subject = step.Subject,
                Body = step.Body,
                BodyPlainText = step.BodyPlainText,
                DelayDays = step.DelayDays,
                DelayHours = step.DelayHours,
                DelayMinutes = step.DelayMinutes,
                StepOrder = step.StepOrder,
                SpecificTime = step.SpecificTime,
                TimingMode = step.TimingMode,
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
            Status = sequence.Status.ToString(),
            DefaultFromName = sequence.FromName,
            DefaultFromEmail = sequence.FromEmail,
            DefaultReplyTo = sequence.ReplyToEmail,
            OwnerId = sequence.OwnerId,
            TotalEnrolled = sequence.TotalEnrolled,
            TotalCompleted = sequence.TotalCompleted,
            TotalActive = sequence.ActiveEnrollments,
            OpenRate = sequence.TotalEmailsSent > 0 ? (decimal)sequence.TotalOpens / sequence.TotalEmailsSent : 0,
            ClickRate = sequence.TotalEmailsSent > 0 ? (decimal)sequence.TotalClicks / sequence.TotalEmailsSent : 0,
            ReplyRate = sequence.TotalEmailsSent > 0 ? (decimal)sequence.TotalReplies / sequence.TotalEmailsSent : 0,
            ConversionRate = sequence.TotalEnrolled > 0 ? (decimal)sequence.TotalCompleted / sequence.TotalEnrolled : 0,
            CreatedAt = sequence.CreatedAt,
            UpdatedAt = sequence.UpdatedAt
        };
    }

    private EmailSequenceStepDto MapStepToDto(EmailSequenceStep step)
    {
        return new EmailSequenceStepDto
        {
            Id = step.Id,
            SequenceId = step.EmailSequenceId,
            StepNumber = step.StepOrder,
            StepType = step.StepType.ToString(),
            Name = step.Name,
            Subject = step.Subject,
            HtmlContent = step.Body,
            TextContent = step.BodyPlainText,
            DelayDays = step.DelayDays,
            DelayHours = step.DelayHours,
            DelayMinutes = step.DelayMinutes,
            TimingMode = step.TimingMode.ToString(),
            SpecificTime = !string.IsNullOrEmpty(step.SpecificTime) ? TimeSpan.Parse(step.SpecificTime) : null,
            SendOnWeekends = true,
            IsABTest = false,
            ABTestPercentage = 50,
            IsActive = step.IsActive,
            CreatedAt = step.CreatedAt,
            UpdatedAt = step.UpdatedAt
        };
    }

    private EmailSequenceEnrollmentDto MapEnrollmentToDto(EmailSequenceEnrollment enrollment)
    {
        return new EmailSequenceEnrollmentDto
        {
            Id = enrollment.Id,
            SequenceId = enrollment.EmailSequenceId,
            ContactId = enrollment.ContactId,
            Status = enrollment.Status.ToString(),
            Email = enrollment.RecipientEmail,
            EnrolledAt = enrollment.EnrolledAt,
            CurrentStepNumber = enrollment.CurrentStepIndex,
            TotalEmailsSent = enrollment.EmailsSent,
            TotalEmailsOpened = enrollment.TotalOpens,
            TotalLinksClicked = enrollment.TotalClicks
        };
    }

    #endregion
}

