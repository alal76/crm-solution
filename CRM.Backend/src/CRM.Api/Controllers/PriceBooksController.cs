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
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CRM.Core.Entities;
using CRM.Core.Interfaces;

namespace CRM.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PriceBooksController : ControllerBase
    {
        private readonly IPricingService _pricingService;
        private readonly ILogger<PriceBooksController> _logger;

        public PriceBooksController(IPricingService pricingService, ILogger<PriceBooksController> logger)
        {
            _pricingService = pricingService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var items = await _pricingService.GetAllPriceBooksAsync(cancellationToken);
            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            var item = await _pricingService.GetPriceBookByIdAsync(id, cancellationToken);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PriceBook model, CancellationToken cancellationToken)
        {
            var created = await _pricingService.CreatePriceBookAsync(model, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] PriceBook model, CancellationToken cancellationToken)
        {
            if (id != model.Id) return BadRequest();
            var updated = await _pricingService.UpdatePriceBookAsync(model, cancellationToken);
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var ok = await _pricingService.DeletePriceBookAsync(id, cancellationToken);
            if (!ok) return NotFound();
            return NoContent();
        }

        private IActionResult HandleServiceException(Exception ex)
        {
            _logger.LogError(ex, "PriceBooksController error");
            return Problem(detail: ex.Message);
        }
    }
}
