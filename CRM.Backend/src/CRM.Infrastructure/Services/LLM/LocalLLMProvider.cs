// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Text;
using System.Text.Json;
using CRM.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.LLM;

/// <summary>
/// AP-036: Local / self-hosted LLM provider (Ollama, LM Studio, vLLM, etc.).
/// Extracted from LLMService.CallLocalLLMAsync, CallOllamaAsync, CallLocalOpenAICompatibleAsync.
/// </summary>
public class LocalLLMProvider : LLMProviderBase
{
    public override string ProviderName => "local";
    public override string[] SupportedAliases => new[] { "local", "ollama", "lmstudio", "vllm" };

    public LocalLLMProvider(
        LLMProviderOptions options,
        HttpClient httpClient,
        ILLMSettingsService? settingsService,
        ILogger<LocalLLMProvider> logger)
        : base(options, httpClient, settingsService, logger) { }

    /// <inheritdoc />
    /// <remarks>AP-036: Extracted from LLMService.CallLocalLLMAsync + CallOllamaAsync + CallLocalOpenAICompatibleAsync</remarks>
    public override async Task<LLMResponse> CallAsync(LLMRequest request, CancellationToken cancellationToken = default)
    {
        var model = !string.IsNullOrEmpty(request.Model) ? request.Model : Options.LocalLLM.DefaultModel;
        var apiFormat = Options.LocalLLM.ApiFormat.ToLower();

        return apiFormat switch
        {
            "ollama" => await CallOllamaAsync(request, model, cancellationToken),
            _ => await CallLocalOpenAICompatibleAsync(request, model, cancellationToken)
        };
    }

    private async Task<LLMResponse> CallOllamaAsync(LLMRequest request, string model, CancellationToken cancellationToken)
    {
        var messages = request.Messages?.Select(m => new { role = m.Role, content = m.Content }).ToArray();

        var requestBody = new
        {
            model,
            messages,
            stream = false,
            options = new
            {
                temperature = request.Temperature,
                num_predict = request.MaxTokens
            }
        };

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{Options.LocalLLM.BaseUrl}/api/chat") // NOSONAR - S5332: internal container-to-container URL
        {
            Content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json")
        };

        foreach (var header in Options.LocalLLM.Headers)
        {
            httpRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        var httpResponse = await HttpClient.SendAsync(httpRequest, cancellationToken);
        var responseJson = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

        if (!httpResponse.IsSuccessStatusCode)
        {
            Logger.LogError("Ollama error: {StatusCode} - {Response}", httpResponse.StatusCode, responseJson);
            throw new HttpRequestException($"Ollama error: {httpResponse.StatusCode}");
        }

        using var doc = JsonDocument.Parse(responseJson);
        var root = doc.RootElement;

        var content = root.GetProperty("message").GetProperty("content").GetString() ?? "";

        int promptTokens = 0, completionTokens = 0;
        if (root.TryGetProperty("prompt_eval_count", out var pec))
        {
            promptTokens = pec.GetInt32();
        }
        if (root.TryGetProperty("eval_count", out var ec))
        {
            completionTokens = ec.GetInt32();
        }

        return new LLMResponse
        {
            Success = true,
            Provider = "ollama",
            Model = model,
            Content = content,
            PromptTokens = promptTokens,
            CompletionTokens = completionTokens,
            TotalTokens = promptTokens + completionTokens
        };
    }

    private async Task<LLMResponse> CallLocalOpenAICompatibleAsync(LLMRequest request, string model, CancellationToken cancellationToken)
    {
        var requestBody = new
        {
            model,
            messages = request.Messages?.Select(m => new { role = m.Role, content = m.Content }).ToArray(),
            temperature = request.Temperature,
            max_tokens = request.MaxTokens,
            stream = false
        };

        var url = Options.LocalLLM.BaseUrl.TrimEnd('/'); // NOSONAR - S5332
        if (!url.EndsWith("/v1/chat/completions"))
        {
            url = url.EndsWith("/v1") ? $"{url}/chat/completions" : $"{url}/v1/chat/completions";
        }

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json")
        };

        if (!string.IsNullOrEmpty(Options.LocalLLM.ApiKey))
        {
            httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Options.LocalLLM.ApiKey);
        }

        foreach (var header in Options.LocalLLM.Headers)
        {
            httpRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        var httpResponse = await HttpClient.SendAsync(httpRequest, cancellationToken);
        var responseJson = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

        if (!httpResponse.IsSuccessStatusCode)
        {
            Logger.LogError("Local LLM error: {StatusCode} - {Response}", httpResponse.StatusCode, responseJson);
            throw new HttpRequestException($"Local LLM error: {httpResponse.StatusCode}");
        }

        using var doc = JsonDocument.Parse(responseJson);
        var root = doc.RootElement;

        var content = root.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "";

        int promptTokens = 0, completionTokens = 0;
        if (root.TryGetProperty("usage", out var usage))
        {
            if (usage.TryGetProperty("prompt_tokens", out var pt))
            {
                promptTokens = pt.GetInt32();
            }
            if (usage.TryGetProperty("completion_tokens", out var ct))
            {
                completionTokens = ct.GetInt32();
            }
        }

        return new LLMResponse
        {
            Success = true,
            Provider = "local",
            Model = model,
            Content = content,
            PromptTokens = promptTokens,
            CompletionTokens = completionTokens,
            TotalTokens = promptTokens + completionTokens
        };
    }
}
