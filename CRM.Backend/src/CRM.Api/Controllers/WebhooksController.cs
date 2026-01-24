using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CRM.Api.Controllers;

/// <summary>
/// Controller for receiving incoming messages from external channels via webhooks
/// These endpoints are called by external services (email servers, social platforms)
/// </summary>
[ApiController]
[Route("api/webhooks")]
public class WebhooksController : ControllerBase
{
    private readonly CrmDbContext _context;
    private readonly ILogger<WebhooksController> _logger;

    public WebhooksController(CrmDbContext context, ILogger<WebhooksController> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region Web Form Submission

    /// <summary>
    /// Receive an interaction from a web form submission
    /// </summary>
    [HttpPost("web-form")]
    public async Task<ActionResult<IngestResultDto>> IngestWebFormSubmission([FromBody] WebFormSubmissionDto dto)
    {
        try
        {
            _logger.LogInformation("Receiving web form submission from {Email}", dto.Email);

            // Try to find or create customer/contact
            var (customerId, contactId) = await FindOrSuggestContactAsync(dto.Email, dto.Phone, dto.Name);

            // Create the interaction
            var interaction = new Interaction
            {
                InteractionType = InteractionType.WebForm,
                Type = "WebForm",
                Direction = InteractionDirection.Inbound,
                Subject = dto.Subject ?? "Web Form Submission",
                Description = BuildWebFormDescription(dto),
                InteractionDate = DateTime.UtcNow,
                CompletedDate = DateTime.UtcNow,
                IsCompleted = true,
                EmailAddress = dto.Email,
                PhoneNumber = dto.Phone,
                Sentiment = InteractionSentiment.Neutral,
                CustomerId = customerId ?? 0,
                ContactId = contactId,
                Tags = dto.FormType,
                CustomFields = dto.CustomFieldsJson,
                CreatedAt = DateTime.UtcNow
            };

            _context.Interactions.Add(interaction);
            await _context.SaveChangesAsync();

            // Also create a communication message for unified inbox
            var channel = await GetOrCreateWebChannel();
            var message = new CommunicationMessage
            {
                ChannelId = channel.Id,
                ChannelType = ChannelType.Email,
                Subject = dto.Subject ?? "Web Form Submission",
                Body = BuildWebFormDescription(dto),
                Direction = MessageDirection.Inbound,
                Status = MessageStatus.Delivered,
                FromAddress = dto.Email,
                FromName = dto.Name,
                ReceivedAt = DateTime.UtcNow,
                CustomerId = customerId,
                ContactId = contactId,
                LinkedEntityType = "Interaction",
                LinkedEntityId = interaction.Id,
                CreatedAt = DateTime.UtcNow
            };

            _context.CommunicationMessages.Add(message);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created interaction {InteractionId} and message {MessageId} from web form", 
                interaction.Id, message.Id);

            return Ok(new IngestResultDto
            {
                Success = true,
                InteractionId = interaction.Id,
                MessageId = message.Id,
                CustomerId = customerId,
                ContactId = contactId,
                Message = "Web form submission received successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing web form submission");
            return StatusCode(500, new IngestResultDto { Success = false, Message = "Error processing submission" });
        }
    }

    #endregion

    #region Email Webhooks

    /// <summary>
    /// Receive an inbound email (from email service webhook)
    /// Supports Exchange, IMAP-based services, SendGrid, Mailgun, etc.
    /// </summary>
    [HttpPost("email/inbound")]
    public async Task<ActionResult<IngestResultDto>> IngestInboundEmail([FromBody] InboundEmailDto dto)
    {
        try
        {
            _logger.LogInformation("Receiving inbound email from {FromEmail} to {ToEmail}", dto.From, dto.To);

            // Find email channel
            var channel = await _context.CommunicationChannels
                .Where(c => c.ChannelType == ChannelType.Email && c.IsEnabled && !c.IsDeleted)
                .FirstOrDefaultAsync();

            if (channel == null)
            {
                channel = await GetOrCreateEmailChannel();
            }

            // Try to find linked customer/contact
            var (customerId, contactId) = await FindOrSuggestContactAsync(dto.From, null, dto.FromName);

            // Check for existing conversation (thread)
            string conversationId = dto.ConversationId ?? dto.InReplyTo ?? Guid.NewGuid().ToString();
            var existingConversation = await _context.Conversations
                .Where(c => c.ConversationId == conversationId && !c.IsDeleted)
                .FirstOrDefaultAsync();

            // Create message
            var message = new CommunicationMessage
            {
                ChannelId = channel.Id,
                ChannelType = ChannelType.Email,
                Subject = dto.Subject,
                Body = dto.TextBody,
                HtmlBody = dto.HtmlBody,
                Direction = MessageDirection.Inbound,
                Status = MessageStatus.Delivered,
                FromAddress = dto.From,
                FromName = dto.FromName,
                ToAddress = dto.To,
                CcAddresses = dto.Cc != null ? JsonSerializer.Serialize(dto.Cc) : null,
                ExternalMessageId = dto.MessageId,
                InReplyToExternalId = dto.InReplyTo,
                ConversationId = conversationId,
                ReceivedAt = dto.ReceivedAt ?? DateTime.UtcNow,
                CustomerId = customerId,
                ContactId = contactId,
                AttachmentCount = dto.Attachments?.Count ?? 0,
                AttachmentsJson = dto.Attachments != null ? JsonSerializer.Serialize(dto.Attachments) : null,
                CreatedAt = DateTime.UtcNow
            };

            _context.CommunicationMessages.Add(message);

            // Create or update conversation
            if (existingConversation != null)
            {
                existingConversation.MessageCount++;
                existingConversation.UnreadCount++;
                existingConversation.LastMessageAt = DateTime.UtcNow;
                existingConversation.LastMessagePreview = TruncateText(dto.TextBody ?? dto.Subject, 100);
                existingConversation.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                var conversation = new Conversation
                {
                    ConversationId = conversationId,
                    PrimaryChannelType = ChannelType.Email,
                    Subject = dto.Subject,
                    LastMessagePreview = TruncateText(dto.TextBody ?? dto.Subject, 100),
                    Status = ConversationStatus.Open,
                    ParticipantAddress = dto.From,
                    ParticipantName = dto.FromName,
                    CustomerId = customerId,
                    ContactId = contactId,
                    MessageCount = 1,
                    UnreadCount = 1,
                    FirstMessageAt = DateTime.UtcNow,
                    LastMessageAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Conversations.Add(conversation);
            }

            // Create interaction record
            var interaction = new Interaction
            {
                InteractionType = InteractionType.Email,
                Type = "Email",
                Direction = InteractionDirection.Inbound,
                Subject = dto.Subject ?? "(No Subject)",
                Description = dto.TextBody ?? "",
                InteractionDate = dto.ReceivedAt ?? DateTime.UtcNow,
                EmailAddress = dto.From,
                IsCompleted = true,
                CompletedDate = DateTime.UtcNow,
                CustomerId = customerId ?? 0,
                ContactId = contactId,
                CreatedAt = DateTime.UtcNow
            };
            _context.Interactions.Add(interaction);

            await _context.SaveChangesAsync();

            // Update linked entity
            message.LinkedEntityType = "Interaction";
            message.LinkedEntityId = interaction.Id;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created email message {MessageId} and interaction {InteractionId}", 
                message.Id, interaction.Id);

            return Ok(new IngestResultDto
            {
                Success = true,
                InteractionId = interaction.Id,
                MessageId = message.Id,
                ConversationId = conversationId,
                CustomerId = customerId,
                ContactId = contactId,
                Message = "Email received successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing inbound email");
            return StatusCode(500, new IngestResultDto { Success = false, Message = "Error processing email" });
        }
    }

    #endregion

    #region Social Media Webhooks

    /// <summary>
    /// Twitter/X webhook for direct messages and mentions
    /// </summary>
    [HttpPost("twitter")]
    public async Task<ActionResult<IngestResultDto>> IngestTwitterMessage([FromBody] SocialMediaMessageDto dto)
    {
        return await IngestSocialMediaMessage(dto, ChannelType.Twitter, InteractionType.SocialMedia);
    }

    /// <summary>
    /// Facebook webhook for messages and comments
    /// </summary>
    [HttpPost("facebook")]
    public async Task<ActionResult<IngestResultDto>> IngestFacebookMessage([FromBody] SocialMediaMessageDto dto)
    {
        return await IngestSocialMediaMessage(dto, ChannelType.Facebook, InteractionType.SocialMedia);
    }

    /// <summary>
    /// Instagram webhook for direct messages
    /// </summary>
    [HttpPost("instagram")]
    public async Task<ActionResult<IngestResultDto>> IngestInstagramMessage([FromBody] SocialMediaMessageDto dto)
    {
        // Instagram uses Facebook's graph API, so we reuse Facebook channel type
        return await IngestSocialMediaMessage(dto, ChannelType.Facebook, InteractionType.SocialMedia);
    }

    /// <summary>
    /// LinkedIn webhook for messages
    /// </summary>
    [HttpPost("linkedin")]
    public async Task<ActionResult<IngestResultDto>> IngestLinkedInMessage([FromBody] SocialMediaMessageDto dto)
    {
        return await IngestSocialMediaMessage(dto, ChannelType.LinkedIn, InteractionType.SocialMedia);
    }

    /// <summary>
    /// WhatsApp Business webhook
    /// </summary>
    [HttpPost("whatsapp")]
    public async Task<ActionResult<IngestResultDto>> IngestWhatsAppMessage([FromBody] WhatsAppMessageDto dto)
    {
        try
        {
            _logger.LogInformation("Receiving WhatsApp message from {Phone}", dto.FromPhone);

            var channel = await _context.CommunicationChannels
                .Where(c => c.ChannelType == ChannelType.WhatsApp && c.IsEnabled && !c.IsDeleted)
                .FirstOrDefaultAsync();

            if (channel == null)
            {
                return BadRequest(new IngestResultDto { Success = false, Message = "WhatsApp channel not configured" });
            }

            // Try to find linked customer/contact by phone
            var (customerId, contactId) = await FindOrSuggestContactAsync(null, dto.FromPhone, dto.FromName);

            var message = new CommunicationMessage
            {
                ChannelId = channel.Id,
                ChannelType = ChannelType.WhatsApp,
                Body = dto.Body,
                Direction = MessageDirection.Inbound,
                Status = MessageStatus.Delivered,
                FromAddress = dto.FromPhone,
                FromName = dto.FromName,
                ExternalMessageId = dto.MessageId,
                ConversationId = dto.ConversationId ?? dto.FromPhone,
                WhatsAppMessageType = dto.MessageType,
                ReceivedAt = dto.Timestamp ?? DateTime.UtcNow,
                CustomerId = customerId,
                ContactId = contactId,
                CreatedAt = DateTime.UtcNow
            };

            _context.CommunicationMessages.Add(message);

            var interaction = new Interaction
            {
                InteractionType = InteractionType.Chat,
                Type = "WhatsApp",
                Direction = InteractionDirection.Inbound,
                Subject = "WhatsApp Message",
                Description = dto.Body ?? "",
                InteractionDate = dto.Timestamp ?? DateTime.UtcNow,
                PhoneNumber = dto.FromPhone,
                IsCompleted = true,
                CustomerId = customerId ?? 0,
                ContactId = contactId,
                CreatedAt = DateTime.UtcNow
            };
            _context.Interactions.Add(interaction);

            await _context.SaveChangesAsync();

            return Ok(new IngestResultDto
            {
                Success = true,
                InteractionId = interaction.Id,
                MessageId = message.Id,
                CustomerId = customerId,
                ContactId = contactId,
                Message = "WhatsApp message received successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing WhatsApp message");
            return StatusCode(500, new IngestResultDto { Success = false, Message = "Error processing message" });
        }
    }

    /// <summary>
    /// Webhook verification endpoint (GET) for Facebook/Twitter/WhatsApp verification
    /// </summary>
    [HttpGet("verify")]
    public ActionResult VerifyWebhook(
        [FromQuery(Name = "hub.mode")] string? hubMode,
        [FromQuery(Name = "hub.verify_token")] string? hubVerifyToken,
        [FromQuery(Name = "hub.challenge")] string? hubChallenge)
    {
        // For Facebook/Instagram verification
        if (hubMode == "subscribe" && !string.IsNullOrEmpty(hubChallenge))
        {
            // TODO: Verify token against stored webhook secret
            _logger.LogInformation("Webhook verification request received");
            return Ok(hubChallenge);
        }

        return BadRequest("Verification failed");
    }

    #endregion

    #region Helper Methods

    private async Task<ActionResult<IngestResultDto>> IngestSocialMediaMessage(
        SocialMediaMessageDto dto, 
        ChannelType channelType,
        InteractionType interactionType)
    {
        try
        {
            _logger.LogInformation("Receiving {ChannelType} message from {Handle}", channelType, dto.SenderHandle);

            var channel = await _context.CommunicationChannels
                .Where(c => c.ChannelType == channelType && c.IsEnabled && !c.IsDeleted)
                .FirstOrDefaultAsync();

            if (channel == null)
            {
                return BadRequest(new IngestResultDto 
                { 
                    Success = false, 
                    Message = $"{channelType} channel not configured" 
                });
            }

            // Try to find linked customer by social handle
            var (customerId, contactId) = await FindContactBySocialHandle(dto.SenderHandle, channelType);

            var message = new CommunicationMessage
            {
                ChannelId = channel.Id,
                ChannelType = channelType,
                Body = dto.Text,
                Direction = MessageDirection.Inbound,
                Status = MessageStatus.Delivered,
                FromAddress = dto.SenderHandle,
                FromName = dto.SenderName,
                ExternalMessageId = dto.PostId ?? dto.MessageId,
                SocialPostId = dto.PostId,
                IsPublicPost = dto.IsPublicPost,
                ReceivedAt = dto.Timestamp ?? DateTime.UtcNow,
                CustomerId = customerId,
                ContactId = contactId,
                MetadataJson = JsonSerializer.Serialize(new
                {
                    dto.ParentPostId,
                    dto.MediaUrls,
                    dto.Hashtags,
                    dto.Mentions
                }),
                CreatedAt = DateTime.UtcNow
            };

            _context.CommunicationMessages.Add(message);

            var interaction = new Interaction
            {
                InteractionType = interactionType,
                Type = channelType.ToString(),
                Direction = InteractionDirection.Inbound,
                Subject = $"{channelType} {(dto.IsPublicPost ? "Post" : "Message")}",
                Description = dto.Text ?? "",
                InteractionDate = dto.Timestamp ?? DateTime.UtcNow,
                IsCompleted = true,
                CustomerId = customerId ?? 0,
                ContactId = contactId,
                Tags = dto.Hashtags != null ? string.Join(",", dto.Hashtags) : null,
                CreatedAt = DateTime.UtcNow
            };
            _context.Interactions.Add(interaction);

            await _context.SaveChangesAsync();

            return Ok(new IngestResultDto
            {
                Success = true,
                InteractionId = interaction.Id,
                MessageId = message.Id,
                CustomerId = customerId,
                ContactId = contactId,
                Message = $"{channelType} message received successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing {ChannelType} message", channelType);
            return StatusCode(500, new IngestResultDto { Success = false, Message = "Error processing message" });
        }
    }

    private async Task<(int? CustomerId, int? ContactId)> FindOrSuggestContactAsync(
        string? email, string? phone, string? name)
    {
        // Try to find by email first
        if (!string.IsNullOrEmpty(email))
        {
            var customer = await _context.Customers
                .Where(c => c.Email == email && !c.IsDeleted)
                .FirstOrDefaultAsync();
            if (customer != null)
                return (customer.Id, null);

            var contact = await _context.Contacts
                .Where(c => c.Email == email && !c.IsDeleted)
                .FirstOrDefaultAsync();
            if (contact != null)
                return (contact.CustomerId, contact.Id);
        }

        // Try by phone
        if (!string.IsNullOrEmpty(phone))
        {
            var normalizedPhone = NormalizePhone(phone);
            var customer = await _context.Customers
                .Where(c => (c.Phone == phone || c.Phone == normalizedPhone || 
                           c.MobilePhone == phone || c.MobilePhone == normalizedPhone) && !c.IsDeleted)
                .FirstOrDefaultAsync();
            if (customer != null)
                return (customer.Id, null);

            var contact = await _context.Contacts
                .Where(c => (c.Phone == phone || c.Phone == normalizedPhone || 
                           c.MobilePhone == phone || c.MobilePhone == normalizedPhone) && !c.IsDeleted)
                .FirstOrDefaultAsync();
            if (contact != null)
                return (contact.CustomerId, contact.Id);
        }

        return (null, null);
    }

    private async Task<(int? CustomerId, int? ContactId)> FindContactBySocialHandle(
        string handle, ChannelType channelType)
    {
        var network = channelType switch
        {
            ChannelType.Twitter => SocialNetwork.Twitter,
            ChannelType.Facebook => SocialNetwork.Facebook,
            ChannelType.LinkedIn => SocialNetwork.LinkedIn,
            _ => SocialNetwork.Other
        };

        // This would need a proper social accounts linkage table
        // For now, try to match against customer social fields
        var customer = await _context.Customers
            .Where(c => c.TwitterHandle == handle || c.LinkedInUrl!.Contains(handle))
            .FirstOrDefaultAsync();

        if (customer != null)
            return (customer.Id, null);

        return (null, null);
    }

    private async Task<CommunicationChannel> GetOrCreateWebChannel()
    {
        var channel = await _context.CommunicationChannels
            .Where(c => c.Name == "Web Forms" && !c.IsDeleted)
            .FirstOrDefaultAsync();

        if (channel == null)
        {
            channel = new CommunicationChannel
            {
                Name = "Web Forms",
                ChannelType = ChannelType.Email,
                Status = ChannelStatus.Connected,
                IsEnabled = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.CommunicationChannels.Add(channel);
            await _context.SaveChangesAsync();
        }

        return channel;
    }

    private async Task<CommunicationChannel> GetOrCreateEmailChannel()
    {
        var channel = await _context.CommunicationChannels
            .Where(c => c.ChannelType == ChannelType.Email && c.IsDefault && !c.IsDeleted)
            .FirstOrDefaultAsync();

        if (channel == null)
        {
            channel = new CommunicationChannel
            {
                Name = "Default Email",
                ChannelType = ChannelType.Email,
                Status = ChannelStatus.Configured,
                IsEnabled = true,
                IsDefault = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.CommunicationChannels.Add(channel);
            await _context.SaveChangesAsync();
        }

        return channel;
    }

    private string BuildWebFormDescription(WebFormSubmissionDto dto)
    {
        var lines = new List<string>();
        if (!string.IsNullOrEmpty(dto.Name)) lines.Add($"Name: {dto.Name}");
        if (!string.IsNullOrEmpty(dto.Email)) lines.Add($"Email: {dto.Email}");
        if (!string.IsNullOrEmpty(dto.Phone)) lines.Add($"Phone: {dto.Phone}");
        if (!string.IsNullOrEmpty(dto.Company)) lines.Add($"Company: {dto.Company}");
        if (!string.IsNullOrEmpty(dto.Message)) lines.Add($"\nMessage:\n{dto.Message}");
        if (dto.FormFields != null)
        {
            lines.Add("\nAdditional Fields:");
            foreach (var field in dto.FormFields)
            {
                lines.Add($"  {field.Key}: {field.Value}");
            }
        }
        return string.Join("\n", lines);
    }

    private string NormalizePhone(string phone)
    {
        return new string(phone.Where(char.IsDigit).ToArray());
    }

    private string TruncateText(string? text, int maxLength)
    {
        if (string.IsNullOrEmpty(text)) return "";
        return text.Length <= maxLength ? text : text.Substring(0, maxLength) + "...";
    }

    #endregion
}

#region Webhook DTOs

public class WebFormSubmissionDto
{
    public string? Name { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Company { get; set; }
    public string? Subject { get; set; }
    public string? Message { get; set; }
    public string? FormType { get; set; }
    public string? Source { get; set; }
    public string? PageUrl { get; set; }
    public Dictionary<string, string>? FormFields { get; set; }
    public string? CustomFieldsJson { get; set; }
}

public class InboundEmailDto
{
    public string From { get; set; } = string.Empty;
    public string? FromName { get; set; }
    public string To { get; set; } = string.Empty;
    public List<string>? Cc { get; set; }
    public string? Subject { get; set; }
    public string? TextBody { get; set; }
    public string? HtmlBody { get; set; }
    public string? MessageId { get; set; }
    public string? InReplyTo { get; set; }
    public string? ConversationId { get; set; }
    public DateTime? ReceivedAt { get; set; }
    public List<EmailAttachmentDto>? Attachments { get; set; }
}

public class EmailAttachmentDto
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Size { get; set; }
    public string? Url { get; set; }
    public string? ContentId { get; set; }
}

public class SocialMediaMessageDto
{
    public string? MessageId { get; set; }
    public string? PostId { get; set; }
    public string? ParentPostId { get; set; }
    public string SenderHandle { get; set; } = string.Empty;
    public string? SenderName { get; set; }
    public string? SenderId { get; set; }
    public string? Text { get; set; }
    public bool IsPublicPost { get; set; }
    public bool IsDirectMessage { get; set; }
    public List<string>? MediaUrls { get; set; }
    public List<string>? Hashtags { get; set; }
    public List<string>? Mentions { get; set; }
    public DateTime? Timestamp { get; set; }
}

public class WhatsAppMessageDto
{
    public string MessageId { get; set; } = string.Empty;
    public string FromPhone { get; set; } = string.Empty;
    public string? FromName { get; set; }
    public string? Body { get; set; }
    public string? MessageType { get; set; }
    public string? MediaUrl { get; set; }
    public string? ConversationId { get; set; }
    public DateTime? Timestamp { get; set; }
}

public class IngestResultDto
{
    public bool Success { get; set; }
    public int? InteractionId { get; set; }
    public int? MessageId { get; set; }
    public string? ConversationId { get; set; }
    public int? CustomerId { get; set; }
    public int? ContactId { get; set; }
    public string? Message { get; set; }
}

#endregion
