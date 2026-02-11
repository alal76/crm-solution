using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
        [ProducesResponseType(typeof(IEnumerable<EmailSequence>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            try
            {
                var sequences = await _service.GetAllAsync(ct);
                return Ok(sequences);
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
        [ProducesResponseType(typeof(EmailSequence), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            try
            {
                var sequence = await _service.GetByIdAsync(id, ct);
                if (sequence == null) return NotFound(new { message = $"Email sequence {id} not found" });
                return Ok(sequence);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving email sequence {Id}", id);
                return StatusCode(500, new { message = "Error retrieving email sequence", error = ex.Message });
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(EmailSequence), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Create([FromBody] EmailSequence sequence, CancellationToken ct)
        {
            if (sequence == null) return BadRequest("Sequence payload required");
            var created = await _service.CreateSequenceAsync(sequence, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>
        /// Update an existing email sequence.
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(EmailSequence), 200)]
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
                return Ok(updated);
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
    }
}
