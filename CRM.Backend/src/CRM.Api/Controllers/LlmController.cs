using CRM.Api.Infrastructure;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

[ApiController]
[Route("api/llm")]
[Authorize]
public class LlmController : CrmControllerBase
{
    private readonly IProviderHealthService _healthService;
    private readonly IProviderRegistryService _registryService;
    private readonly ILogger<LlmController> _logger;

    public LlmController(
        IProviderHealthService healthService,
        IProviderRegistryService registryService,
        ILogger<LlmController> logger)
    {
        _healthService = healthService;
        _registryService = registryService;
        _logger = logger;
    }

    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHealth(CancellationToken ct)
    {
        var health = await _healthService.GetCategoryProvidersHealthAsync("AI", ct);
        return Ok(health);
    }

    [HttpGet("models")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetModels(CancellationToken ct)
    {
        var config = await _registryService.GetActiveProviderConfigAsync("AI", ct);
        return Ok(config);
    }

    [HttpGet("providers")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProviders(CancellationToken ct)
    {
        var providers = await _registryService.GetProvidersByCategoryAsync("AI", ct);
        return Ok(providers);
    }

    [HttpPost("chat")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Chat([FromBody] LlmChatRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
            return BadRequest(new { message = "Message is required." });

        // Proxy to the AI agent system - use /api/agents/{agentId}/chat for full agent chat
        return Ok(new { message = "Use /api/agents/{agentId}/chat for agent-powered conversations. Direct LLM proxy is available via the AI provider configuration." });
    }

    [HttpPost("complete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Complete([FromBody] LlmCompletionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Prompt))
            return BadRequest(new { message = "Prompt is required." });

        return Ok(new { message = "Use /api/agents/ai-email/chat for AI-powered text generation." });
    }

    [HttpPost("embed")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Embed([FromBody] LlmEmbedRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Text))
            return BadRequest(new { message = "Text is required." });

        return Ok(new { message = "Embedding is handled internally by the AI knowledge search service." });
    }
}

public class LlmChatRequest
{
    public string Message { get; set; } = string.Empty;
    public string? Model { get; set; }
    public double? Temperature { get; set; }
}

public class LlmCompletionRequest
{
    public string Prompt { get; set; } = string.Empty;
    public string? Model { get; set; }
    public int? MaxTokens { get; set; }
}

public class LlmEmbedRequest
{
    public string Text { get; set; } = string.Empty;
    public string? Model { get; set; }
}
