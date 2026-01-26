/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * LLM Integration Service - Multi-provider AI service with fallback support
 */

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Configuration for LLM providers
/// </summary>
public class LLMProviderOptions
{
    public string DefaultProvider { get; set; } = "openai";
    public OpenAIOptions OpenAI { get; set; } = new();
    public AzureOpenAIOptions AzureOpenAI { get; set; } = new();
    public AnthropicOptions Anthropic { get; set; } = new();
    public GoogleCloudOptions GoogleCloud { get; set; } = new();
    public AWSBedrockOptions AWSBedrock { get; set; } = new();
    public DeepSeekOptions DeepSeek { get; set; } = new();
    public LocalLLMOptions LocalLLM { get; set; } = new();
    public CustomEndpointOptions CustomEndpoint { get; set; } = new();
    public int DefaultMaxTokens { get; set; } = 1000;
    public double DefaultTemperature { get; set; } = 0.7;
    public int TimeoutSeconds { get; set; } = 60;
    public int MaxRetries { get; set; } = 3;
    public bool EnableFallback { get; set; } = true;
    public string[] FallbackOrder { get; set; } = { "openai", "anthropic", "azure", "google", "local" };
}

public class OpenAIOptions
{
    public string ApiKey { get; set; } = "";
    public string BaseUrl { get; set; } = "https://api.openai.com/v1";
    public string DefaultModel { get; set; } = "gpt-4";
    public string Organization { get; set; } = "";
}

/// <summary>
/// Azure OpenAI Service configuration
/// </summary>
public class AzureOpenAIOptions
{
    public string Endpoint { get; set; } = "";  // e.g., https://your-resource.openai.azure.com
    public string ApiKey { get; set; } = "";
    public string DeploymentName { get; set; } = "";  // Your deployed model name
    public string ApiVersion { get; set; } = "2024-02-01";
    public string DefaultModel { get; set; } = "gpt-4";
}

public class AnthropicOptions
{
    public string ApiKey { get; set; } = "";
    public string BaseUrl { get; set; } = "https://api.anthropic.com/v1";
    public string DefaultModel { get; set; } = "claude-3-sonnet-20240229";
    public string ApiVersion { get; set; } = "2023-06-01";
}

/// <summary>
/// Google Cloud Vertex AI / Gemini configuration
/// </summary>
public class GoogleCloudOptions
{
    public string ProjectId { get; set; } = "";
    public string Location { get; set; } = "us-central1";
    public string ApiKey { get; set; } = "";  // For Gemini API
    public string ServiceAccountKeyPath { get; set; } = "";  // For Vertex AI (JSON key file)
    public string DefaultModel { get; set; } = "gemini-1.5-pro";
    public bool UseVertexAI { get; set; } = false;  // true = Vertex AI, false = Gemini API
}

/// <summary>
/// AWS Bedrock configuration
/// </summary>
public class AWSBedrockOptions
{
    public string Region { get; set; } = "us-east-1";
    public string AccessKeyId { get; set; } = "";
    public string SecretAccessKey { get; set; } = "";
    public string SessionToken { get; set; } = "";  // Optional, for temporary credentials
    public string DefaultModel { get; set; } = "anthropic.claude-3-sonnet-20240229-v1:0";
    public bool UseDefaultCredentials { get; set; } = true;  // Use AWS credential chain
}

/// <summary>
/// DeepSeek AI configuration
/// </summary>
public class DeepSeekOptions
{
    public string ApiKey { get; set; } = "";
    public string BaseUrl { get; set; } = "https://api.deepseek.com";
    public string DefaultModel { get; set; } = "deepseek-chat";  // or deepseek-coder, deepseek-reasoner
}

/// <summary>
/// Local/Self-hosted LLM configuration (Ollama, LM Studio, vLLM, etc.)
/// </summary>
public class LocalLLMOptions
{
    public string BaseUrl { get; set; } = "http://localhost:11434";  // Ollama default
    public string ApiKey { get; set; } = "";  // Optional for some local servers
    public string DefaultModel { get; set; } = "llama3";
    public string ApiFormat { get; set; } = "ollama";  // ollama, openai, or custom
    public Dictionary<string, string> Headers { get; set; } = new();
    public bool Enabled { get; set; } = false;
}

public class CustomEndpointOptions
{
    public string Url { get; set; } = "";
    public string ApiKey { get; set; } = "";
    public Dictionary<string, string> Headers { get; set; } = new();
}

/// <summary>
/// LLM request parameters
/// </summary>
public class LLMRequest
{
    public string Provider { get; set; } = "openai";
    public string Model { get; set; } = "gpt-4";
    public string Prompt { get; set; } = "";
    public string? SystemPrompt { get; set; }
    public List<LLMMessage>? Messages { get; set; }
    public double Temperature { get; set; } = 0.7;
    public int MaxTokens { get; set; } = 1000;
    public bool JsonMode { get; set; }
    public string? JsonSchema { get; set; }
    public Dictionary<string, object>? Variables { get; set; }
}

