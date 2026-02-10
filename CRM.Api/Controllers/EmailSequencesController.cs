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
    [Route("api/[controller]")]
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

        [HttpPost]
        [ProducesResponseType(typeof(EmailSequence), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Create([FromBody] EmailSequence sequence, CancellationToken ct)
        {
            if (sequence == null) return BadRequest("Sequence payload required");
            var created = await _service.CreateSequenceAsync(sequence, ct);
            return CreatedAtAction(nameof(GetStatus), new { id = created.Id }, created);
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
