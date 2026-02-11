using System.Threading;
using System.Threading.Tasks;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CRM.Api.Controllers
{
    /// <summary>
    /// API controller for navigation configuration.
    /// Provides navigation structure with pluggable architecture awareness.
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
    }
}