public class LLMMessage
{
    public string Role { get; set; } = "user";
    public string Content { get; set; } = "";
}

/// <summary>
/// LLM response
/// </summary>
public class LLMResponse
{
    public bool Success { get; set; }
    public string Content { get; set; } = "";
    public string? Error { get; set; }
    public string Provider { get; set; } = "";
    public string Model { get; set; } = "";
    public int PromptTokens { get; set; }
    public int CompletionTokens { get; set; }
    public int TotalTokens { get; set; }
    public double DurationMs { get; set; }
    public object? ParsedJson { get; set; }
}

/// <summary>
/// LLM provider info for frontend
/// </summary>
public class LLMProviderInfo
{
    public string Value { get; set; } = "";
    public string Label { get; set; } = "";
    public bool IsConfigured { get; set; }
    public List<LLMModelInfo> Models { get; set; } = new();
}

/// <summary>
/// LLM model info for frontend
/// </summary>
public class LLMModelInfo
{
    public string Value { get; set; } = "";
    public string Label { get; set; } = "";
    public string Provider { get; set; } = "";
    public bool IsDefault { get; set; }
}

/// <summary>
/// Interface for LLM service
/// </summary>
public interface ILLMService
{
    Task<LLMResponse> CompletionAsync(LLMRequest request, CancellationToken cancellationToken = default);
    Task<LLMResponse> ChatAsync(LLMRequest request, CancellationToken cancellationToken = default);
    bool IsConfigured(string provider);
    List<LLMProviderInfo> GetAvailableProviders();
    List<LLMModelInfo> GetAvailableModels();
}

/// <summary>
/// Multi-provider LLM service with fallback support
/// </summary>
public class LLMService : ILLMService
{
    private readonly ILogger<LLMService> _logger;
    private readonly LLMProviderOptions _options;
    private readonly HttpClient _httpClient;
    private readonly IResilienceService? _resilienceService;

