using System.Text.Json;
using CRM.Core.Dtos.WorkflowEngine;
using CRM.Core.Entities.WorkflowEngine;
using CRM.Core.Interfaces.WorkflowEngine;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.WorkflowEngine.StepExecutors;

/// <summary>
/// Executor for Notification steps - sends emails and notifications
/// </summary>
public class NotificationStepExecutor : IStepExecutor
{
    private readonly IWorkflowExpressionEvaluator _expressionEvaluator;
    private readonly ILogger<NotificationStepExecutor> _logger;
    // Could inject email service here: private readonly IEmailService _emailService;

    public NotificationStepExecutor(
        IWorkflowExpressionEvaluator expressionEvaluator,
        ILogger<NotificationStepExecutor> logger)
    {
        _expressionEvaluator = expressionEvaluator;
        _logger = logger;
    }

    public IEnumerable<string> SupportedStepTypes => new[] { WorkflowStepTypes.Notification };

    public async Task<StepExecutionResult> ExecuteAsync(
        StepExecutionContext context, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Executing Notification step {StepKey} for instance {InstanceId}",
            context.Step.StepKey, context.Instance.Id);

        try
        {
            // Parse configuration
            var config = !string.IsNullOrEmpty(context.Step.Configuration)
                ? JsonSerializer.Deserialize<NotificationStepConfig>(context.Step.Configuration)
                : null;

            if (config == null)
            {
                return new StepExecutionResult
                {
                    Success = false,
                    ErrorMessage = "Notification step requires configuration"
                };
            }

            var notificationsSent = new List<string>();
            var errors = new List<string>();

            // Process each notification channel
            if (config.Email != null)
            {
                var success = await SendEmailNotificationAsync(config.Email, context, cancellationToken);
                if (success)
                    notificationsSent.Add("email");
                else
                    errors.Add("Failed to send email notification");
            }

            if (config.InApp != null)
            {
                var success = await SendInAppNotificationAsync(config.InApp, context, cancellationToken);
                if (success)
                    notificationsSent.Add("in-app");
                else
                    errors.Add("Failed to send in-app notification");
            }

            if (config.Webhook != null)
            {
                var success = await SendWebhookNotificationAsync(config.Webhook, context, cancellationToken);
                if (success)
                    notificationsSent.Add("webhook");
                else
                    errors.Add("Failed to send webhook notification");
            }

            // Get next step
            var nextStepKey = await GetNextStepAsync(context, cancellationToken);

            if (notificationsSent.Any())
            {
                return new StepExecutionResult
                {
                    Success = true,
                    NextStepKey = nextStepKey,
                    OutputVariables = new Dictionary<string, object?>
                    {
                        [$"{context.Step.StepKey}_notificationsSent"] = notificationsSent,
                        [$"{context.Step.StepKey}_errors"] = errors.Any() ? errors : null
                    }
                };
            }
            else
            {
                return new StepExecutionResult
                {
                    Success = false,
                    ErrorMessage = string.Join("; ", errors),
                    ShouldRetry = true,
                    RetryAfter = DateTime.UtcNow.AddMinutes(5)
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing Notification step {StepKey}", context.Step.StepKey);
            return new StepExecutionResult
            {
                Success = false,
                ErrorMessage = $"Error sending notification: {ex.Message}",
                ErrorDetails = ex.StackTrace,
                ShouldRetry = true,
                RetryAfter = DateTime.UtcNow.AddMinutes(5)
            };
        }
    }

    public Task<WorkflowValidationResultDto> ValidateConfigurationAsync(
        WorkflowStep step, 
        CancellationToken cancellationToken = default)
    {
        var result = new WorkflowValidationResultDto { IsValid = true };

        if (string.IsNullOrEmpty(step.Configuration))
        {
            result.IsValid = false;
            result.Errors.Add($"Step '{step.Name}' requires notification configuration");
            return Task.FromResult(result);
        }

        try
        {
            var config = JsonSerializer.Deserialize<NotificationStepConfig>(step.Configuration);
            if (config == null)
            {
                result.IsValid = false;
                result.Errors.Add($"Step '{step.Name}' has invalid configuration");
            }
            else
            {
                var hasNotification = config.Email != null || config.InApp != null || config.Webhook != null;
                if (!hasNotification)
                {
                    result.IsValid = false;
                    result.Errors.Add($"Step '{step.Name}' must have at least one notification type configured");
                }

                // Validate email config
                if (config.Email != null)
                {
                    if (string.IsNullOrEmpty(config.Email.To) && 
                        string.IsNullOrEmpty(config.Email.ToRole) &&
                        string.IsNullOrEmpty(config.Email.ToVariable))
                    {
                        result.Errors.Add($"Step '{step.Name}' email must have recipient");
                        result.IsValid = false;
                    }

                    if (string.IsNullOrEmpty(config.Email.Subject))
                    {
                        result.Warnings.Add($"Step '{step.Name}' email has no subject");
                    }
                }

                // Validate webhook config
                if (config.Webhook != null)
                {
                    if (string.IsNullOrEmpty(config.Webhook.Url))
                    {
                        result.Errors.Add($"Step '{step.Name}' webhook must have URL");
                        result.IsValid = false;
                    }
                }
            }
        }
        catch (JsonException ex)
        {
            result.IsValid = false;
            result.Errors.Add($"Step '{step.Name}' configuration parse error: {ex.Message}");
        }

        return Task.FromResult(result);
    }

    private async Task<bool> SendEmailNotificationAsync(
        EmailNotificationConfig config,
        StepExecutionContext context,
        CancellationToken cancellationToken)
    {
        try
        {
            // Resolve recipient
            var recipient = config.To;
            if (!string.IsNullOrEmpty(config.ToVariable))
            {
                recipient = context.Variables.TryGetValue(config.ToVariable, out var value)
                    ? value?.ToString()
                    : null;
            }

            if (string.IsNullOrEmpty(recipient))
            {
                _logger.LogWarning("Email notification has no recipient");
                return false;
            }

            // Replace variables in subject and body
            var subject = await _expressionEvaluator.ReplaceVariablesAsync(
                config.Subject ?? "", context.Variables, cancellationToken);
            
            var body = await _expressionEvaluator.ReplaceVariablesAsync(
                config.Body ?? "", context.Variables, cancellationToken);

            _logger.LogInformation("Sending email to {Recipient}: {Subject}", recipient, subject);

            // TODO: Call actual email service
            // await _emailService.SendAsync(recipient, subject, body, config.IsHtml);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email notification");
            return false;
        }
    }

    private async Task<bool> SendInAppNotificationAsync(
        InAppNotificationConfig config,
        StepExecutionContext context,
        CancellationToken cancellationToken)
    {
        try
        {
            // Resolve user ID
            int? userId = config.UserId;
            if (!string.IsNullOrEmpty(config.UserIdVariable))
            {
                if (context.Variables.TryGetValue(config.UserIdVariable, out var value))
                {
                    userId = value switch
                    {
                        int i => i,
                        long l => (int)l,
                        string s when int.TryParse(s, out var parsed) => parsed,
                        _ => null
                    };
                }
            }

            // Replace variables in message
            var title = await _expressionEvaluator.ReplaceVariablesAsync(
                config.Title ?? "", context.Variables, cancellationToken);
            
            var message = await _expressionEvaluator.ReplaceVariablesAsync(
                config.Message ?? "", context.Variables, cancellationToken);

            _logger.LogInformation("Sending in-app notification: {Title}", title);

            // TODO: Call actual notification service
            // await _notificationService.SendAsync(userId, title, message, config.Type, config.Link);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send in-app notification");
            return false;
        }
    }

    private async Task<bool> SendWebhookNotificationAsync(
        WebhookNotificationConfig config,
        StepExecutionContext context,
        CancellationToken cancellationToken)
    {
        try
        {
            var url = await _expressionEvaluator.ReplaceVariablesAsync(
                config.Url ?? "", context.Variables, cancellationToken);

            _logger.LogInformation("Sending webhook to {Url}", url);

            // Build payload
            var payload = new Dictionary<string, object?>
            {
                ["workflowInstanceId"] = context.Instance.Id,
                ["workflowName"] = context.Instance.WorkflowDefinition?.Name,
                ["stepKey"] = context.Step.StepKey,
                ["timestamp"] = DateTime.UtcNow,
                ["variables"] = context.Variables
            };

            if (config.IncludePayload != null)
            {
                foreach (var key in config.IncludePayload)
                {
                    if (context.Variables.TryGetValue(key, out var value))
                    {
                        payload[key] = value;
                    }
                }
            }

            // TODO: Make actual HTTP call
            // var client = _httpClientFactory.CreateClient();
            // await client.PostAsJsonAsync(url, payload, cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send webhook notification");
            return false;
        }
    }

    private Task<string?> GetNextStepAsync(
        StepExecutionContext context,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(context.Step.Transitions))
        {
            return Task.FromResult<string?>(null);
        }

        try
        {
            var transitions = JsonSerializer.Deserialize<List<StepTransition>>(context.Step.Transitions);
            var defaultTransition = transitions?.FirstOrDefault(t => string.IsNullOrEmpty(t.Condition));
            return Task.FromResult(defaultTransition?.NextStepKey);
        }
        catch
        {
            return Task.FromResult<string?>(null);
        }
    }
}

#region Notification Config Classes

public class NotificationStepConfig
{
    public EmailNotificationConfig? Email { get; set; }
    public InAppNotificationConfig? InApp { get; set; }
    public WebhookNotificationConfig? Webhook { get; set; }
}

public class EmailNotificationConfig
{
    public string? To { get; set; }
    public string? ToRole { get; set; }
    public string? ToVariable { get; set; }
    public string? Cc { get; set; }
    public string? Subject { get; set; }
    public string? Body { get; set; }
    public bool IsHtml { get; set; } = true;
    public string? TemplateName { get; set; }
}

public class InAppNotificationConfig
{
    public int? UserId { get; set; }
    public string? UserIdVariable { get; set; }
    public string? Role { get; set; }
    public string? Title { get; set; }
    public string? Message { get; set; }
    public string? Type { get; set; } // info, warning, success, error
    public string? Link { get; set; }
    public int? ExpiresInHours { get; set; }
}

public class WebhookNotificationConfig
{
    public string? Url { get; set; }
    public string? Method { get; set; } = "POST";
    public Dictionary<string, string>? Headers { get; set; }
    public List<string>? IncludePayload { get; set; }
    public string? Secret { get; set; }
}

#endregion
