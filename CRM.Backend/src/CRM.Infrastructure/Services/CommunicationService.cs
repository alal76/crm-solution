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

using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service for managing communication channels and messaging
/// </summary>
public class CommunicationService : ICommunicationService
{
    private readonly ICrmDbContext _dbContext;
    private readonly ILogger<CommunicationService> _logger;

    public CommunicationService(ICrmDbContext dbContext, ILogger<CommunicationService> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Channels

    /// <inheritdoc />
    public async Task<IEnumerable<CommunicationChannelInfo>> GetChannelsAsync()
    {
        _logger.LogDebug("Getting all communication channels");

        return await _dbContext.CommunicationChannels
            .Where(c => !c.IsDeleted)
            .Select(c => new CommunicationChannelInfo
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
    }

    /// <inheritdoc />
    public async Task<CommunicationChannelDetail?> GetChannelByIdAsync(int id)
    {
        _logger.LogDebug("Getting channel by ID: {ChannelId}", id);

        var channel = await _dbContext.CommunicationChannels
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

        if (channel == null) return null;

        return MapToDetail(channel);
    }

    /// <inheritdoc />
    public async Task<CommunicationChannelDetail> CreateChannelAsync(CommunicationChannelCreateRequest dto)
    {
        _logger.LogDebug("Creating communication channel: {ChannelName}", dto.Name);

        try
        {
            var channelType = ParseChannelType(dto.ChannelType);

            // If this channel should be default, unset other defaults of same type
            if (dto.IsDefault)
            {
                var existingDefaults = await _dbContext.CommunicationChannels
                    .Where(c => c.ChannelType == channelType && c.IsDefault && !c.IsDeleted)
                    .ToListAsync();

                foreach (var ch in existingDefaults)
                {
                    ch.IsDefault = false;
                    ch.UpdatedAt = DateTime.UtcNow;
                }
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
                SmtpUseSsl = dto.SmtpUseSsl ?? true,
                SmtpUsername = dto.SmtpUsername,
                SmtpPassword = dto.SmtpPassword,
                ImapServer = dto.ImapServer,
                ImapPort = dto.ImapPort,
                ImapUseSsl = dto.ImapUseSsl ?? true,
                FromEmail = dto.FromEmail,
                FromName = dto.FromName,
                WhatsAppBusinessAccountId = dto.WhatsAppBusinessAccountId,
                WhatsAppPhoneNumberId = dto.WhatsAppPhoneNumberId,
                WhatsAppVerifyToken = dto.WhatsAppVerifyToken,
                SocialAccountId = dto.SocialAccountId,
                SocialUsername = dto.SocialUsername,
                PageAccessToken = dto.PageAccessToken,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _dbContext.CommunicationChannels.Add(channel);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Created communication channel {ChannelId}: {ChannelName}", 
                channel.Id, channel.Name);

            return MapToDetail(channel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating communication channel: {ChannelName}", dto.Name);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<CommunicationChannelDetail?> UpdateChannelAsync(int id, CommunicationChannelCreateRequest dto)
    {
        _logger.LogDebug("Updating channel {ChannelId}", id);

        try
        {
            var channel = await _dbContext.CommunicationChannels
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (channel == null)
            {
                _logger.LogWarning("Channel {ChannelId} not found", id);
                return null;
            }

            var channelType = ParseChannelType(dto.ChannelType);

            // If this channel should be default, unset other defaults of same type
            if (dto.IsDefault && !channel.IsDefault)
            {
                var existingDefaults = await _dbContext.CommunicationChannels
                    .Where(c => c.ChannelType == channelType && c.IsDefault && !c.IsDeleted && c.Id != id)
                    .ToListAsync();

                foreach (var ch in existingDefaults)
                {
                    ch.IsDefault = false;
                    ch.UpdatedAt = DateTime.UtcNow;
                }
            }

            // Update properties
            channel.ChannelType = channelType;
            channel.Name = dto.Name;
            channel.IsEnabled = dto.IsEnabled;
            channel.IsDefault = dto.IsDefault;
            channel.ApiKey = dto.ApiKey ?? channel.ApiKey;
            channel.ApiSecret = dto.ApiSecret ?? channel.ApiSecret;
            channel.ClientId = dto.ClientId ?? channel.ClientId;
            channel.ClientSecret = dto.ClientSecret ?? channel.ClientSecret;
            channel.AccessToken = dto.AccessToken ?? channel.AccessToken;
            channel.RefreshToken = dto.RefreshToken ?? channel.RefreshToken;
            channel.SmtpServer = dto.SmtpServer ?? channel.SmtpServer;
            channel.SmtpPort = dto.SmtpPort ?? channel.SmtpPort;
            channel.SmtpUseSsl = dto.SmtpUseSsl ?? channel.SmtpUseSsl;
            channel.SmtpUsername = dto.SmtpUsername ?? channel.SmtpUsername;
            channel.SmtpPassword = dto.SmtpPassword ?? channel.SmtpPassword;
            channel.ImapServer = dto.ImapServer ?? channel.ImapServer;
            channel.ImapPort = dto.ImapPort ?? channel.ImapPort;
            channel.ImapUseSsl = dto.ImapUseSsl ?? channel.ImapUseSsl;
            channel.FromEmail = dto.FromEmail ?? channel.FromEmail;
            channel.FromName = dto.FromName ?? channel.FromName;
            channel.WhatsAppBusinessAccountId = dto.WhatsAppBusinessAccountId ?? channel.WhatsAppBusinessAccountId;
            channel.WhatsAppPhoneNumberId = dto.WhatsAppPhoneNumberId ?? channel.WhatsAppPhoneNumberId;
            channel.WhatsAppVerifyToken = dto.WhatsAppVerifyToken ?? channel.WhatsAppVerifyToken;
            channel.SocialAccountId = dto.SocialAccountId ?? channel.SocialAccountId;
            channel.SocialUsername = dto.SocialUsername ?? channel.SocialUsername;
            channel.PageAccessToken = dto.PageAccessToken ?? channel.PageAccessToken;
            channel.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Updated communication channel {ChannelId}", id);
            return MapToDetail(channel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating channel {ChannelId}", id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> DeleteChannelAsync(int id)
    {
        _logger.LogDebug("Deleting channel {ChannelId}", id);

        try
        {
            var channel = await _dbContext.CommunicationChannels
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (channel == null)
            {
                _logger.LogWarning("Channel {ChannelId} not found", id);
                return false;
            }

            channel.IsDeleted = true;
            channel.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Deleted communication channel {ChannelId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting channel {ChannelId}", id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> TestChannelAsync(int id)
    {
        _logger.LogDebug("Testing channel {ChannelId}", id);

        try
        {
            var channel = await _dbContext.CommunicationChannels
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (channel == null)
            {
                _logger.LogWarning("Channel {ChannelId} not found", id);
                return false;
            }

            bool success = false;
            string? error = null;

            try
            {
                // Test based on channel type
                success = channel.ChannelType switch
                {
                    ChannelType.Email => await TestEmailChannelAsync(channel),
                    ChannelType.WhatsApp => await TestWhatsAppChannelAsync(channel),
                    ChannelType.Facebook => await TestFacebookChannelAsync(channel),
                    ChannelType.Twitter => await TestTwitterChannelAsync(channel),
                    _ => false
                };
            }
            catch (Exception ex)
            {
                error = ex.Message;
                _logger.LogWarning(ex, "Channel {ChannelId} test failed", id);
            }

            // Update channel status
            channel.LastConnectedAt = success ? DateTime.UtcNow : channel.LastConnectedAt;
            channel.Status = success ? ChannelStatus.Connected : ChannelStatus.Error;
            channel.LastError = error;
            channel.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing channel {ChannelId}", id);
            throw;
        }
    }

    #endregion

    #region Messages

    /// <inheritdoc />
    public async Task<IEnumerable<CommunicationMessage>> GetMessagesAsync(
        int? customerId = null,
        int? channelId = null,
        MessageDirection? direction = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        _logger.LogDebug("Getting messages with filters");

        var query = _dbContext.CommunicationMessages
            .Where(m => !m.IsDeleted)
            .AsQueryable();

        if (customerId.HasValue)
        {
            query = query.Where(m => m.AccountId == customerId.Value);
        }

        if (channelId.HasValue)
        {
            query = query.Where(m => m.ChannelId == channelId.Value);
        }

        if (direction.HasValue)
        {
            query = query.Where(m => m.Direction == direction.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(m => m.SentAt >= fromDate.Value || m.ReceivedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(m => m.SentAt <= toDate.Value || m.ReceivedAt <= toDate.Value);
        }

        return await query
            .OrderByDescending(m => m.SentAt ?? m.ReceivedAt)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<CommunicationMessage?> GetMessageByIdAsync(int id)
    {
        _logger.LogDebug("Getting message by ID: {MessageId}", id);

        return await _dbContext.CommunicationMessages
            .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);
    }

    /// <inheritdoc />
    public async Task<CommunicationMessage> SendMessageAsync(SendMessageRequest request)
    {
        _logger.LogDebug("Sending message via channel {ChannelId} to customer {CustomerId}", 
            request.ChannelId, request.CustomerId);

        try
        {
            var channel = await _dbContext.CommunicationChannels
                .FirstOrDefaultAsync(c => c.Id == request.ChannelId && !c.IsDeleted && c.IsEnabled);

            if (channel == null)
            {
                throw new InvalidOperationException($"Channel {request.ChannelId} not found or not enabled");
            }

            // Create the message record
            var message = new CommunicationMessage
            {
                AccountId = request.CustomerId,
                ContactId = request.ContactId,
                ChannelId = request.ChannelId,
                ChannelType = channel.ChannelType,
                Direction = MessageDirection.Outbound,
                Subject = request.Subject,
                Body = request.Body,
                HtmlBody = request.IsHtml ? request.Body : null,
                FromAddress = channel.FromEmail,
                ToAddress = request.ToEmail ?? request.ToPhone,
                Status = MessageStatus.Queued,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Actually send the message based on channel type
            try
            {
                bool sent = channel.ChannelType switch
                {
                    ChannelType.Email => await SendEmailAsync(channel, message, request),
                    ChannelType.WhatsApp => await SendWhatsAppAsync(channel, message, request),
                    _ => false
                };

                message.Status = sent ? MessageStatus.Sent : MessageStatus.Failed;
                message.SentAt = sent ? DateTime.UtcNow : null;
            }
            catch (Exception ex)
            {
                message.Status = MessageStatus.Failed;
                _logger.LogWarning(ex, "Failed to send message via channel {ChannelId}", request.ChannelId);
            }

            _dbContext.CommunicationMessages.Add(message);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Message {MessageId} created with status {Status}", 
                message.Id, message.Status);

            return message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message to customer {CustomerId}", request.CustomerId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<CommunicationMessage>> GetConversationAsync(int customerId, int? contactId = null)
    {
        _logger.LogDebug("Getting conversation for customer {CustomerId}", customerId);

        var query = _dbContext.CommunicationMessages
            .Where(m => m.AccountId == customerId && !m.IsDeleted);

        if (contactId.HasValue)
        {
            query = query.Where(m => m.ContactId == contactId.Value);
        }

        return await query
            .OrderBy(m => m.SentAt ?? m.ReceivedAt)
            .ToListAsync();
    }

    #endregion

    #region Private Helper Methods

    private static ChannelType ParseChannelType(string channelType)
    {
        if (Enum.TryParse<ChannelType>(channelType, true, out var result))
        {
            return result;
        }
        return ChannelType.Email; // Default
    }

    private static CommunicationChannelDetail MapToDetail(CommunicationChannel channel)
    {
        return new CommunicationChannelDetail
        {
            Id = channel.Id,
            ChannelType = channel.ChannelType.ToString(),
            Name = channel.Name,
            Status = channel.Status.ToString(),
            IsEnabled = channel.IsEnabled,
            IsDefault = channel.IsDefault,
            SocialUsername = channel.SocialUsername,
            FromEmail = channel.FromEmail,
            LastConnectedAt = channel.LastConnectedAt,
            CreatedAt = channel.CreatedAt,
            SmtpServer = channel.SmtpServer,
            SmtpPort = channel.SmtpPort,
            SmtpUseSsl = channel.SmtpUseSsl,
            SmtpUsername = channel.SmtpUsername,
            ImapServer = channel.ImapServer,
            ImapPort = channel.ImapPort,
            ImapUseSsl = channel.ImapUseSsl,
            FromName = channel.FromName,
            WhatsAppBusinessAccountId = channel.WhatsAppBusinessAccountId,
            WhatsAppPhoneNumberId = channel.WhatsAppPhoneNumberId,
            SocialAccountId = channel.SocialAccountId,
            WebhookUrl = channel.WebhookUrl,
            WebhookEnabled = channel.WebhookEnabled,
            LastError = channel.LastError
        };
    }

    private Task<bool> TestEmailChannelAsync(CommunicationChannel channel)
    {
        // For now, just validate configuration exists
        if (string.IsNullOrEmpty(channel.SmtpServer) || !channel.SmtpPort.HasValue)
        {
            throw new InvalidOperationException("SMTP server and port are required");
        }
        
        // In a real implementation, we would test SMTP connection
        _logger.LogInformation("Email channel {ChannelId} configuration validated", channel.Id);
        return Task.FromResult(true);
    }

    private Task<bool> TestWhatsAppChannelAsync(CommunicationChannel channel)
    {
        // Validate WhatsApp configuration
        if (string.IsNullOrEmpty(channel.WhatsAppBusinessAccountId) || 
            string.IsNullOrEmpty(channel.WhatsAppPhoneNumberId))
        {
            throw new InvalidOperationException("WhatsApp Business Account ID and Phone Number ID are required");
        }
        
        // In a real implementation, we would test WhatsApp API connection
        _logger.LogInformation("WhatsApp channel {ChannelId} configuration validated", channel.Id);
        return Task.FromResult(true);
    }

    private Task<bool> TestFacebookChannelAsync(CommunicationChannel channel)
    {
        // Validate Facebook configuration
        if (string.IsNullOrEmpty(channel.PageAccessToken))
        {
            throw new InvalidOperationException("Facebook Page Access Token is required");
        }
        
        _logger.LogInformation("Facebook channel {ChannelId} configuration validated", channel.Id);
        return Task.FromResult(true);
    }

    private Task<bool> TestTwitterChannelAsync(CommunicationChannel channel)
    {
        // Validate Twitter configuration
        if (string.IsNullOrEmpty(channel.ApiKey) || string.IsNullOrEmpty(channel.ApiSecret))
        {
            throw new InvalidOperationException("Twitter API Key and Secret are required");
        }
        
        _logger.LogInformation("Twitter channel {ChannelId} configuration validated", channel.Id);
        return Task.FromResult(true);
    }

    private Task<bool> SendEmailAsync(CommunicationChannel channel, CommunicationMessage message, SendMessageRequest request)
    {
        // In a real implementation, we would use SMTP to send
        // For now, just log and return success if configured properly
        if (string.IsNullOrEmpty(request.ToEmail))
        {
            throw new InvalidOperationException("ToEmail is required for email messages");
        }
        
        _logger.LogInformation("Would send email from {From} to {To}: {Subject}", 
            channel.FromEmail, request.ToEmail, message.Subject);
        return Task.FromResult(true);
    }

    private Task<bool> SendWhatsAppAsync(CommunicationChannel channel, CommunicationMessage message, SendMessageRequest request)
    {
        // In a real implementation, we would use WhatsApp Cloud API
        if (string.IsNullOrEmpty(request.ToPhone))
        {
            throw new InvalidOperationException("ToPhone is required for WhatsApp messages");
        }
        
        _logger.LogInformation("Would send WhatsApp message to {To}", request.ToPhone);
        return Task.FromResult(true);
    }

    #endregion
}