    public LLMService(
        ILogger<LLMService> logger,
        IOptions<LLMProviderOptions> options,
        HttpClient? httpClient = null,
        IResilienceService? resilienceService = null)
    {
        _logger = logger;
        _options = options.Value;
        _httpClient = httpClient ?? new HttpClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);
        _resilienceService = resilienceService;
    }

    public bool IsConfigured(string provider)
    {
        return provider.ToLower() switch
        {
            "openai" => !string.IsNullOrEmpty(_options.OpenAI.ApiKey),
            "azure" or "azureopenai" => !string.IsNullOrEmpty(_options.AzureOpenAI.Endpoint) && 
                                        !string.IsNullOrEmpty(_options.AzureOpenAI.ApiKey),
            "anthropic" => !string.IsNullOrEmpty(_options.Anthropic.ApiKey),
            "google" or "gemini" or "vertexai" => !string.IsNullOrEmpty(_options.GoogleCloud.ApiKey) || 
                                                   !string.IsNullOrEmpty(_options.GoogleCloud.ServiceAccountKeyPath),
            "aws" or "bedrock" => _options.AWSBedrock.UseDefaultCredentials || 
                                  (!string.IsNullOrEmpty(_options.AWSBedrock.AccessKeyId) && 
                                   !string.IsNullOrEmpty(_options.AWSBedrock.SecretAccessKey)),
            "deepseek" => !string.IsNullOrEmpty(_options.DeepSeek.ApiKey),
            "local" or "ollama" or "lmstudio" or "vllm" => _options.LocalLLM.Enabled && 
                                                           !string.IsNullOrEmpty(_options.LocalLLM.BaseUrl),
            "custom" => !string.IsNullOrEmpty(_options.CustomEndpoint.Url),
            _ => false
        };
    }

    public List<LLMProviderInfo> GetAvailableProviders()
    {
        var providers = new List<LLMProviderInfo>
        {
            new() { Value = "openai", Label = "OpenAI", IsConfigured = IsConfigured("openai"), Models = GetModelsForProvider("openai") },
            new() { Value = "azure", Label = "Azure OpenAI", IsConfigured = IsConfigured("azure"), Models = GetModelsForProvider("azure") },
            new() { Value = "anthropic", Label = "Anthropic (Claude)", IsConfigured = IsConfigured("anthropic"), Models = GetModelsForProvider("anthropic") },
            new() { Value = "google", Label = "Google Cloud (Gemini)", IsConfigured = IsConfigured("google"), Models = GetModelsForProvider("google") },
            new() { Value = "bedrock", Label = "AWS Bedrock", IsConfigured = IsConfigured("bedrock"), Models = GetModelsForProvider("bedrock") },
            new() { Value = "deepseek", Label = "DeepSeek", IsConfigured = IsConfigured("deepseek"), Models = GetModelsForProvider("deepseek") },
            new() { Value = "local", Label = "Local LLM (Ollama/LM Studio)", IsConfigured = IsConfigured("local"), Models = GetModelsForProvider("local") },
            new() { Value = "custom", Label = "Custom Endpoint", IsConfigured = IsConfigured("custom"), Models = GetModelsForProvider("custom") }
        };
        return providers;
    }

    public List<LLMModelInfo> GetAvailableModels()
    {
        var models = new List<LLMModelInfo>();
        foreach (var provider in GetAvailableProviders().Where(p => p.IsConfigured))
        {
            models.AddRange(provider.Models);
        }
        return models;
    }

    private List<LLMModelInfo> GetModelsForProvider(string provider)
    {
        return provider.ToLower() switch
        {
            "openai" => new List<LLMModelInfo>
            {
                new() { Value = "gpt-4o", Label = "GPT-4o", Provider = "openai", IsDefault = _options.OpenAI.DefaultModel == "gpt-4o" },
                new() { Value = "gpt-4o-mini", Label = "GPT-4o Mini", Provider = "openai", IsDefault = _options.OpenAI.DefaultModel == "gpt-4o-mini" },
                new() { Value = "gpt-4-turbo", Label = "GPT-4 Turbo", Provider = "openai", IsDefault = _options.OpenAI.DefaultModel == "gpt-4-turbo" },
                new() { Value = "gpt-4", Label = "GPT-4", Provider = "openai", IsDefault = _options.OpenAI.DefaultModel == "gpt-4" },
                new() { Value = "gpt-3.5-turbo", Label = "GPT-3.5 Turbo", Provider = "openai", IsDefault = _options.OpenAI.DefaultModel == "gpt-3.5-turbo" },
                new() { Value = "o1-preview", Label = "o1 Preview (Reasoning)", Provider = "openai", IsDefault = _options.OpenAI.DefaultModel == "o1-preview" },
                new() { Value = "o1-mini", Label = "o1 Mini", Provider = "openai", IsDefault = _options.OpenAI.DefaultModel == "o1-mini" }
            },
            "azure" => new List<LLMModelInfo>
            {
                new() { Value = _options.AzureOpenAI.DeploymentName, Label = $"Azure: {_options.AzureOpenAI.DeploymentName}", Provider = "azure", IsDefault = true }
            },
            "anthropic" => new List<LLMModelInfo>
            {
                new() { Value = "claude-3-5-sonnet-20241022", Label = "Claude 3.5 Sonnet", Provider = "anthropic", IsDefault = _options.Anthropic.DefaultModel?.Contains("sonnet") == true },
                new() { Value = "claude-3-5-haiku-20241022", Label = "Claude 3.5 Haiku", Provider = "anthropic", IsDefault = _options.Anthropic.DefaultModel?.Contains("haiku") == true },
                new() { Value = "claude-3-opus-20240229", Label = "Claude 3 Opus", Provider = "anthropic", IsDefault = _options.Anthropic.DefaultModel?.Contains("opus") == true }
            },
            "google" => new List<LLMModelInfo>
            {
                new() { Value = "gemini-2.0-flash-exp", Label = "Gemini 2.0 Flash", Provider = "google", IsDefault = _options.GoogleCloud.DefaultModel?.Contains("2.0") == true },
                new() { Value = "gemini-1.5-pro", Label = "Gemini 1.5 Pro", Provider = "google", IsDefault = _options.GoogleCloud.DefaultModel == "gemini-1.5-pro" },
                new() { Value = "gemini-1.5-flash", Label = "Gemini 1.5 Flash", Provider = "google", IsDefault = _options.GoogleCloud.DefaultModel == "gemini-1.5-flash" }
            },
            "bedrock" => new List<LLMModelInfo>
            {
                new() { Value = "anthropic.claude-3-5-sonnet-20241022-v2:0", Label = "Claude 3.5 Sonnet (Bedrock)", Provider = "bedrock", IsDefault = _options.AWSBedrock.DefaultModel?.Contains("sonnet") == true },
                new() { Value = "anthropic.claude-3-haiku-20240307-v1:0", Label = "Claude 3 Haiku (Bedrock)", Provider = "bedrock", IsDefault = _options.AWSBedrock.DefaultModel?.Contains("haiku") == true },
                new() { Value = "amazon.titan-text-premier-v1:0", Label = "Amazon Titan Text Premier", Provider = "bedrock", IsDefault = _options.AWSBedrock.DefaultModel?.Contains("titan") == true },
                new() { Value = "meta.llama3-70b-instruct-v1:0", Label = "Llama 3 70B (Bedrock)", Provider = "bedrock", IsDefault = _options.AWSBedrock.DefaultModel?.Contains("llama") == true }
            },
            "deepseek" => new List<LLMModelInfo>
            {
                new() { Value = "deepseek-chat", Label = "DeepSeek Chat", Provider = "deepseek", IsDefault = _options.DeepSeek.DefaultModel == "deepseek-chat" },
                new() { Value = "deepseek-coder", Label = "DeepSeek Coder", Provider = "deepseek", IsDefault = _options.DeepSeek.DefaultModel == "deepseek-coder" },
                new() { Value = "deepseek-reasoner", Label = "DeepSeek Reasoner", Provider = "deepseek", IsDefault = _options.DeepSeek.DefaultModel == "deepseek-reasoner" }
            },
            "local" => new List<LLMModelInfo>
            {
                new() { Value = _options.LocalLLM.DefaultModel, Label = $"Local: {_options.LocalLLM.DefaultModel}", Provider = "local", IsDefault = true },
                new() { Value = "llama3", Label = "Llama 3", Provider = "local" },
                new() { Value = "mistral", Label = "Mistral", Provider = "local" },
                new() { Value = "codellama", Label = "Code Llama", Provider = "local" },
                new() { Value = "phi3", Label = "Phi-3", Provider = "local" }
            },
            "custom" => new List<LLMModelInfo>
            {
                new() { Value = "custom", Label = "Custom Model", Provider = "custom", IsDefault = true }
            },
            _ => new List<LLMModelInfo>()
        };
    }

    public async Task<LLMResponse> CompletionAsync(LLMRequest request, CancellationToken cancellationToken = default)
    {
        // Convert to chat format for modern APIs
        request.Messages = new List<LLMMessage>
        {
            new() { Role = "user", Content = InterpolateVariables(request.Prompt, request.Variables) }
        };

        if (!string.IsNullOrEmpty(request.SystemPrompt))
        {
            request.Messages.Insert(0, new LLMMessage { Role = "system", Content = request.SystemPrompt });
        }

        return await ChatAsync(request, cancellationToken);
    }

    public async Task<LLMResponse> ChatAsync(LLMRequest request, CancellationToken cancellationToken = default)
    {
        // Use resilience service if available for better error handling and circuit breaking
        if (_resilienceService != null)
        {
            return await _resilienceService.ExecuteWithFallbackAsync(
                $"LLM-{request.Provider}",
                async ct => await ExecuteChatInternalAsync(request, ct),
                ex => new LLMResponse
                {
                    Success = false,
                    Error = $"LLM service temporarily unavailable: {ex.Message}",
                    Provider = request.Provider
                },
                cancellationToken);
        }

        return await ExecuteChatInternalAsync(request, cancellationToken);
    }

    private async Task<LLMResponse> ExecuteChatInternalAsync(LLMRequest request, CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;
        var providers = GetProviderOrder(request.Provider);
        Exception? lastException = null;

        foreach (var provider in providers)
        {
            if (!IsConfigured(provider))
            {
                _logger.LogDebug("Skipping provider {Provider} - not configured", provider);
                continue;
            }

            try
            {
                var response = provider.ToLower() switch
                {
                    "openai" => await CallOpenAIAsync(request, cancellationToken),
                    "azure" or "azureopenai" => await CallAzureOpenAIAsync(request, cancellationToken),
                    "anthropic" => await CallAnthropicAsync(request, cancellationToken),
                    "google" or "gemini" or "vertexai" => await CallGoogleCloudAsync(request, cancellationToken),
                    "aws" or "bedrock" => await CallAWSBedrockAsync(request, cancellationToken),
                    "deepseek" => await CallDeepSeekAsync(request, cancellationToken),
                    "local" or "ollama" or "lmstudio" or "vllm" => await CallLocalLLMAsync(request, cancellationToken),
                    "custom" => await CallCustomEndpointAsync(request, cancellationToken),
                    _ => throw new NotSupportedException($"Provider {provider} is not supported")
                };

                response.DurationMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
                
                // Parse JSON if requested
                if (request.JsonMode && response.Success)
                {
                    try
                    {
                        response.ParsedJson = JsonSerializer.Deserialize<object>(response.Content);
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning(ex, "Failed to parse LLM response as JSON");
                    }
                }

                return response;
            }
            catch (Exception ex)
            {
                lastException = ex;
                _logger.LogWarning(ex, "LLM call to {Provider} failed, trying fallback", provider);
                
                if (!_options.EnableFallback) break;
            }
        }

        return new LLMResponse
        {
            Success = false,
            Error = lastException?.Message ?? "No LLM provider available",
            DurationMs = (DateTime.UtcNow - startTime).TotalMilliseconds
        };
    }

    private string[] GetProviderOrder(string requestedProvider)
    {
        if (!_options.EnableFallback)
        {
            return new[] { requestedProvider };
        }

        var order = new List<string> { requestedProvider };
        foreach (var fallback in _options.FallbackOrder)
        {
            if (fallback != requestedProvider)
            {
                order.Add(fallback);
            }
        }
        return order.ToArray();
    }

    private string InterpolateVariables(string template, Dictionary<string, object>? variables)
    {
        if (variables == null || variables.Count == 0) return template;

        var result = template;
        foreach (var kvp in variables)
        {
            result = result.Replace($"{{{{{kvp.Key}}}}}", kvp.Value?.ToString() ?? "");
        }
        return result;
    }

    #region OpenAI Provider

    private async Task<LLMResponse> CallOpenAIAsync(LLMRequest request, CancellationToken cancellationToken)
    {
        var model = request.Model.StartsWith("gpt") ? request.Model : _options.OpenAI.DefaultModel;

        var requestBody = new
        {
            model,
            messages = request.Messages?.Select(m => new { role = m.Role, content = m.Content }).ToArray(),
            temperature = request.Temperature,
            max_tokens = request.MaxTokens,
            response_format = request.JsonMode ? new { type = "json_object" } : null
        };

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_options.OpenAI.BaseUrl}/chat/completions")
        {
            Content = new StringContent(
                JsonSerializer.Serialize(requestBody, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }),
                Encoding.UTF8,
                "application/json"
            )
        };

        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.OpenAI.ApiKey);
        
        if (!string.IsNullOrEmpty(_options.OpenAI.Organization))
        {
            httpRequest.Headers.Add("OpenAI-Organization", _options.OpenAI.Organization);
        }

        var httpResponse = await _httpClient.SendAsync(httpRequest, cancellationToken);
        var responseJson = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

        if (!httpResponse.IsSuccessStatusCode)
        {
            _logger.LogError("OpenAI API error: {StatusCode} - {Response}", httpResponse.StatusCode, responseJson);
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

    #endregion

    #region Azure OpenAI Provider

    private async Task<LLMResponse> CallAzureOpenAIAsync(LLMRequest request, CancellationToken cancellationToken)
    {
        var deploymentName = _options.AzureOpenAI.DeploymentName;
        var apiVersion = _options.AzureOpenAI.ApiVersion;

        var requestBody = new
        {
            messages = request.Messages?.Select(m => new { role = m.Role, content = m.Content }).ToArray(),
            temperature = request.Temperature,
            max_tokens = request.MaxTokens,
            response_format = request.JsonMode ? new { type = "json_object" } : null
        };

        var url = $"{_options.AzureOpenAI.Endpoint}/openai/deployments/{deploymentName}/chat/completions?api-version={apiVersion}";
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(requestBody, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }),
                Encoding.UTF8,
                "application/json"
            )
        };

        httpRequest.Headers.Add("api-key", _options.AzureOpenAI.ApiKey);

        var httpResponse = await _httpClient.SendAsync(httpRequest, cancellationToken);
        var responseJson = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

        if (!httpResponse.IsSuccessStatusCode)
        {
            _logger.LogError("Azure OpenAI API error: {StatusCode} - {Response}", httpResponse.StatusCode, responseJson);
            throw new HttpRequestException($"Azure OpenAI API error: {httpResponse.StatusCode}");
        }

        using var doc = JsonDocument.Parse(responseJson);
        var root = doc.RootElement;

        return new LLMResponse
        {
            Success = true,
            Provider = "azure",
            Model = _options.AzureOpenAI.DefaultModel,
            Content = root.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "",
            PromptTokens = root.GetProperty("usage").GetProperty("prompt_tokens").GetInt32(),
            CompletionTokens = root.GetProperty("usage").GetProperty("completion_tokens").GetInt32(),
            TotalTokens = root.GetProperty("usage").GetProperty("total_tokens").GetInt32()
        };
    }

    #endregion

    #region Anthropic Provider

    private async Task<LLMResponse> CallAnthropicAsync(LLMRequest request, CancellationToken cancellationToken)
    {
        var model = request.Model.StartsWith("claude") ? request.Model : _options.Anthropic.DefaultModel;

        // Extract system message
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

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_options.Anthropic.BaseUrl}/messages")
        {
            Content = new StringContent(
                JsonSerializer.Serialize(requestBody, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }),
                Encoding.UTF8,
                "application/json"
            )
        };

        httpRequest.Headers.Add("x-api-key", _options.Anthropic.ApiKey);
        httpRequest.Headers.Add("anthropic-version", _options.Anthropic.ApiVersion);

        var httpResponse = await _httpClient.SendAsync(httpRequest, cancellationToken);
        var responseJson = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

        if (!httpResponse.IsSuccessStatusCode)
        {
            _logger.LogError("Anthropic API error: {StatusCode} - {Response}", httpResponse.StatusCode, responseJson);
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

    #endregion

    #region Google Cloud (Gemini / Vertex AI)

    private async Task<LLMResponse> CallGoogleCloudAsync(LLMRequest request, CancellationToken cancellationToken)
    {
        var model = request.Model.StartsWith("gemini") ? request.Model : _options.GoogleCloud.DefaultModel;
        
        if (_options.GoogleCloud.UseVertexAI)
        {
            return await CallVertexAIAsync(request, model, cancellationToken);
        }
        else
        {
            return await CallGeminiAPIAsync(request, model, cancellationToken);
        }
    }

    private async Task<LLMResponse> CallGeminiAPIAsync(LLMRequest request, string model, CancellationToken cancellationToken)
    {
        // Gemini API format
        var contents = new List<object>();
        
        foreach (var msg in request.Messages ?? new List<LLMMessage>())
        {
            contents.Add(new
            {
                role = msg.Role == "assistant" ? "model" : msg.Role,
                parts = new[] { new { text = msg.Content } }
            });
        }

        var requestBody = new
        {
            contents,
            generationConfig = new
            {
                temperature = request.Temperature,
                maxOutputTokens = request.MaxTokens,
                responseMimeType = request.JsonMode ? "application/json" : "text/plain"
            }
        };

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={_options.GoogleCloud.ApiKey}";
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(requestBody, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }),
                Encoding.UTF8,
                "application/json"
            )
        };

        var httpResponse = await _httpClient.SendAsync(httpRequest, cancellationToken);
        var responseJson = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

        if (!httpResponse.IsSuccessStatusCode)
        {
            _logger.LogError("Gemini API error: {StatusCode} - {Response}", httpResponse.StatusCode, responseJson);
            throw new HttpRequestException($"Gemini API error: {httpResponse.StatusCode}");
        }

        using var doc = JsonDocument.Parse(responseJson);
        var root = doc.RootElement;

        var contentText = root.GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text").GetString() ?? "";

        var usageMetadata = root.GetProperty("usageMetadata");

        return new LLMResponse
        {
            Success = true,
            Provider = "google",
            Model = model,
            Content = contentText,
            PromptTokens = usageMetadata.TryGetProperty("promptTokenCount", out var pt) ? pt.GetInt32() : 0,
            CompletionTokens = usageMetadata.TryGetProperty("candidatesTokenCount", out var ct) ? ct.GetInt32() : 0,
            TotalTokens = usageMetadata.TryGetProperty("totalTokenCount", out var tt) ? tt.GetInt32() : 0
        };
    }

    private async Task<LLMResponse> CallVertexAIAsync(LLMRequest request, string model, CancellationToken cancellationToken)
    {
        // Vertex AI requires OAuth2 token from service account
        // For simplicity, using API key approach - in production, use Google.Cloud.AIPlatform SDK
        var projectId = _options.GoogleCloud.ProjectId;
        var location = _options.GoogleCloud.Location;

        var contents = new List<object>();
        foreach (var msg in request.Messages ?? new List<LLMMessage>())
        {
            contents.Add(new
            {
                role = msg.Role == "assistant" ? "model" : msg.Role,
                parts = new[] { new { text = msg.Content } }
            });
        }

        var requestBody = new
        {
            contents,
            generationConfig = new
            {
                temperature = request.Temperature,
                maxOutputTokens = request.MaxTokens
            }
        };

        var url = $"https://{location}-aiplatform.googleapis.com/v1/projects/{projectId}/locations/{location}/publishers/google/models/{model}:generateContent";
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            )
        };

        // Note: In production, use Google.Cloud.AIPlatform.V1 SDK with proper auth
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.GoogleCloud.ApiKey);

        var httpResponse = await _httpClient.SendAsync(httpRequest, cancellationToken);
        var responseJson = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

        if (!httpResponse.IsSuccessStatusCode)
        {
            _logger.LogError("Vertex AI error: {StatusCode} - {Response}", httpResponse.StatusCode, responseJson);
            throw new HttpRequestException($"Vertex AI error: {httpResponse.StatusCode}");
        }

        using var doc = JsonDocument.Parse(responseJson);
        var root = doc.RootElement;

        var contentText = root.GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text").GetString() ?? "";

        return new LLMResponse
        {
            Success = true,
            Provider = "vertexai",
            Model = model,
            Content = contentText
        };
    }

    #endregion

    #region AWS Bedrock

    private async Task<LLMResponse> CallAWSBedrockAsync(LLMRequest request, CancellationToken cancellationToken)
    {
        var model = request.Model.Contains(".") ? request.Model : _options.AWSBedrock.DefaultModel;
        var region = _options.AWSBedrock.Region;

        // Determine the request format based on model provider
        object requestBody;
        if (model.StartsWith("anthropic."))
        {
            // Anthropic Claude format for Bedrock
            var messages = request.Messages?.Where(m => m.Role != "system").ToList() ?? new List<LLMMessage>();
            var systemMsg = request.Messages?.FirstOrDefault(m => m.Role == "system");
            
            requestBody = new
            {
                anthropic_version = "bedrock-2023-05-31",
                max_tokens = request.MaxTokens,
                system = systemMsg?.Content,
                messages = messages.Select(m => new { role = m.Role, content = m.Content }).ToArray(),
                temperature = request.Temperature
            };
        }
        else if (model.StartsWith("amazon.titan"))
        {
            // Amazon Titan format
            var prompt = string.Join("\n", request.Messages?.Select(m => $"{m.Role}: {m.Content}") ?? Array.Empty<string>());
            requestBody = new
            {
                inputText = prompt,
                textGenerationConfig = new
                {
                    maxTokenCount = request.MaxTokens,
                    temperature = request.Temperature
                }
            };
        }
        else if (model.StartsWith("meta.llama"))
        {
            // Meta Llama format
            var prompt = string.Join("\n", request.Messages?.Select(m => $"{m.Role}: {m.Content}") ?? Array.Empty<string>());
            requestBody = new
            {
                prompt,
                max_gen_len = request.MaxTokens,
                temperature = request.Temperature
            };
        }
        else
        {
            // Generic format
            requestBody = new
            {
                prompt = string.Join("\n", request.Messages?.Select(m => m.Content) ?? Array.Empty<string>()),
                max_tokens = request.MaxTokens,
                temperature = request.Temperature
            };
        }

        var url = $"https://bedrock-runtime.{region}.amazonaws.com/model/{model}/invoke";
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(requestBody, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }),
                Encoding.UTF8,
                "application/json"
            )
        };

        // AWS Signature V4 signing would be needed here
        // For production, use AWS SDK for .NET (AWSSDK.BedrockRuntime)
        // This is a simplified implementation
        if (!_options.AWSBedrock.UseDefaultCredentials)
        {
            // Add basic auth headers (in production, use proper AWS SigV4)
            httpRequest.Headers.Add("X-Amz-Access-Key", _options.AWSBedrock.AccessKeyId);
            if (!string.IsNullOrEmpty(_options.AWSBedrock.SessionToken))
            {
                httpRequest.Headers.Add("X-Amz-Security-Token", _options.AWSBedrock.SessionToken);
            }
        }

        var httpResponse = await _httpClient.SendAsync(httpRequest, cancellationToken);
        var responseJson = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

        if (!httpResponse.IsSuccessStatusCode)
        {
            _logger.LogError("AWS Bedrock error: {StatusCode} - {Response}", httpResponse.StatusCode, responseJson);
            throw new HttpRequestException($"AWS Bedrock error: {httpResponse.StatusCode}");
        }

        using var doc = JsonDocument.Parse(responseJson);
        var root = doc.RootElement;

        string content;
        int inputTokens = 0, outputTokens = 0;

        // Parse based on model type
        if (model.StartsWith("anthropic."))
        {
            content = root.GetProperty("content")[0].GetProperty("text").GetString() ?? "";
            if (root.TryGetProperty("usage", out var usage))
            {
                inputTokens = usage.GetProperty("input_tokens").GetInt32();
                outputTokens = usage.GetProperty("output_tokens").GetInt32();
            }
        }
        else if (model.StartsWith("amazon.titan"))
        {
            content = root.GetProperty("results")[0].GetProperty("outputText").GetString() ?? "";
        }
        else if (model.StartsWith("meta.llama"))
        {
            content = root.GetProperty("generation").GetString() ?? "";
        }
        else
        {
            content = root.TryGetProperty("completion", out var comp) ? comp.GetString() ?? "" : responseJson;
        }

        return new LLMResponse
        {
            Success = true,
            Provider = "bedrock",
            Model = model,
            Content = content,
            PromptTokens = inputTokens,
            CompletionTokens = outputTokens,
            TotalTokens = inputTokens + outputTokens
        };
    }

    #endregion

    #region DeepSeek

    private async Task<LLMResponse> CallDeepSeekAsync(LLMRequest request, CancellationToken cancellationToken)
    {
        // DeepSeek uses OpenAI-compatible API
        var model = request.Model?.StartsWith("deepseek") == true ? request.Model : _options.DeepSeek.DefaultModel;
        
        var messages = request.Messages?.Select(m => new { role = m.Role, content = m.Content }).ToArray();

        var requestBody = new
        {
            model,
            messages,
            max_tokens = request.MaxTokens > 0 ? request.MaxTokens : 4096,
            temperature = request.Temperature
        };

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_options.DeepSeek.BaseUrl}/v1/chat/completions");
        httpRequest.Headers.Add("Authorization", $"Bearer {_options.DeepSeek.ApiKey}");
        httpRequest.Content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("DeepSeek API error: {StatusCode} - {Content}", response.StatusCode, content);
            throw new Exception($"DeepSeek API error: {response.StatusCode}");
        }

        var jsonResponse = JsonDocument.Parse(content);
        var responseContent = jsonResponse.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? "";

        var usage = jsonResponse.RootElement.GetProperty("usage");

        return new LLMResponse
        {
            Content = responseContent,
            Model = model,
            Provider = "deepseek",
            TotalTokens = usage.GetProperty("total_tokens").GetInt32(),
            PromptTokens = usage.GetProperty("prompt_tokens").GetInt32(),
            CompletionTokens = usage.GetProperty("completion_tokens").GetInt32(),
            Success = true
        };
    }

    #endregion

    #region Local LLM (Ollama, LM Studio, vLLM)

    private async Task<LLMResponse> CallLocalLLMAsync(LLMRequest request, CancellationToken cancellationToken)
    {
        var model = !string.IsNullOrEmpty(request.Model) ? request.Model : _options.LocalLLM.DefaultModel;
        var apiFormat = _options.LocalLLM.ApiFormat.ToLower();

        return apiFormat switch
        {
            "ollama" => await CallOllamaAsync(request, model, cancellationToken),
            "openai" => await CallLocalOpenAICompatibleAsync(request, model, cancellationToken),
            _ => await CallLocalOpenAICompatibleAsync(request, model, cancellationToken)
        };
    }

    private async Task<LLMResponse> CallOllamaAsync(LLMRequest request, string model, CancellationToken cancellationToken)
    {
        // Ollama native API format
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

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_options.LocalLLM.BaseUrl}/api/chat")
        {
            Content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            )
        };

        foreach (var header in _options.LocalLLM.Headers)
        {
            httpRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        var httpResponse = await _httpClient.SendAsync(httpRequest, cancellationToken);
        var responseJson = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

        if (!httpResponse.IsSuccessStatusCode)
        {
            _logger.LogError("Ollama error: {StatusCode} - {Response}", httpResponse.StatusCode, responseJson);
            throw new HttpRequestException($"Ollama error: {httpResponse.StatusCode}");
        }

        using var doc = JsonDocument.Parse(responseJson);
        var root = doc.RootElement;

        var content = root.GetProperty("message").GetProperty("content").GetString() ?? "";
        
        int promptTokens = 0, completionTokens = 0;
        if (root.TryGetProperty("prompt_eval_count", out var pec))
            promptTokens = pec.GetInt32();
        if (root.TryGetProperty("eval_count", out var ec))
            completionTokens = ec.GetInt32();

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
        // OpenAI-compatible API (LM Studio, vLLM, Text Generation WebUI, etc.)
        var requestBody = new
        {
            model,
            messages = request.Messages?.Select(m => new { role = m.Role, content = m.Content }).ToArray(),
            temperature = request.Temperature,
            max_tokens = request.MaxTokens,
            stream = false
        };

        var url = _options.LocalLLM.BaseUrl.TrimEnd('/');
        if (!url.EndsWith("/v1/chat/completions"))
        {
            url = url.EndsWith("/v1") ? $"{url}/chat/completions" : $"{url}/v1/chat/completions";
        }

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            )
        };

        if (!string.IsNullOrEmpty(_options.LocalLLM.ApiKey))
        {
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.LocalLLM.ApiKey);
        }

        foreach (var header in _options.LocalLLM.Headers)
        {
            httpRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        var httpResponse = await _httpClient.SendAsync(httpRequest, cancellationToken);
        var responseJson = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

        if (!httpResponse.IsSuccessStatusCode)
        {
            _logger.LogError("Local LLM error: {StatusCode} - {Response}", httpResponse.StatusCode, responseJson);
            throw new HttpRequestException($"Local LLM error: {httpResponse.StatusCode}");
        }

        using var doc = JsonDocument.Parse(responseJson);
        var root = doc.RootElement;

        var content = root.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "";
        
        int promptTokens = 0, completionTokens = 0;
        if (root.TryGetProperty("usage", out var usage))
        {
            if (usage.TryGetProperty("prompt_tokens", out var pt))
                promptTokens = pt.GetInt32();
            if (usage.TryGetProperty("completion_tokens", out var ct))
                completionTokens = ct.GetInt32();
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

    #endregion

    #region Custom Endpoint

    private async Task<LLMResponse> CallCustomEndpointAsync(LLMRequest request, CancellationToken cancellationToken)
    {
        var requestBody = new
        {
            model = request.Model,
            messages = request.Messages?.Select(m => new { role = m.Role, content = m.Content }).ToArray(),
            temperature = request.Temperature,
            max_tokens = request.MaxTokens
        };

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, _options.CustomEndpoint.Url)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            )
        };

        if (!string.IsNullOrEmpty(_options.CustomEndpoint.ApiKey))
        {
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.CustomEndpoint.ApiKey);
        }

        foreach (var header in _options.CustomEndpoint.Headers)
        {
            httpRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        var httpResponse = await _httpClient.SendAsync(httpRequest, cancellationToken);
        var responseJson = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

        if (!httpResponse.IsSuccessStatusCode)
        {
            _logger.LogError("Custom endpoint error: {StatusCode} - {Response}", httpResponse.StatusCode, responseJson);
            throw new HttpRequestException($"Custom endpoint error: {httpResponse.StatusCode}");
        }

        // Try to parse OpenAI-compatible response format
        using var doc = JsonDocument.Parse(responseJson);
        var root = doc.RootElement;

        string content;
        int promptTokens = 0, completionTokens = 0;

        // Handle OpenAI format
        if (root.TryGetProperty("choices", out var choices))
        {
            content = choices[0].GetProperty("message").GetProperty("content").GetString() ?? "";
            if (root.TryGetProperty("usage", out var usage))
            {
                usage.TryGetProperty("prompt_tokens", out var pt);
                usage.TryGetProperty("completion_tokens", out var ct);
                promptTokens = pt.ValueKind == JsonValueKind.Number ? pt.GetInt32() : 0;
                completionTokens = ct.ValueKind == JsonValueKind.Number ? ct.GetInt32() : 0;
            }
        }
        // Handle direct content response
        else if (root.TryGetProperty("content", out var contentProp))
        {
            content = contentProp.GetString() ?? "";
        }
        else
        {
            content = responseJson;
        }

        return new LLMResponse
        {
            Success = true,
            Provider = "custom",
            Model = request.Model,
            Content = content,
            PromptTokens = promptTokens,
            CompletionTokens = completionTokens,
            TotalTokens = promptTokens + completionTokens
        };
    }

    #endregion
}
