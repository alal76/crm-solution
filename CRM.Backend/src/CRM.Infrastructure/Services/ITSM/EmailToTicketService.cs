// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Text.RegularExpressions;
using CRM.Core.Interfaces.ITSM;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.ITSM;

/// <summary>
/// Service for parsing inbound emails and creating/updating incidents.
/// </summary>
public class EmailToTicketService : IEmailToTicketService
{
    private readonly ILogger<EmailToTicketService> _logger;
    private static readonly Regex IncidentReferencePattern = new(@"\[INC-(\d+)\]", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex EmailQuotePattern = new(@"^>.*$", RegexOptions.Multiline | RegexOptions.Compiled);
    private static readonly Regex SignaturePattern = new(@"(^--\s*$|^Sent from my|^Best regards|^Thanks,|^Regards,)", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // In-memory config for demo; would be stored in database in production
    private EmailParsingConfigDto _config = new()
    {
        IsEnabled = true,
        DefaultCategory = "Email",
        DefaultPriority = 3,
        AutoDetectCustomer = true,
        CreateCustomerIfNotFound = false,
        AttachOriginalEmail = true,
        MaxAttachmentSizeMB = 10,
        PriorityKeywords = new Dictionary<string, int>
        {
            { "urgent", 1 },
            { "critical", 1 },
            { "emergency", 1 },
            { "asap", 2 },
            { "important", 2 },
            { "high priority", 2 }
        }
    };

    public EmailToTicketService(ILogger<EmailToTicketService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<EmailParseResult> ParseAndCreateIncidentAsync(InboundEmailDto email)
    {
        try
        {
            _logger.LogInformation("Parsing inbound email from {From} with subject: {Subject}", email.From, email.Subject);

            if (!_config.IsEnabled)
            {
                return new EmailParseResult
                {
                    Success = false,
                    ErrorMessage = "Email-to-ticket parsing is disabled",
                    Action = EmailParseAction.Ignored
                };
            }

            // Check for blocked domains
            var fromDomain = ExtractDomain(email.From);
            if (_config.BlockedDomains.Any(d => d.Equals(fromDomain, StringComparison.OrdinalIgnoreCase)))
            {
                _logger.LogWarning("Email from blocked domain: {Domain}", fromDomain);
                return new EmailParseResult
                {
                    Success = false,
                    ErrorMessage = "Email from blocked domain",
                    Action = EmailParseAction.Ignored
                };
            }

            // Check allowed domains if configured
            if (_config.AllowedDomains.Any() && !_config.AllowedDomains.Any(d => d.Equals(fromDomain, StringComparison.OrdinalIgnoreCase)))
            {
                _logger.LogWarning("Email from non-allowed domain: {Domain}", fromDomain);
                return new EmailParseResult
                {
                    Success = false,
                    ErrorMessage = "Email from non-allowed domain",
                    Action = EmailParseAction.Ignored
                };
            }

            // Check for ignored subject patterns
            foreach (var pattern in _config.IgnoreSubjectPatterns)
            {
                if (Regex.IsMatch(email.Subject, pattern, RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1)))
                {
                    _logger.LogInformation("Email subject matches ignore pattern: {Pattern}", pattern);
                    return new EmailParseResult
                    {
                        Success = true,
                        Action = EmailParseAction.Ignored
                    };
                }
            }

            // Create incident (simulated - would use IncidentService in production)
            var incidentNumber = $"INC-{DateTime.UtcNow:yyyyMMddHHmmss}";
            var incidentId = Random.Shared.Next(10000, 99999);

            _logger.LogInformation("Created incident {IncidentNumber} from email", incidentNumber);

            return await Task.FromResult(new EmailParseResult
            {
                Success = true,
                IncidentId = incidentId,
                IncidentNumber = incidentNumber,
                Action = EmailParseAction.IncidentCreated
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse email and create incident");
            return new EmailParseResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                Action = EmailParseAction.Failed
            };
        }
    }

    /// <inheritdoc />
    public async Task<EmailParseResult> ParseAndUpdateIncidentAsync(InboundEmailDto email, int incidentId)
    {
        try
        {
            _logger.LogInformation("Updating incident {IncidentId} from email reply", incidentId);

            if (!_config.IsEnabled)
            {
                return new EmailParseResult
                {
                    Success = false,
                    ErrorMessage = "Email-to-ticket parsing is disabled",
                    Action = EmailParseAction.Ignored
                };
            }

            // Add comment to incident (simulated)
            var commentId = Random.Shared.Next(1000, 9999);

            _logger.LogInformation("Added comment {CommentId} to incident {IncidentId}", commentId, incidentId);

            return await Task.FromResult(new EmailParseResult
            {
                Success = true,
                IncidentId = incidentId,
                Action = EmailParseAction.CommentAdded,
                CommentId = commentId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update incident from email");
            return new EmailParseResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                Action = EmailParseAction.Failed
            };
        }
    }

    /// <inheritdoc />
    public int? ExtractIncidentReference(string subject)
    {
        if (string.IsNullOrWhiteSpace(subject))
        {
            return null;
        }

        var match = IncidentReferencePattern.Match(subject);
        if (match.Success && int.TryParse(match.Groups[1].Value, out var incidentId))
        {
            return incidentId;
        }

        return null;
    }

    /// <inheritdoc />
    public Task<EmailParsingConfigDto> GetConfigurationAsync()
    {
        return Task.FromResult(_config);
    }

    /// <inheritdoc />
    public Task UpdateConfigurationAsync(EmailParsingConfigDto config)
    {
        _config = config;
        _logger.LogInformation("Email parsing configuration updated");
        return Task.CompletedTask;
    }

    private int DeterminePriority(string subject, string body)
    {
        var combinedText = $"{subject} {body}".ToLower();

        foreach (var keyword in _config.PriorityKeywords.OrderBy(k => k.Value))
        {
            if (combinedText.Contains(keyword.Key.ToLower()))
            {
                _logger.LogDebug("Detected priority keyword '{Keyword}', setting priority to {Priority}", keyword.Key, keyword.Value);
                return keyword.Value;
            }
        }

        return _config.DefaultPriority;
    }

    private static string CleanEmailBody(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return string.Empty;
        }

        // Remove quoted lines
        var cleaned = EmailQuotePattern.Replace(body, string.Empty);

        // Find and remove signature
        var signatureMatch = SignaturePattern.Match(cleaned);
        if (signatureMatch.Success)
        {
            cleaned = cleaned.Substring(0, signatureMatch.Index);
        }

        // Clean up extra whitespace
        cleaned = Regex.Replace(cleaned, @"\n{3,}", "\n\n", RegexOptions.None, TimeSpan.FromSeconds(1));
        cleaned = cleaned.Trim();

        return cleaned;
    }

    private static string ExtractDomain(string email)
    {
        var atIndex = email.IndexOf('@');
        if (atIndex < 0 || atIndex >= email.Length - 1)
        {
            return string.Empty;
        }

        var domain = email.Substring(atIndex + 1);
        var gtIndex = domain.IndexOf('>');
        if (gtIndex > 0)
        {
            domain = domain.Substring(0, gtIndex);
        }

        return domain.Trim().ToLower();
    }
}
