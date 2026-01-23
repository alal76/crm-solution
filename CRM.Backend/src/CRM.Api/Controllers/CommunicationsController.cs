using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CRM.Api.Controllers;

/// <summary>
/// Controller for managing communication channels and unified messaging
/// Supports Email, WhatsApp, X (Twitter), and Facebook
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CommunicationsController : ControllerBase
{
    private readonly CrmDbContext _context;
    private readonly ILogger<CommunicationsController> _logger;

    public CommunicationsController(CrmDbContext context, ILogger<CommunicationsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region Communication Channels

    /// <summary>
    /// Get all configured communication channels
    /// </summary>
    [HttpGet("channels")]
    public async Task<ActionResult<List<CommunicationChannelListDto>>> GetChannels()
    {
        try
        {
            var channels = await _context.CommunicationChannels
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.ChannelType)
                .ThenBy(c => c.Name)
                .Select(c => new CommunicationChannelListDto
                {
                    Id = c.Id,
                    ChannelType = c.ChannelType.ToString(),
                    Name = c.Name,
                    Status = c.Status.ToString(),
                    IsEnabled = c.IsEnabled,
                    IsDefault = c.IsDefault,
                    SocialUsername = c.SocialUsername,
                    FromEmail = c.FromEmail,
                    LastConnectedAt = c.LastConnectedAt,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            return Ok(channels);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting communication channels");
            return StatusCode(500, new { message = "Error retrieving channels" });
        }
    }

    /// <summary>
    /// Get a specific channel by ID
    /// </summary>
    [HttpGet("channels/{id}")]
    public async Task<ActionResult<CommunicationChannelDto>> GetChannel(int id)
    {
        try
        {
            var channel = await _context.CommunicationChannels
                .Where(c => c.Id == id && !c.IsDeleted)
                .Select(c => new CommunicationChannelDto
                {
                    Id = c.Id,
                    ChannelType = c.ChannelType.ToString(),
                    Name = c.Name,
                    Status = c.Status.ToString(),
                    IsEnabled = c.IsEnabled,
                    IsDefault = c.IsDefault,
                    SmtpServer = c.SmtpServer,
                    SmtpPort = c.SmtpPort,
                    SmtpUseSsl = c.SmtpUseSsl,
                    SmtpUsername = c.SmtpUsername,
                    ImapServer = c.ImapServer,
                    ImapPort = c.ImapPort,
                    ImapUseSsl = c.ImapUseSsl,
                    FromEmail = c.FromEmail,
                    FromName = c.FromName,
                    WhatsAppBusinessAccountId = c.WhatsAppBusinessAccountId,
                    WhatsAppPhoneNumberId = c.WhatsAppPhoneNumberId,
                    SocialAccountId = c.SocialAccountId,
                    SocialUsername = c.SocialUsername,
                    WebhookUrl = c.WebhookUrl,
                    WebhookEnabled = c.WebhookEnabled,
                    LastConnectedAt = c.LastConnectedAt,
                    LastError = c.LastError,
                    CreatedAt = c.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (channel == null)
                return NotFound(new { message = "Channel not found" });

            return Ok(channel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting channel {ChannelId}", id);
            return StatusCode(500, new { message = "Error retrieving channel" });
        }
    }

    /// <summary>
    /// Create a new communication channel
    /// </summary>
    [HttpPost("channels")]
    public async Task<ActionResult<CommunicationChannelDto>> CreateChannel([FromBody] CommunicationChannelCreateDto dto)
    {
        try
        {
            if (!Enum.TryParse<ChannelType>(dto.ChannelType, true, out var channelType))
                return BadRequest(new { message = "Invalid channel type" });

            // If setting as default, unset other defaults of same type
            if (dto.IsDefault)
            {
                var existingDefaults = await _context.CommunicationChannels
                    .Where(c => c.ChannelType == channelType && c.IsDefault && !c.IsDeleted)
                    .ToListAsync();
                foreach (var c in existingDefaults)
                    c.IsDefault = false;
            }

            var channel = new CommunicationChannel
            {
                ChannelType = channelType,
                Name = dto.Name,
                IsEnabled = dto.IsEnabled,
                IsDefault = dto.IsDefault,
                Status = ChannelStatus.Configured,
                ApiKey = dto.ApiKey,
                ApiSecret = dto.ApiSecret,
                ClientId = dto.ClientId,
                ClientSecret = dto.ClientSecret,
                AccessToken = dto.AccessToken,
                RefreshToken = dto.RefreshToken,
                SmtpServer = dto.SmtpServer,
                SmtpPort = dto.SmtpPort,
                SmtpUseSsl = dto.SmtpUseSsl,
                SmtpUsername = dto.SmtpUsername,
                SmtpPassword = dto.SmtpPassword,
                ImapServer = dto.ImapServer,
                ImapPort = dto.ImapPort,
                ImapUseSsl = dto.ImapUseSsl,
                FromEmail = dto.FromEmail,
                FromName = dto.FromName,
                WhatsAppBusinessAccountId = dto.WhatsAppBusinessAccountId,
                WhatsAppPhoneNumberId = dto.WhatsAppPhoneNumberId,
                WhatsAppVerifyToken = dto.WhatsAppVerifyToken,
                SocialAccountId = dto.SocialAccountId,
                SocialUsername = dto.SocialUsername,
                PageAccessToken = dto.PageAccessToken,
                CreatedAt = DateTime.UtcNow
            };

            _context.CommunicationChannels.Add(channel);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created communication channel {ChannelName} of type {ChannelType}", channel.Name, channel.ChannelType);

            return CreatedAtAction(nameof(GetChannel), new { id = channel.Id }, new CommunicationChannelDto
            {
                Id = channel.Id,
                ChannelType = channel.ChannelType.ToString(),
                Name = channel.Name,
                Status = channel.Status.ToString(),
                IsEnabled = channel.IsEnabled,
                IsDefault = channel.IsDefault,
                CreatedAt = channel.CreatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating communication channel");
            return StatusCode(500, new { message = "Error creating channel" });
        }
    }

    /// <summary>
    /// Update a communication channel
    /// </summary>
    [HttpPut("channels/{id}")]
    public async Task<ActionResult<CommunicationChannelDto>> UpdateChannel(int id, [FromBody] CommunicationChannelCreateDto dto)
    {
        try
        {
            var channel = await _context.CommunicationChannels.FindAsync(id);
            if (channel == null || channel.IsDeleted)
                return NotFound(new { message = "Channel not found" });

            if (!Enum.TryParse<ChannelType>(dto.ChannelType, true, out var channelType))
                return BadRequest(new { message = "Invalid channel type" });

            // If setting as default, unset other defaults of same type
            if (dto.IsDefault && !channel.IsDefault)
            {
                var existingDefaults = await _context.CommunicationChannels
                    .Where(c => c.ChannelType == channelType && c.IsDefault && !c.IsDeleted && c.Id != id)
                    .ToListAsync();
                foreach (var c in existingDefaults)
                    c.IsDefault = false;
            }

            channel.ChannelType = channelType;
            channel.Name = dto.Name;
            channel.IsEnabled = dto.IsEnabled;
            channel.IsDefault = dto.IsDefault;
            channel.SmtpServer = dto.SmtpServer;
            channel.SmtpPort = dto.SmtpPort;
            channel.SmtpUseSsl = dto.SmtpUseSsl;
            channel.SmtpUsername = dto.SmtpUsername;
            channel.ImapServer = dto.ImapServer;
            channel.ImapPort = dto.ImapPort;
            channel.ImapUseSsl = dto.ImapUseSsl;
            channel.FromEmail = dto.FromEmail;
            channel.FromName = dto.FromName;
            channel.WhatsAppBusinessAccountId = dto.WhatsAppBusinessAccountId;
            channel.WhatsAppPhoneNumberId = dto.WhatsAppPhoneNumberId;
            channel.SocialAccountId = dto.SocialAccountId;
            channel.SocialUsername = dto.SocialUsername;
            channel.UpdatedAt = DateTime.UtcNow;

            // Only update credentials if provided (non-null)
            if (!string.IsNullOrEmpty(dto.ApiKey)) channel.ApiKey = dto.ApiKey;
            if (!string.IsNullOrEmpty(dto.ApiSecret)) channel.ApiSecret = dto.ApiSecret;
            if (!string.IsNullOrEmpty(dto.ClientId)) channel.ClientId = dto.ClientId;
            if (!string.IsNullOrEmpty(dto.ClientSecret)) channel.ClientSecret = dto.ClientSecret;
            if (!string.IsNullOrEmpty(dto.AccessToken)) channel.AccessToken = dto.AccessToken;
            if (!string.IsNullOrEmpty(dto.RefreshToken)) channel.RefreshToken = dto.RefreshToken;
            if (!string.IsNullOrEmpty(dto.SmtpPassword)) channel.SmtpPassword = dto.SmtpPassword;
            if (!string.IsNullOrEmpty(dto.PageAccessToken)) channel.PageAccessToken = dto.PageAccessToken;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated communication channel {ChannelId}", id);

            return Ok(await GetChannel(id));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating channel {ChannelId}", id);
            return StatusCode(500, new { message = "Error updating channel" });
        }
    }

    /// <summary>
    /// Delete a communication channel
    /// </summary>
    [HttpDelete("channels/{id}")]
    public async Task<ActionResult> DeleteChannel(int id)
    {
        try
        {
            var channel = await _context.CommunicationChannels.FindAsync(id);
            if (channel == null || channel.IsDeleted)
                return NotFound(new { message = "Channel not found" });

            channel.IsDeleted = true;
            channel.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted communication channel {ChannelId}", id);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting channel {ChannelId}", id);
            return StatusCode(500, new { message = "Error deleting channel" });
        }
    }

    /// <summary>
    /// Test channel connection
    /// </summary>
    [HttpPost("channels/{id}/test")]
    public async Task<ActionResult> TestChannel(int id)
    {
        try
        {
            var channel = await _context.CommunicationChannels.FindAsync(id);
            if (channel == null || channel.IsDeleted)
                return NotFound(new { message = "Channel not found" });

            // TODO: Implement actual connection testing for each channel type
            // For now, just mark as connected
            channel.Status = ChannelStatus.Connected;
            channel.LastConnectedAt = DateTime.UtcNow;
            channel.LastError = null;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Connection test successful" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing channel {ChannelId}", id);
            
            var channel = await _context.CommunicationChannels.FindAsync(id);
            if (channel != null)
            {
                channel.Status = ChannelStatus.Error;
                channel.LastError = ex.Message;
                await _context.SaveChangesAsync();
            }

            return StatusCode(500, new { success = false, message = "Connection test failed", error = ex.Message });
        }
    }

    #endregion

    #region Messages

    /// <summary>
    /// Get messages with optional filtering
    /// </summary>
    [HttpGet("messages")]
    public async Task<ActionResult<List<CommunicationMessageListDto>>> GetMessages(
        [FromQuery] string? channelType = null,
        [FromQuery] string? direction = null,
        [FromQuery] string? status = null,
        [FromQuery] bool? unreadOnly = null,
        [FromQuery] int? customerId = null,
        [FromQuery] int? contactId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            var query = _context.CommunicationMessages
                .Where(m => !m.IsDeleted && !m.IsArchived);

            if (!string.IsNullOrEmpty(channelType) && Enum.TryParse<ChannelType>(channelType, true, out var ct))
                query = query.Where(m => m.ChannelType == ct);

            if (!string.IsNullOrEmpty(direction) && Enum.TryParse<MessageDirection>(direction, true, out var dir))
                query = query.Where(m => m.Direction == dir);

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<MessageStatus>(status, true, out var st))
                query = query.Where(m => m.Status == st);

            if (unreadOnly == true)
                query = query.Where(m => !m.IsRead);

            if (customerId.HasValue)
                query = query.Where(m => m.CustomerId == customerId);

            if (contactId.HasValue)
                query = query.Where(m => m.ContactId == contactId);

            var messages = await query
                .OrderByDescending(m => m.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new CommunicationMessageListDto
                {
                    Id = m.Id,
                    ChannelType = m.ChannelType.ToString(),
                    Subject = m.Subject,
                    BodyPreview = m.Body != null ? (m.Body.Length > 100 ? m.Body.Substring(0, 100) + "..." : m.Body) : null,
                    Direction = m.Direction.ToString(),
                    Status = m.Status.ToString(),
                    FromAddress = m.FromAddress,
                    FromName = m.FromName,
                    ToAddress = m.ToAddress,
                    ToName = m.ToName,
                    AttachmentCount = m.AttachmentCount,
                    IsRead = m.IsRead,
                    IsStarred = m.IsStarred,
                    SentAt = m.SentAt,
                    ReceivedAt = m.ReceivedAt,
                    CreatedAt = m.CreatedAt,
                    CustomerId = m.CustomerId,
                    ContactId = m.ContactId
                })
                .ToListAsync();

            return Ok(messages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting messages");
            return StatusCode(500, new { message = "Error retrieving messages" });
        }
    }

    /// <summary>
    /// Get a specific message by ID
    /// </summary>
    [HttpGet("messages/{id}")]
    public async Task<ActionResult<CommunicationMessageDto>> GetMessage(int id)
    {
        try
        {
            var message = await _context.CommunicationMessages
                .Include(m => m.Channel)
                .Where(m => m.Id == id && !m.IsDeleted)
                .FirstOrDefaultAsync();

            if (message == null)
                return NotFound(new { message = "Message not found" });

            // Mark as read
            if (!message.IsRead)
            {
                message.IsRead = true;
                await _context.SaveChangesAsync();
            }

            var dto = new CommunicationMessageDto
            {
                Id = message.Id,
                ChannelId = message.ChannelId,
                ChannelType = message.ChannelType.ToString(),
                ChannelName = message.Channel?.Name ?? string.Empty,
                Subject = message.Subject,
                Body = message.Body,
                HtmlBody = message.HtmlBody,
                Direction = message.Direction.ToString(),
                Status = message.Status.ToString(),
                Priority = message.Priority.ToString(),
                FromAddress = message.FromAddress,
                FromName = message.FromName,
                ToAddress = message.ToAddress,
                ToName = message.ToName,
                AttachmentCount = message.AttachmentCount,
                ConversationId = message.ConversationId,
                ParentMessageId = message.ParentMessageId,
                CustomerId = message.CustomerId,
                ContactId = message.ContactId,
                LeadId = message.LeadId,
                OpportunityId = message.OpportunityId,
                SentAt = message.SentAt,
                DeliveredAt = message.DeliveredAt,
                ReadAt = message.ReadAt,
                ReceivedAt = message.ReceivedAt,
                CreatedAt = message.CreatedAt,
                OpenCount = message.OpenCount,
                ClickCount = message.ClickCount,
                IsPublicPost = message.IsPublicPost,
                LikeCount = message.LikeCount,
                ShareCount = message.ShareCount,
                CommentCount = message.CommentCount,
                IsRead = message.IsRead,
                IsStarred = message.IsStarred,
                IsArchived = message.IsArchived,
                ErrorMessage = message.ErrorMessage
            };

            // Parse CC/BCC if present
            if (!string.IsNullOrEmpty(message.CcAddresses))
                dto.CcAddresses = JsonSerializer.Deserialize<List<string>>(message.CcAddresses);
            if (!string.IsNullOrEmpty(message.BccAddresses))
                dto.BccAddresses = JsonSerializer.Deserialize<List<string>>(message.BccAddresses);
            if (!string.IsNullOrEmpty(message.AttachmentsJson))
                dto.Attachments = JsonSerializer.Deserialize<List<MessageAttachmentDto>>(message.AttachmentsJson);

            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting message {MessageId}", id);
            return StatusCode(500, new { message = "Error retrieving message" });
        }
    }

    /// <summary>
    /// Send a new message
    /// </summary>
    [HttpPost("messages/send")]
    public async Task<ActionResult<CommunicationMessageDto>> SendMessage([FromBody] SendMessageDto dto)
    {
        try
        {
            var channel = await _context.CommunicationChannels.FindAsync(dto.ChannelId);
            if (channel == null || channel.IsDeleted || !channel.IsEnabled)
                return BadRequest(new { message = "Channel not found or not enabled" });

            var message = new CommunicationMessage
            {
                ChannelId = dto.ChannelId,
                ChannelType = channel.ChannelType,
                Subject = dto.Subject,
                Body = dto.Body,
                HtmlBody = dto.HtmlBody,
                Direction = MessageDirection.Outbound,
                Status = dto.ScheduledAt.HasValue ? MessageStatus.Queued : MessageStatus.Sending,
                Priority = Enum.TryParse<MessagePriority>(dto.Priority, true, out var priority) ? priority : MessagePriority.Normal,
                FromAddress = channel.FromEmail ?? channel.SocialUsername,
                FromName = channel.FromName,
                ToAddress = dto.ToAddress,
                ToName = dto.ToName,
                CcAddresses = dto.CcAddresses != null ? JsonSerializer.Serialize(dto.CcAddresses) : null,
                BccAddresses = dto.BccAddresses != null ? JsonSerializer.Serialize(dto.BccAddresses) : null,
                TrackOpens = dto.TrackOpens,
                TrackClicks = dto.TrackClicks,
                EmailTemplateId = dto.EmailTemplateId,
                CustomerId = dto.CustomerId,
                ContactId = dto.ContactId,
                LeadId = dto.LeadId,
                OpportunityId = dto.OpportunityId,
                ParentMessageId = dto.ParentMessageId,
                ConversationId = dto.ConversationId ?? Guid.NewGuid().ToString(),
                ScheduledAt = dto.ScheduledAt,
                CreatedAt = DateTime.UtcNow
            };

            // TODO: Implement actual message sending via external services
            // For now, simulate immediate send
            if (!dto.ScheduledAt.HasValue)
            {
                message.Status = MessageStatus.Sent;
                message.SentAt = DateTime.UtcNow;
            }

            _context.CommunicationMessages.Add(message);
            await _context.SaveChangesAsync();

            // Create activity record
            var activity = new Activity
            {
                ActivityType = channel.ChannelType == ChannelType.Email ? ActivityType.EmailSent : ActivityType.ChatMessage,
                Title = $"Message sent via {channel.ChannelType}",
                Description = dto.Subject ?? dto.Body?.Substring(0, Math.Min(100, dto.Body.Length)),
                EntityType = dto.CustomerId.HasValue ? "Customer" : (dto.ContactId.HasValue ? "Contact" : (dto.LeadId.HasValue ? "Lead" : null)),
                EntityId = dto.CustomerId ?? dto.ContactId ?? dto.LeadId,
                ActivityDate = DateTime.UtcNow
            };
            _context.Activities.Add(activity);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Sent message {MessageId} via {ChannelType}", message.Id, channel.ChannelType);

            return CreatedAtAction(nameof(GetMessage), new { id = message.Id }, await GetMessage(message.Id));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message");
            return StatusCode(500, new { message = "Error sending message" });
        }
    }

    /// <summary>
    /// Mark message as read/unread
    /// </summary>
    [HttpPatch("messages/{id}/read")]
    public async Task<ActionResult> MarkMessageRead(int id, [FromQuery] bool isRead = true)
    {
        try
        {
            var message = await _context.CommunicationMessages.FindAsync(id);
            if (message == null || message.IsDeleted)
                return NotFound(new { message = "Message not found" });

            message.IsRead = isRead;
            message.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking message read {MessageId}", id);
            return StatusCode(500, new { message = "Error updating message" });
        }
    }

    /// <summary>
    /// Star/unstar a message
    /// </summary>
    [HttpPatch("messages/{id}/star")]
    public async Task<ActionResult> StarMessage(int id, [FromQuery] bool isStarred = true)
    {
        try
        {
            var message = await _context.CommunicationMessages.FindAsync(id);
            if (message == null || message.IsDeleted)
                return NotFound(new { message = "Message not found" });

            message.IsStarred = isStarred;
            message.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starring message {MessageId}", id);
            return StatusCode(500, new { message = "Error updating message" });
        }
    }

    /// <summary>
    /// Archive a message
    /// </summary>
    [HttpPatch("messages/{id}/archive")]
    public async Task<ActionResult> ArchiveMessage(int id)
    {
        try
        {
            var message = await _context.CommunicationMessages.FindAsync(id);
            if (message == null || message.IsDeleted)
                return NotFound(new { message = "Message not found" });

            message.IsArchived = true;
            message.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error archiving message {MessageId}", id);
            return StatusCode(500, new { message = "Error updating message" });
        }
    }

    /// <summary>
    /// Delete a message
    /// </summary>
    [HttpDelete("messages/{id}")]
    public async Task<ActionResult> DeleteMessage(int id)
    {
        try
        {
            var message = await _context.CommunicationMessages.FindAsync(id);
            if (message == null || message.IsDeleted)
                return NotFound(new { message = "Message not found" });

            message.IsDeleted = true;
            message.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting message {MessageId}", id);
            return StatusCode(500, new { message = "Error deleting message" });
        }
    }

    #endregion

    #region Conversations

    /// <summary>
    /// Get conversations with optional filtering
    /// </summary>
    [HttpGet("conversations")]
    public async Task<ActionResult<List<ConversationListDto>>> GetConversations(
        [FromQuery] string? channelType = null,
        [FromQuery] string? status = null,
        [FromQuery] int? customerId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            var query = _context.Conversations
                .Where(c => !c.IsDeleted);

            if (!string.IsNullOrEmpty(channelType) && Enum.TryParse<ChannelType>(channelType, true, out var ct))
                query = query.Where(c => c.PrimaryChannelType == ct);

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<ConversationStatus>(status, true, out var st))
                query = query.Where(c => c.Status == st);

            if (customerId.HasValue)
                query = query.Where(c => c.CustomerId == customerId);

            var conversations = await query
                .OrderByDescending(c => c.IsPinned)
                .ThenByDescending(c => c.LastMessageAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new ConversationListDto
                {
                    Id = c.Id,
                    ConversationId = c.ConversationId,
                    ChannelType = c.PrimaryChannelType.ToString(),
                    Subject = c.Subject,
                    LastMessagePreview = c.LastMessagePreview,
                    Status = c.Status.ToString(),
                    ParticipantAddress = c.ParticipantAddress,
                    ParticipantName = c.ParticipantName,
                    MessageCount = c.MessageCount,
                    UnreadCount = c.UnreadCount,
                    LastMessageAt = c.LastMessageAt,
                    IsStarred = c.IsStarred,
                    IsPinned = c.IsPinned,
                    CustomerId = c.CustomerId,
                    ContactId = c.ContactId
                })
                .ToListAsync();

            return Ok(conversations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting conversations");
            return StatusCode(500, new { message = "Error retrieving conversations" });
        }
    }

    /// <summary>
    /// Get a conversation with its messages
    /// </summary>
    [HttpGet("conversations/{id}")]
    public async Task<ActionResult<ConversationDto>> GetConversation(int id)
    {
        try
        {
            var conversation = await _context.Conversations
                .Where(c => c.Id == id && !c.IsDeleted)
                .FirstOrDefaultAsync();

            if (conversation == null)
                return NotFound(new { message = "Conversation not found" });

            var messages = await _context.CommunicationMessages
                .Where(m => m.ConversationId == conversation.ConversationId && !m.IsDeleted)
                .OrderBy(m => m.CreatedAt)
                .Select(m => new CommunicationMessageListDto
                {
                    Id = m.Id,
                    ChannelType = m.ChannelType.ToString(),
                    Subject = m.Subject,
                    BodyPreview = m.Body,
                    Direction = m.Direction.ToString(),
                    Status = m.Status.ToString(),
                    FromAddress = m.FromAddress,
                    FromName = m.FromName,
                    ToAddress = m.ToAddress,
                    ToName = m.ToName,
                    IsRead = m.IsRead,
                    SentAt = m.SentAt,
                    ReceivedAt = m.ReceivedAt,
                    CreatedAt = m.CreatedAt
                })
                .ToListAsync();

            var dto = new ConversationDto
            {
                Id = conversation.Id,
                ConversationId = conversation.ConversationId,
                ChannelType = conversation.PrimaryChannelType.ToString(),
                Subject = conversation.Subject,
                Status = conversation.Status.ToString(),
                Priority = conversation.Priority.ToString(),
                ParticipantAddress = conversation.ParticipantAddress,
                ParticipantName = conversation.ParticipantName,
                CustomerId = conversation.CustomerId,
                ContactId = conversation.ContactId,
                LeadId = conversation.LeadId,
                AssignedToUserId = conversation.AssignedToUserId,
                MessageCount = conversation.MessageCount,
                UnreadCount = conversation.UnreadCount,
                FirstMessageAt = conversation.FirstMessageAt,
                LastMessageAt = conversation.LastMessageAt,
                IsStarred = conversation.IsStarred,
                IsMuted = conversation.IsMuted,
                IsPinned = conversation.IsPinned,
                Messages = messages
            };

            if (!string.IsNullOrEmpty(conversation.TagsJson))
                dto.Tags = JsonSerializer.Deserialize<List<string>>(conversation.TagsJson);

            // Mark all messages as read
            var unreadMessages = await _context.CommunicationMessages
                .Where(m => m.ConversationId == conversation.ConversationId && !m.IsRead && !m.IsDeleted)
                .ToListAsync();
            foreach (var msg in unreadMessages)
                msg.IsRead = true;
            conversation.UnreadCount = 0;
            await _context.SaveChangesAsync();

            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting conversation {ConversationId}", id);
            return StatusCode(500, new { message = "Error retrieving conversation" });
        }
    }

    #endregion

    #region Email Templates

    /// <summary>
    /// Get all email templates
    /// </summary>
    [HttpGet("templates")]
    public async Task<ActionResult<List<EmailTemplateListDto>>> GetEmailTemplates([FromQuery] string? category = null)
    {
        try
        {
            var query = _context.EmailTemplates.Where(t => !t.IsDeleted);

            if (!string.IsNullOrEmpty(category) && Enum.TryParse<EmailTemplateCategory>(category, true, out var cat))
                query = query.Where(t => t.Category == cat);

            var templates = await query
                .OrderBy(t => t.Category)
                .ThenBy(t => t.Name)
                .Select(t => new EmailTemplateListDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Description = t.Description,
                    Category = t.Category.ToString(),
                    Subject = t.Subject,
                    IsActive = t.IsActive,
                    IsSystem = t.IsSystem,
                    UsageCount = t.UsageCount,
                    LastUsedAt = t.LastUsedAt,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();

            return Ok(templates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting email templates");
            return StatusCode(500, new { message = "Error retrieving templates" });
        }
    }

    /// <summary>
    /// Get a specific email template
    /// </summary>
    [HttpGet("templates/{id}")]
    public async Task<ActionResult<EmailTemplateDto>> GetEmailTemplate(int id)
    {
        try
        {
            var template = await _context.EmailTemplates
                .Where(t => t.Id == id && !t.IsDeleted)
                .FirstOrDefaultAsync();

            if (template == null)
                return NotFound(new { message = "Template not found" });

            var dto = new EmailTemplateDto
            {
                Id = template.Id,
                Name = template.Name,
                Description = template.Description,
                Category = template.Category.ToString(),
                Subject = template.Subject,
                PlainTextBody = template.PlainTextBody,
                HtmlBody = template.HtmlBody,
                IsActive = template.IsActive,
                IsSystem = template.IsSystem,
                FromEmail = template.FromEmail,
                FromName = template.FromName,
                ReplyToEmail = template.ReplyToEmail,
                PreviewText = template.PreviewText,
                UsageCount = template.UsageCount,
                LastUsedAt = template.LastUsedAt,
                CreatedAt = template.CreatedAt
            };

            if (!string.IsNullOrEmpty(template.MergeFieldsJson))
                dto.MergeFields = JsonSerializer.Deserialize<List<string>>(template.MergeFieldsJson);

            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting email template {TemplateId}", id);
            return StatusCode(500, new { message = "Error retrieving template" });
        }
    }

    /// <summary>
    /// Create a new email template
    /// </summary>
    [HttpPost("templates")]
    public async Task<ActionResult<EmailTemplateDto>> CreateEmailTemplate([FromBody] EmailTemplateCreateDto dto)
    {
        try
        {
            var template = new EmailTemplate
            {
                Name = dto.Name,
                Description = dto.Description,
                Category = Enum.TryParse<EmailTemplateCategory>(dto.Category, true, out var cat) ? cat : EmailTemplateCategory.General,
                Subject = dto.Subject,
                PlainTextBody = dto.PlainTextBody,
                HtmlBody = dto.HtmlBody,
                IsActive = dto.IsActive,
                FromEmail = dto.FromEmail,
                FromName = dto.FromName,
                ReplyToEmail = dto.ReplyToEmail,
                PreviewText = dto.PreviewText,
                CreatedAt = DateTime.UtcNow
            };

            _context.EmailTemplates.Add(template);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created email template {TemplateName}", template.Name);

            return CreatedAtAction(nameof(GetEmailTemplate), new { id = template.Id }, await GetEmailTemplate(template.Id));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating email template");
            return StatusCode(500, new { message = "Error creating template" });
        }
    }

    /// <summary>
    /// Update an email template
    /// </summary>
    [HttpPut("templates/{id}")]
    public async Task<ActionResult<EmailTemplateDto>> UpdateEmailTemplate(int id, [FromBody] EmailTemplateCreateDto dto)
    {
        try
        {
            var template = await _context.EmailTemplates.FindAsync(id);
            if (template == null || template.IsDeleted)
                return NotFound(new { message = "Template not found" });

            if (template.IsSystem)
                return BadRequest(new { message = "Cannot modify system templates" });

            template.Name = dto.Name;
            template.Description = dto.Description;
            template.Category = Enum.TryParse<EmailTemplateCategory>(dto.Category, true, out var cat) ? cat : EmailTemplateCategory.General;
            template.Subject = dto.Subject;
            template.PlainTextBody = dto.PlainTextBody;
            template.HtmlBody = dto.HtmlBody;
            template.IsActive = dto.IsActive;
            template.FromEmail = dto.FromEmail;
            template.FromName = dto.FromName;
            template.ReplyToEmail = dto.ReplyToEmail;
            template.PreviewText = dto.PreviewText;
            template.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated email template {TemplateId}", id);

            return Ok(await GetEmailTemplate(id));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating email template {TemplateId}", id);
            return StatusCode(500, new { message = "Error updating template" });
        }
    }

    /// <summary>
    /// Delete an email template
    /// </summary>
    [HttpDelete("templates/{id}")]
    public async Task<ActionResult> DeleteEmailTemplate(int id)
    {
        try
        {
            var template = await _context.EmailTemplates.FindAsync(id);
            if (template == null || template.IsDeleted)
                return NotFound(new { message = "Template not found" });

            if (template.IsSystem)
                return BadRequest(new { message = "Cannot delete system templates" });

            template.IsDeleted = true;
            template.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting email template {TemplateId}", id);
            return StatusCode(500, new { message = "Error deleting template" });
        }
    }

    #endregion

    #region Statistics

    /// <summary>
    /// Get communication statistics
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult<CommunicationStatsDto>> GetStats()
    {
        try
        {
            var messages = _context.CommunicationMessages.Where(m => !m.IsDeleted);
            var conversations = _context.Conversations.Where(c => !c.IsDeleted);

            var stats = new CommunicationStatsDto
            {
                TotalMessages = await messages.CountAsync(),
                TotalInbound = await messages.CountAsync(m => m.Direction == MessageDirection.Inbound),
                TotalOutbound = await messages.CountAsync(m => m.Direction == MessageDirection.Outbound),
                UnreadCount = await messages.CountAsync(m => !m.IsRead && m.Direction == MessageDirection.Inbound),
                OpenConversations = await conversations.CountAsync(c => c.Status == ConversationStatus.Open),
                PendingMessages = await messages.CountAsync(m => m.Status == MessageStatus.Queued)
            };

            // Per-channel stats
            stats.EmailStats = await GetChannelStats(ChannelType.Email);
            stats.WhatsAppStats = await GetChannelStats(ChannelType.WhatsApp);
            stats.TwitterStats = await GetChannelStats(ChannelType.Twitter);
            stats.FacebookStats = await GetChannelStats(ChannelType.Facebook);

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting communication stats");
            return StatusCode(500, new { message = "Error retrieving statistics" });
        }
    }

    private async Task<ChannelStatsDto> GetChannelStats(ChannelType channelType)
    {
        var messages = _context.CommunicationMessages
            .Where(m => !m.IsDeleted && m.ChannelType == channelType);

        return new ChannelStatsDto
        {
            ChannelType = channelType.ToString(),
            TotalMessages = await messages.CountAsync(),
            Inbound = await messages.CountAsync(m => m.Direction == MessageDirection.Inbound),
            Outbound = await messages.CountAsync(m => m.Direction == MessageDirection.Outbound),
            Unread = await messages.CountAsync(m => !m.IsRead && m.Direction == MessageDirection.Inbound),
            Pending = await messages.CountAsync(m => m.Status == MessageStatus.Queued),
            Failed = await messages.CountAsync(m => m.Status == MessageStatus.Failed)
        };
    }

    #endregion
}
