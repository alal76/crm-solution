// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Threading;
using System.Threading.Tasks;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductBundlesController : CrmControllerBase
    {
        private readonly IProductBundleService _bundleService;
        private readonly ILogger<ProductBundlesController> _logger;

        public ProductBundlesController(IProductBundleService bundleService, ILogger<ProductBundlesController> logger)
        {
            _bundleService = bundleService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var items = await _bundleService.GetAllBundlesAsync(cancellationToken);
            return Ok(items);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            var item = await _bundleService.GetBundleByIdAsync(id, cancellationToken);
            if (item == null)
            {
                return NotFound();
            }
            return Ok(item);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] ProductBundle model, CancellationToken cancellationToken)
        {
            var created = await _bundleService.CreateBundleAsync(model, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(int id, [FromBody] ProductBundle model, CancellationToken cancellationToken)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }
            var updated = await _bundleService.UpdateBundleAsync(model, cancellationToken);
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var ok = await _bundleService.DeleteBundleAsync(id, cancellationToken);
            if (!ok)
            {
                return NotFound();
            }
            return NoContent();
        }

        [HttpGet("{id}/price")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPrice(int id, CancellationToken cancellationToken)
        {
            var price = await _bundleService.CalculateBundlePriceAsync(id, cancellationToken);
            return Ok(new { bundleId = id, price });
        }

        private IActionResult HandleServiceException(Exception ex)
        {
            _logger.LogError(ex, "ProductBundlesController error");
            return Problem(detail: ex.Message);
        }
    }
}
