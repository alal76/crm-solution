// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CRM.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.LLM;

/// <summary>
/// AP-036: OpenAI API provider (GPT-4, GPT-4o, etc.).
/// Extracted from LLMService.CallOpenAIAsync.
/// </summary>
public class OpenAILLMProvider : LLMProviderBase
{
    public override string ProviderName => "openai";
    public override string[] SupportedAliases => new[] { "openai" };

    public OpenAILLMProvider(
        LLMProviderOptions options,
        HttpClient httpClient,
        ILLMSettingsService? settingsService,
        ILogger<OpenAILLMProvider> logger)
        : base(options, httpClient, settingsService, logger) { }

    /// <inheritdoc />
    /// <remarks>AP-036: Extracted from LLMService.CallOpenAIAsync</remarks>
    public override async Task<LLMResponse> CallAsync(LLMRequest request, CancellationToken cancellationToken = default)
    {
        var model = request.Model.StartsWith("gpt") || request.Model.StartsWith("o1") || request.Model.StartsWith("o3")
            ? request.Model : Options.OpenAI.DefaultModel;
        var apiKey = await ResolveApiKeyAsync("openai", Options.OpenAI.ApiKey);
        var baseUrl = await ResolveBaseUrlAsync("openai", Options.OpenAI.BaseUrl);

        var requestBody = new
        {
            model,
            messages = request.Messages?.Select(m => new { role = m.Role, content = m.Content }).ToArray(),
            temperature = request.Temperature,
            max_tokens = request.MaxTokens,
            response_format = request.JsonMode ? new { type = "json_object" } : null
        };

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/chat/completions")
        {
            Content = new StringContent(
                JsonSerializer.Serialize(requestBody, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }),
                Encoding.UTF8,
                "application/json")
        };

        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        if (!string.IsNullOrEmpty(Options.OpenAI.Organization))
        {
            httpRequest.Headers.Add("OpenAI-Organization", Options.OpenAI.Organization);
        }

        var httpResponse = await HttpClient.SendAsync(httpRequest, cancellationToken);
        var responseJson = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

        if (!httpResponse.IsSuccessStatusCode)
        {
            Logger.LogError("OpenAI API error: {StatusCode} - {Response}", httpResponse.StatusCode, responseJson);
            throw new HttpRequestException($"OpenAI API error: {httpResponse.StatusCode}");
        }

        using var doc = JsonDocument.Parse(responseJson);
        var root = doc.RootElement;

        return new LLMResponse
        {
            Success = true,
            Provider = "openai",
            Model = model,
            Content = root.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "",
            PromptTokens = root.GetProperty("usage").GetProperty("prompt_tokens").GetInt32(),
            CompletionTokens = root.GetProperty("usage").GetProperty("completion_tokens").GetInt32(),
            TotalTokens = root.GetProperty("usage").GetProperty("total_tokens").GetInt32()
        };
    }
}
