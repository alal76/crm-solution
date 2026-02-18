// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CRM.Api.Controllers
{
    [ApiController]
    [Route("api/email-sequences")]
    [Authorize]
    public class EmailSequencesController : ControllerBase
    {
        private readonly IEmailSequenceService _service;
        private readonly ILogger<EmailSequencesController> _logger;

        public EmailSequencesController(IEmailSequenceService service, ILogger<EmailSequencesController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// List all email sequences.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<EmailSequenceDto>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            try
            {
                var sequences = await _service.GetAllAsync(ct);
                return Ok(sequences.Select(MapToDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving email sequences");
                return StatusCode(500, new { message = "Error retrieving email sequences", error = ex.Message });
            }
        }

        /// <summary>
        /// Get an email sequence by ID.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(EmailSequenceDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            try
            {
                var sequence = await _service.GetByIdAsync(id, ct);
                if (sequence == null) return NotFound(new { message = $"Email sequence {id} not found" });
                return Ok(MapToDto(sequence));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving email sequence {Id}", id);
                return StatusCode(500, new { message = "Error retrieving email sequence", error = ex.Message });
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(EmailSequenceDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Create([FromBody] EmailSequence sequence, CancellationToken ct)
        {
            if (sequence == null) return BadRequest("Sequence payload required");
            var created = await _service.CreateSequenceAsync(sequence, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToDto(created));
        }

        /// <summary>
        /// Update an existing email sequence.
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(EmailSequenceDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Update(int id, [FromBody] EmailSequence sequence, CancellationToken ct)
        {
            if (sequence == null) return BadRequest("Sequence payload required");
            sequence.Id = id;
            try
            {
                var updated = await _service.UpdateAsync(sequence, ct);
                return Ok(MapToDto(updated));
            }
            catch (InvalidOperationException)
            {
                return NotFound(new { message = $"Email sequence {id} not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating email sequence {Id}", id);
                return StatusCode(500, new { message = "Error updating email sequence", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete an email sequence (soft delete).
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            try
            {
                var deleted = await _service.DeleteAsync(id, ct);
                if (!deleted) return NotFound(new { message = $"Email sequence {id} not found" });
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting email sequence {Id}", id);
                return StatusCode(500, new { message = "Error deleting email sequence", error = ex.Message });
            }
        }

        [HttpPost("{id}/enroll")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Enroll(int id, [FromQuery] int contactId, [FromQuery] int? enrolledById, CancellationToken ct)
        {
            if (contactId <= 0) return BadRequest("contactId is required");
            var enrollment = await _service.EnrollContactAsync(id, contactId, enrolledById, ct);
            return Ok(enrollment);
        }

        [HttpPost("{id}/start")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Start(int id, CancellationToken ct)
        {
            var ok = await _service.StartSequenceAsync(id, ct);
            if (!ok) return NotFound();
            return NoContent();
        }

        [HttpPost("{id}/stop")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Stop(int id, CancellationToken ct)
        {
            var ok = await _service.StopSequenceAsync(id, ct);
            if (!ok) return NotFound();
            return NoContent();
        }

        [HttpGet("{id}/status")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetStatus(int id, CancellationToken ct)
        {
            var status = await _service.GetSequenceStatusAsync(id, ct);
            return Ok(status);
        }

        #region DTO Mapping

        private static EmailSequenceDto MapToDto(EmailSequence entity)
        {
            return new EmailSequenceDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                Status = entity.Status.ToString(),
                SequenceType = entity.SendFromOwner ? "Owner" : "Shared",
                TotalEnrolled = entity.TotalEnrolled,
                TotalCompleted = entity.TotalCompleted,
                TotalActive = entity.ActiveEnrollments,
                OpenRate = entity.TotalEmailsSent > 0 ? (decimal)entity.TotalOpens / entity.TotalEmailsSent * 100 : 0,
                ClickRate = entity.TotalEmailsSent > 0 ? (decimal)entity.TotalClicks / entity.TotalEmailsSent * 100 : 0,
                ReplyRate = entity.TotalEmailsSent > 0 ? (decimal)entity.TotalReplies / entity.TotalEmailsSent * 100 : 0,
                ConversionRate = entity.TotalEnrolled > 0 ? (decimal)entity.TotalCompleted / entity.TotalEnrolled * 100 : 0,
                DefaultFromName = entity.FromName,
                DefaultFromEmail = entity.FromEmail,
                DefaultReplyTo = entity.ReplyToEmail,
                OwnerId = entity.OwnerId,
                CampaignId = null,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                Steps = entity.Steps?.Select(MapStepToDto).OrderBy(s => s.StepNumber).ToList() ?? new List<EmailSequenceStepDto>()
            };
        }

        private static EmailSequenceStepDto MapStepToDto(EmailSequenceStep step)
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
                TemplateId = step.EmailTemplateId,
                DelayDays = step.DelayDays,
                DelayHours = step.DelayHours,
                DelayMinutes = step.DelayMinutes,
                TimingMode = step.TimingMode.ToString(),
                SpecificTime = step.SpecificTime != null ? TimeSpan.TryParse(step.SpecificTime, out var ts) ? ts : null : null,
                SendOnWeekends = false,
                IsABTest = step.IsABTest,
                ABVariant = step.ABVariant,
                ABTestPercentage = step.ABSplitPercent ?? 50,
                TotalSent = step.EmailsSent,
                TotalOpened = step.Opens,
                TotalClicked = step.Clicks,
                TotalReplied = step.Replies,
                IsActive = step.IsActive,
                CreatedAt = step.CreatedAt,
                UpdatedAt = step.UpdatedAt
            };
        }

        #endregion
    }
}
