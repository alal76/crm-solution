// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using CRM.Core.Dtos;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CRM.Api.Controllers
{
    /// <summary>
    /// API controller for navigation configuration.
    /// Provides navigation structure with pluggable architecture awareness and RBAC support.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Tags("Navigation")]
    public class NavigationController : ControllerBase
    {
        private readonly INavigationConfigService _navigationConfigService;
        private readonly ILogger<NavigationController> _logger;

        public NavigationController(
            INavigationConfigService navigationConfigService,
            ILogger<NavigationController> logger)
        {
            _navigationConfigService = navigationConfigService;
            _logger = logger;
        }

        /// <summary>
        /// Gets the complete navigation configuration for the current deployment.
        /// This endpoint is used by the frontend at startup to configure navigation
        /// based on enabled features and pluggable providers.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Complete navigation configuration</returns>
        [HttpGet("config")]
        [AllowAnonymous] // Allow anonymous so frontend can load nav before auth
        [ProducesResponseType(typeof(NavigationConfig), 200)]
        public async Task<IActionResult> GetNavigationConfig(CancellationToken cancellationToken)
        {
            _logger.LogDebug("Getting navigation configuration");
            var config = await _navigationConfigService.GetNavigationConfigAsync(cancellationToken);
            return Ok(config);
        }

        /// <summary>
        /// Gets navigation configuration filtered by the current user's RBAC permissions.
        /// Returns only nav items the user has access to based on their group memberships.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>User-filtered navigation configuration</returns>
        [HttpGet("config/user")]
        [Authorize]
        [ProducesResponseType(typeof(NavigationConfig), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetUserNavigationConfig(CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized("Unable to determine user identity");
            }

            _logger.LogDebug("Getting user-filtered navigation configuration for user {UserId}", userId.Value);
            var config = await _navigationConfigService.GetNavigationConfigForUserAsync(userId.Value, cancellationToken);
            return Ok(config);
        }

        /// <summary>
        /// Gets the current user's navigation permissions from their group memberships.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>User navigation permissions</returns>
        [HttpGet("permissions")]
        [Authorize]
        [ProducesResponseType(typeof(UserNavigationPermissions), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetUserPermissions(CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized("Unable to determine user identity");
            }

            var permissions = await _navigationConfigService.GetUserPermissionsAsync(userId.Value, cancellationToken);
            return Ok(permissions);
        }

        /// <summary>
        /// Gets module field configurations for a specific module.
        /// </summary>
        /// <param name="moduleName">Name of the module (e.g., "Customers", "Leads")</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Module field configuration</returns>
        [HttpGet("modules/{moduleName}/fields")]
        [Authorize]
        [ProducesResponseType(typeof(ModuleConfig), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetModuleFieldConfig(string moduleName, CancellationToken cancellationToken)
        {
            var configs = await _navigationConfigService.GetModuleConfigsAsync(cancellationToken);
            if (configs.TryGetValue(moduleName, out var config))
            {
                return Ok(config);
            }

            return NotFound($"Module '{moduleName}' not found");
        }

        /// <summary>
        /// Gets all module field configurations.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Dictionary of module field configurations</returns>
        [HttpGet("modules/fields")]
        [Authorize]
        [ProducesResponseType(typeof(System.Collections.Generic.Dictionary<string, ModuleConfig>), 200)]
        public async Task<IActionResult> GetAllModuleFieldConfigs(CancellationToken cancellationToken)
        {
            var configs = await _navigationConfigService.GetModuleConfigsAsync(cancellationToken);
            return Ok(configs);
        }

        /// <summary>
        /// Gets only the available (enabled and visible) navigation items.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of available navigation items</returns>
        [HttpGet("items")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(System.Collections.Generic.IEnumerable<NavigationItemConfig>), 200)]
        public async Task<IActionResult> GetAvailableNavItems(CancellationToken cancellationToken)
        {
            var items = await _navigationConfigService.GetAvailableNavItemsAsync(cancellationToken);
            return Ok(items);
        }

        /// <summary>
        /// Gets navigation items filtered by the current user's permissions.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>User-filtered navigation items</returns>
        [HttpGet("items/user")]
        [Authorize]
        [ProducesResponseType(typeof(System.Collections.Generic.IEnumerable<NavigationItemConfig>), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetUserNavItems(CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized("Unable to determine user identity");
            }

            var config = await _navigationConfigService.GetNavigationConfigForUserAsync(userId.Value, cancellationToken);
            return Ok(config.NavItems);
        }

        /// <summary>
        /// Gets external service configurations for pluggable providers.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>External service configurations</returns>
        [HttpGet("external-services")]
        [Authorize]
        [ProducesResponseType(typeof(System.Collections.Generic.Dictionary<string, ExternalServiceConfig>), 200)]
        public async Task<IActionResult> GetExternalServices(CancellationToken cancellationToken)
        {
            var services = await _navigationConfigService.GetExternalServiceConfigsAsync(cancellationToken);
            return Ok(services);
        }

        /// <summary>
        /// Gets the status of all pluggable providers.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Provider status information</returns>
        [HttpGet("provider-status")]
        [Authorize]
        [ProducesResponseType(typeof(System.Collections.Generic.Dictionary<string, ProviderStatus>), 200)]
        public async Task<IActionResult> GetProviderStatus(CancellationToken cancellationToken)
        {
            var status = await _navigationConfigService.GetProviderStatusAsync(cancellationToken);
            return Ok(status);
        }

        /// <summary>
        /// Invalidates the navigation configuration cache.
        /// Useful after configuration changes.
        /// </summary>
        /// <returns>Success message</returns>
        [HttpPost("cache/invalidate")]
        [Authorize(Roles = "Admin,SystemAdmin")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public IActionResult InvalidateCache()
        {
            _navigationConfigService.InvalidateCache();
            _logger.LogInformation("Navigation cache invalidated by user {UserId}", GetCurrentUserId());
            return Ok(new { message = "Cache invalidated successfully" });
        }

        /// <summary>
        /// Gets the current user ID from the JWT claims.
        /// </summary>
        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value;

            if (int.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }

            return null;
        }
    }
}
