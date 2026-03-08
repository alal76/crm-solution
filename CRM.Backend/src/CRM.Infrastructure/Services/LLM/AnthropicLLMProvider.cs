// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CRM.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.LLM;

/// <summary>
/// AP-036: Anthropic Claude API provider.
/// Extracted from LLMService.CallAnthropicAsync.
/// </summary>
public class AnthropicLLMProvider : LLMProviderBase
{
    public override string ProviderName => "anthropic";
    public override string[] SupportedAliases => new[] { "anthropic" };

    public AnthropicLLMProvider(
        LLMProviderOptions options,
        HttpClient httpClient,
        ILLMSettingsService? settingsService,
        ILogger<AnthropicLLMProvider> logger)
        : base(options, httpClient, settingsService, logger) { }

    /// <inheritdoc />
    /// <remarks>AP-036: Extracted from LLMService.CallAnthropicAsync</remarks>
    public override async Task<LLMResponse> CallAsync(LLMRequest request, CancellationToken cancellationToken = default)
    {
        var model = request.Model.StartsWith("claude") ? request.Model : Options.Anthropic.DefaultModel;
        var apiKey = await ResolveApiKeyAsync("anthropic", Options.Anthropic.ApiKey);
        var baseUrl = await ResolveBaseUrlAsync("anthropic", Options.Anthropic.BaseUrl);

        // Extract system message - Anthropic separates system from user messages
        string? systemMessage = null;
        var messages = request.Messages?.ToList() ?? new List<LLMMessage>();
        var systemMsg = messages.FirstOrDefault(m => m.Role == "system");
        if (systemMsg != null)
        {
            systemMessage = systemMsg.Content;
            messages.Remove(systemMsg);
        }

        var requestBody = new
        {
            model,
            system = systemMessage,
            messages = messages.Select(m => new { role = m.Role, content = m.Content }).ToArray(),
            temperature = request.Temperature,
            max_tokens = request.MaxTokens
        };

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/messages")
        {
            Content = new StringContent(
                JsonSerializer.Serialize(requestBody, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }),
                Encoding.UTF8,
                "application/json")
        };

        httpRequest.Headers.Add("x-api-key", apiKey);
        httpRequest.Headers.Add("anthropic-version", Options.Anthropic.ApiVersion);

        var httpResponse = await HttpClient.SendAsync(httpRequest, cancellationToken);
        var responseJson = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

        if (!httpResponse.IsSuccessStatusCode)
        {
            Logger.LogError("Anthropic API error: {StatusCode} - {Response}", httpResponse.StatusCode, responseJson);
            throw new HttpRequestException($"Anthropic API error: {httpResponse.StatusCode}");
        }

        using var doc = JsonDocument.Parse(responseJson);
        var root = doc.RootElement;

        var content = root.GetProperty("content")[0].GetProperty("text").GetString() ?? "";
        var usage = root.GetProperty("usage");

        return new LLMResponse
        {
            Success = true,
            Provider = "anthropic",
            Model = model,
            Content = content,
            PromptTokens = usage.GetProperty("input_tokens").GetInt32(),
            CompletionTokens = usage.GetProperty("output_tokens").GetInt32(),
            TotalTokens = usage.GetProperty("input_tokens").GetInt32() + usage.GetProperty("output_tokens").GetInt32()
        };
    }
}
