// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using CRM.Core.Dtos;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CRM.Api.Controllers
{
    /// <summary>
    /// Admin Configuration Controller
    /// Manages sales and service desk configurations:
    /// - Commission Rules
    /// - Discount Rules
    /// - SLA Policies
    /// - Escalation Rules
    /// - Service Queues
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminConfigurationController : ControllerBase
    {
        private readonly IAdminConfigurationService _adminConfigService;
        private readonly ILogger<AdminConfigurationController> _logger;

        public AdminConfigurationController(
            IAdminConfigurationService adminConfigService,
            ILogger<AdminConfigurationController> logger)
        {
            _adminConfigService = adminConfigService ?? throw new ArgumentNullException(nameof(adminConfigService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Commission Rules

        /// <summary>
        /// Get all commission rules with pagination
        /// </summary>
        [HttpGet("commission-rules")]
        [ProducesResponseType(typeof(IEnumerable<CommissionRuleDto>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetCommissionRules(
            [FromQuery] int? page = 1,
            [FromQuery] int? pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation($"Fetching commission rules - Page: {page}, PageSize: {pageSize}");

                var rules = await _adminConfigService.GetCommissionRulesAsync(cancellationToken);
                return Ok(rules);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching commission rules: {ex.Message}");
                return StatusCode(500, new { message = "Error fetching commission rules", error = ex.Message });
            }
        }

        /// <summary>
        /// Get commission rule by ID
        /// </summary>
        [HttpGet("commission-rules/{id}")]
        [ProducesResponseType(typeof(CommissionRuleDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetCommissionRuleById(
            int id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation($"Fetching commission rule: {id}");

                var rule = await _adminConfigService.GetCommissionRuleByIdAsync(id, cancellationToken: cancellationToken);
                if (rule == null)
                    return NotFound(new { message = $"Commission rule with ID {id} not found" });

                return Ok(rule);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching commission rule {id}: {ex.Message}");
                return StatusCode(500, new { message = "Error fetching commission rule", error = ex.Message });
            }
        }

        /// <summary>
        /// Create new commission rule
        /// </summary>
        [HttpPost("commission-rules")]
        [ProducesResponseType(typeof(CommissionRuleDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CreateCommissionRule(
            [FromBody] CreateCommissionRuleDto request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (request == null)
                    return BadRequest(new { message = "Request body is required" });

                _logger.LogInformation($"Creating commission rule: {request.Name}");

                var rule = await _adminConfigService.CreateCommissionRuleAsync(request, cancellationToken: cancellationToken);
                return CreatedAtAction(nameof(GetCommissionRuleById), new { id = rule.Id }, rule);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Validation error creating commission rule: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating commission rule: {ex.Message}");
                return StatusCode(500, new { message = "Error creating commission rule", error = ex.Message });
            }
        }

        /// <summary>
        /// Update commission rule
        /// </summary>
        [HttpPut("commission-rules/{id}")]
        [ProducesResponseType(typeof(CommissionRuleDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> UpdateCommissionRule(
            int id,
            [FromBody] UpdateCommissionRuleDto request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation($"Updating commission rule: {id}");

                var rule = await _adminConfigService.UpdateCommissionRuleAsync(id, request, cancellationToken: cancellationToken);
                if (rule == null)
                    return NotFound(new { message = $"Commission rule with ID {id} not found" });

                return Ok(rule);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating commission rule {id}: {ex.Message}");
                return StatusCode(500, new { message = "Error updating commission rule", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete commission rule
        /// </summary>
        [HttpDelete("commission-rules/{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DeleteCommissionRule(
            int id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation($"Deleting commission rule: {id}");

                var success = await _adminConfigService.DeleteCommissionRuleAsync(id, cancellationToken: cancellationToken);
                if (!success)
                    return NotFound(new { message = $"Commission rule with ID {id} not found" });

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting commission rule {id}: {ex.Message}");
                return StatusCode(500, new { message = "Error deleting commission rule", error = ex.Message });
            }
        }

        #endregion

        #region Discount Rules

        /// <summary>
        /// Get all discount rules
        /// </summary>
        [HttpGet("discount-rules")]
        [ProducesResponseType(typeof(IEnumerable<DiscountRuleDto>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetDiscountRules(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Fetching discount rules");

                var rules = await _adminConfigService.GetDiscountRulesAsync(cancellationToken);
                return Ok(rules);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching discount rules: {ex.Message}");
                return StatusCode(500, new { message = "Error fetching discount rules", error = ex.Message });
            }
        }

        /// <summary>
        /// Get discount rule by ID
        /// </summary>
        [HttpGet("discount-rules/{id}")]
        [ProducesResponseType(typeof(DiscountRuleDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetDiscountRuleById(
            int id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation($"Fetching discount rule: {id}");

                var rule = await _adminConfigService.GetDiscountRuleByIdAsync(id, cancellationToken: cancellationToken);
                if (rule == null)
                    return NotFound(new { message = $"Discount rule with ID {id} not found" });

                return Ok(rule);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching discount rule {id}: {ex.Message}");
                return StatusCode(500, new { message = "Error fetching discount rule", error = ex.Message });
            }
        }

        /// <summary>
        /// Create new discount rule
        /// </summary>
        [HttpPost("discount-rules")]
        [ProducesResponseType(typeof(DiscountRuleDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CreateDiscountRule(
            [FromBody] CreateDiscountRuleDto request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (request == null)
                    return BadRequest(new { message = "Request body is required" });

                _logger.LogInformation($"Creating discount rule: {request.Name}");

                var rule = await _adminConfigService.CreateDiscountRuleAsync(request, cancellationToken: cancellationToken);
                return CreatedAtAction(nameof(GetDiscountRuleById), new { id = rule.Id }, rule);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Validation error creating discount rule: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating discount rule: {ex.Message}");
                return StatusCode(500, new { message = "Error creating discount rule", error = ex.Message });
            }
        }

        /// <summary>
        /// Update discount rule
        /// </summary>
        [HttpPut("discount-rules/{id}")]
        [ProducesResponseType(typeof(DiscountRuleDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> UpdateDiscountRule(
            int id,
            [FromBody] UpdateDiscountRuleDto request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation($"Updating discount rule: {id}");

                var rule = await _adminConfigService.UpdateDiscountRuleAsync(id, request, cancellationToken: cancellationToken);
                if (rule == null)
                    return NotFound(new { message = $"Discount rule with ID {id} not found" });

                return Ok(rule);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating discount rule {id}: {ex.Message}");
                return StatusCode(500, new { message = "Error updating discount rule", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete discount rule
        /// </summary>
        [HttpDelete("discount-rules/{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DeleteDiscountRule(
            int id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation($"Deleting discount rule: {id}");

                var success = await _adminConfigService.DeleteDiscountRuleAsync(id, cancellationToken: cancellationToken);
                if (!success)
                    return NotFound(new { message = $"Discount rule with ID {id} not found" });

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting discount rule {id}: {ex.Message}");
                return StatusCode(500, new { message = "Error deleting discount rule", error = ex.Message });
            }
        }

        #endregion

        #region SLA Policies

        /// <summary>
        /// Get all SLA policies
        /// </summary>
        [HttpGet("sla-policies")]
        [ProducesResponseType(typeof(IEnumerable<SLAPolicyDto>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetSLAPolicies(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Fetching SLA policies");

                var policies = await _adminConfigService.GetSLAPoliciesAsync(cancellationToken);
                return Ok(policies);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching SLA policies: {ex.Message}");
                return StatusCode(500, new { message = "Error fetching SLA policies", error = ex.Message });
            }
        }

        /// <summary>
        /// Get SLA policy by ID
        /// </summary>
        [HttpGet("sla-policies/{id}")]
        [ProducesResponseType(typeof(SLAPolicyDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetSLAPolicyById(
            int id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation($"Fetching SLA policy: {id}");

                var policy = await _adminConfigService.GetSLAPolicyByIdAsync(id, cancellationToken: cancellationToken);
                if (policy == null)
                    return NotFound(new { message = $"SLA policy with ID {id} not found" });

                return Ok(policy);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching SLA policy {id}: {ex.Message}");
                return StatusCode(500, new { message = "Error fetching SLA policy", error = ex.Message });
            }
        }

        /// <summary>
        /// Create new SLA policy
        /// </summary>
        [HttpPost("sla-policies")]
        [ProducesResponseType(typeof(SLAPolicyDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CreateSLAPolicy(
            [FromBody] CreateSLAPolicyDto request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (request == null)
                    return BadRequest(new { message = "Request body is required" });

                _logger.LogInformation($"Creating SLA policy: {request.Name}");

                var policy = await _adminConfigService.CreateSLAPolicyAsync(request, cancellationToken: cancellationToken);
                return CreatedAtAction(nameof(GetSLAPolicyById), new { id = policy.Id }, policy);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Validation error creating SLA policy: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating SLA policy: {ex.Message}");
                return StatusCode(500, new { message = "Error creating SLA policy", error = ex.Message });
            }
        }

        /// <summary>
        /// Update SLA policy
        /// </summary>
        [HttpPut("sla-policies/{id}")]
        [ProducesResponseType(typeof(SLAPolicyDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> UpdateSLAPolicy(
            int id,
            [FromBody] UpdateSLAPolicyDto request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation($"Updating SLA policy: {id}");

                var policy = await _adminConfigService.UpdateSLAPolicyAsync(id, request, cancellationToken: cancellationToken);
                if (policy == null)
                    return NotFound(new { message = $"SLA policy with ID {id} not found" });

                return Ok(policy);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating SLA policy {id}: {ex.Message}");
                return StatusCode(500, new { message = "Error updating SLA policy", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete SLA policy
        /// </summary>
        [HttpDelete("sla-policies/{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DeleteSLAPolicy(
            int id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation($"Deleting SLA policy: {id}");

                var success = await _adminConfigService.DeleteSLAPolicyAsync(id, cancellationToken: cancellationToken);
                if (!success)
                    return NotFound(new { message = $"SLA policy with ID {id} not found" });

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting SLA policy {id}: {ex.Message}");
                return StatusCode(500, new { message = "Error deleting SLA policy", error = ex.Message });
            }
        }

        #endregion

        #region Escalation Rules

        /// <summary>
        /// Get all escalation rules
        /// </summary>
        [HttpGet("escalation-rules")]
        [ProducesResponseType(typeof(IEnumerable<EscalationRuleDto>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetEscalationRules(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Fetching escalation rules");

                var rules = await _adminConfigService.GetEscalationRulesAsync(cancellationToken);
                return Ok(rules);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching escalation rules: {ex.Message}");
                return StatusCode(500, new { message = "Error fetching escalation rules", error = ex.Message });
            }
        }

        /// <summary>
        /// Get escalation rule by ID
        /// </summary>
        [HttpGet("escalation-rules/{id}")]
        [ProducesResponseType(typeof(EscalationRuleDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetEscalationRuleById(
            int id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation($"Fetching escalation rule: {id}");

                var rule = await _adminConfigService.GetEscalationRuleByIdAsync(id, cancellationToken: cancellationToken);
                if (rule == null)
                    return NotFound(new { message = $"Escalation rule with ID {id} not found" });

                return Ok(rule);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching escalation rule {id}: {ex.Message}");
                return StatusCode(500, new { message = "Error fetching escalation rule", error = ex.Message });
            }
        }

        /// <summary>
        /// Create new escalation rule
        /// </summary>
        [HttpPost("escalation-rules")]
        [ProducesResponseType(typeof(EscalationRuleDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CreateEscalationRule(
            [FromBody] CreateEscalationRuleDto request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (request == null)
                    return BadRequest(new { message = "Request body is required" });

                _logger.LogInformation($"Creating escalation rule: {request.Name}");

                var rule = await _adminConfigService.CreateEscalationRuleAsync(request, cancellationToken: cancellationToken);
                return CreatedAtAction(nameof(GetEscalationRuleById), new { id = rule.Id }, rule);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Validation error creating escalation rule: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating escalation rule: {ex.Message}");
                return StatusCode(500, new { message = "Error creating escalation rule", error = ex.Message });
            }
        }

        /// <summary>
        /// Update escalation rule
        /// </summary>
        [HttpPut("escalation-rules/{id}")]
        [ProducesResponseType(typeof(EscalationRuleDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> UpdateEscalationRule(
            int id,
            [FromBody] UpdateEscalationRuleDto request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation($"Updating escalation rule: {id}");

                var rule = await _adminConfigService.UpdateEscalationRuleAsync(id, request, cancellationToken: cancellationToken);
                if (rule == null)
                    return NotFound(new { message = $"Escalation rule with ID {id} not found" });

                return Ok(rule);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating escalation rule {id}: {ex.Message}");
                return StatusCode(500, new { message = "Error updating escalation rule", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete escalation rule
        /// </summary>
        [HttpDelete("escalation-rules/{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DeleteEscalationRule(
            int id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation($"Deleting escalation rule: {id}");

                var success = await _adminConfigService.DeleteEscalationRuleAsync(id, cancellationToken: cancellationToken);
                if (!success)
                    return NotFound(new { message = $"Escalation rule with ID {id} not found" });

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting escalation rule {id}: {ex.Message}");
                return StatusCode(500, new { message = "Error deleting escalation rule", error = ex.Message });
            }
        }

        #endregion

        #region Service Queues

        /// <summary>
        /// Get all service queues
        /// </summary>
        [HttpGet("service-queues")]
        [ProducesResponseType(typeof(IEnumerable<ServiceQueueDto>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetServiceQueues(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Fetching service queues");

                var queues = await _adminConfigService.GetServiceQueuesAsync(cancellationToken);
                return Ok(queues);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching service queues: {ex.Message}");
                return StatusCode(500, new { message = "Error fetching service queues", error = ex.Message });
            }
        }

        /// <summary>
        /// Get service queue by ID
        /// </summary>
        [HttpGet("service-queues/{id}")]
        [ProducesResponseType(typeof(ServiceQueueDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetServiceQueueById(
            int id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation($"Fetching service queue: {id}");

                var queue = await _adminConfigService.GetServiceQueueByIdAsync(id, cancellationToken: cancellationToken);
                if (queue == null)
                    return NotFound(new { message = $"Service queue with ID {id} not found" });

                return Ok(queue);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching service queue {id}: {ex.Message}");
                return StatusCode(500, new { message = "Error fetching service queue", error = ex.Message });
            }
        }

        /// <summary>
        /// Create new service queue
        /// </summary>
        [HttpPost("service-queues")]
        [ProducesResponseType(typeof(ServiceQueueDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CreateServiceQueue(
            [FromBody] CreateServiceQueueDto request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (request == null)
                    return BadRequest(new { message = "Request body is required" });

                _logger.LogInformation($"Creating service queue: {request.Name}");

                var queue = await _adminConfigService.CreateServiceQueueAsync(request, cancellationToken: cancellationToken);
                return CreatedAtAction(nameof(GetServiceQueueById), new { id = queue.Id }, queue);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Validation error creating service queue: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating service queue: {ex.Message}");
                return StatusCode(500, new { message = "Error creating service queue", error = ex.Message });
            }
        }

        /// <summary>
        /// Update service queue
        /// </summary>
        [HttpPut("service-queues/{id}")]
        [ProducesResponseType(typeof(ServiceQueueDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> UpdateServiceQueue(
            int id,
            [FromBody] UpdateServiceQueueDto request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation($"Updating service queue: {id}");

                var queue = await _adminConfigService.UpdateServiceQueueAsync(id, request, cancellationToken: cancellationToken);
                if (queue == null)
                    return NotFound(new { message = $"Service queue with ID {id} not found" });

                return Ok(queue);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating service queue {id}: {ex.Message}");
                return StatusCode(500, new { message = "Error updating service queue", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete service queue
        /// </summary>
        [HttpDelete("service-queues/{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DeleteServiceQueue(
            int id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation($"Deleting service queue: {id}");

                var success = await _adminConfigService.DeleteServiceQueueAsync(id, cancellationToken: cancellationToken);
                if (!success)
                    return NotFound(new { message = $"Service queue with ID {id} not found" });

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting service queue {id}: {ex.Message}");
                return StatusCode(500, new { message = "Error deleting service queue", error = ex.Message });
            }
        }

        #endregion

        #region Configuration Overview

        /// <summary>
        /// Get complete admin configuration overview
        /// </summary>
        [HttpGet("overview")]
        [ProducesResponseType(typeof(AdminConfigurationDto), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetConfigurationOverview(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Fetching admin configuration overview");

                var config = await _adminConfigService.GetConfigurationAsync(cancellationToken);
                return Ok(config);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching configuration overview: {ex.Message}");
                return StatusCode(500, new { message = "Error fetching configuration overview", error = ex.Message });
            }
        }

        /// <summary>
        /// Get sales configuration overview
        /// </summary>
        [HttpGet("sales")]
        [ProducesResponseType(typeof(SalesAdminConfigDto), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetSalesConfig(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Fetching sales admin configuration");

                var config = await _adminConfigService.GetSalesConfigAsync(cancellationToken);
                return Ok(config);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching sales configuration: {ex.Message}");
                return StatusCode(500, new { message = "Error fetching sales configuration", error = ex.Message });
            }
        }

        /// <summary>
        /// Get service desk configuration overview
        /// </summary>
        [HttpGet("service-desk")]
        [ProducesResponseType(typeof(ServiceDeskAdminConfigDto), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetServiceDeskConfig(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Fetching service desk admin configuration");

                var config = await _adminConfigService.GetServiceDeskConfigAsync(cancellationToken);
                return Ok(config);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching service desk configuration: {ex.Message}");
                return StatusCode(500, new { message = "Error fetching service desk configuration", error = ex.Message });
            }
        }

        #endregion
    }
}
