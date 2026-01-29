using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CRM.Api.Hubs;

/// <summary>
/// SignalR Hub for real-time CRM notifications.
/// Enables multi-user collaboration by broadcasting record changes.
/// 
/// FEATURES:
/// - Subscribe to specific record updates
/// - Receive notifications when records are modified by other users
/// - Broadcasting for entity CRUD operations
/// - User presence tracking (optional)
/// </summary>
[Authorize]
public class CrmNotificationHub : Hub
{
    private readonly ILogger<CrmNotificationHub> _logger;
    
    public CrmNotificationHub(ILogger<CrmNotificationHub> logger)
    {
        _logger = logger;
    }
    
    /// <summary>
    /// Called when a client connects to the hub.
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst("sub")?.Value ?? Context.User?.FindFirst("userId")?.Value;
        _logger.LogInformation("User {UserId} connected to CRM notifications hub. ConnectionId: {ConnectionId}", 
            userId, Context.ConnectionId);
        
        await base.OnConnectedAsync();
    }
    
    /// <summary>
    /// Called when a client disconnects from the hub.
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst("sub")?.Value ?? Context.User?.FindFirst("userId")?.Value;
        _logger.LogInformation("User {UserId} disconnected from CRM notifications hub. ConnectionId: {ConnectionId}", 
            userId, Context.ConnectionId);
        
        await base.OnDisconnectedAsync(exception);
    }
    
    /// <summary>
    /// Subscribe to updates for a specific entity record.
    /// Clients call this when they open an entity for viewing/editing.
    /// </summary>
    /// <param name="entityType">The type of entity (e.g., "Customer", "Lead", "Opportunity")</param>
    /// <param name="entityId">The unique ID of the entity</param>
    public async Task SubscribeToRecord(string entityType, int entityId)
    {
        var groupName = GetRecordGroupName(entityType, entityId);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        
        _logger.LogDebug("Connection {ConnectionId} subscribed to {GroupName}", 
            Context.ConnectionId, groupName);
        
        // Notify others that a user is viewing this record
        await Clients.OthersInGroup(groupName).SendAsync("UserViewingRecord", new
        {
            EntityType = entityType,
            EntityId = entityId,
            UserId = Context.User?.FindFirst("sub")?.Value,
            UserName = Context.User?.Identity?.Name,
            Timestamp = DateTime.UtcNow
        });
    }
    
    /// <summary>
    /// Unsubscribe from updates for a specific entity record.
    /// Clients call this when they close an entity or navigate away.
    /// </summary>
    /// <param name="entityType">The type of entity</param>
    /// <param name="entityId">The unique ID of the entity</param>
    public async Task UnsubscribeFromRecord(string entityType, int entityId)
    {
        var groupName = GetRecordGroupName(entityType, entityId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        
        _logger.LogDebug("Connection {ConnectionId} unsubscribed from {GroupName}", 
            Context.ConnectionId, groupName);
        
        // Notify others that a user stopped viewing this record
        await Clients.OthersInGroup(groupName).SendAsync("UserLeftRecord", new
        {
            EntityType = entityType,
            EntityId = entityId,
            UserId = Context.User?.FindFirst("sub")?.Value,
            UserName = Context.User?.Identity?.Name,
            Timestamp = DateTime.UtcNow
        });
    }
    
    /// <summary>
    /// Subscribe to all updates for an entity type.
    /// Useful for list views that need to know about new/deleted records.
    /// </summary>
    /// <param name="entityType">The type of entity (e.g., "Customer")</param>
    public async Task SubscribeToEntityType(string entityType)
    {
        var groupName = GetEntityTypeGroupName(entityType);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        
        _logger.LogDebug("Connection {ConnectionId} subscribed to entity type {EntityType}", 
            Context.ConnectionId, entityType);
    }
    
    /// <summary>
    /// Unsubscribe from all updates for an entity type.
    /// </summary>
    /// <param name="entityType">The type of entity</param>
    public async Task UnsubscribeFromEntityType(string entityType)
    {
        var groupName = GetEntityTypeGroupName(entityType);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        
        _logger.LogDebug("Connection {ConnectionId} unsubscribed from entity type {EntityType}", 
            Context.ConnectionId, entityType);
    }
    
    /// <summary>
    /// Notify clients that a user is actively editing a record.
    /// Provides visual feedback to other users viewing the same record.
    /// </summary>
    /// <param name="entityType">The type of entity</param>
    /// <param name="entityId">The unique ID of the entity</param>
    public async Task StartEditing(string entityType, int entityId)
    {
        var groupName = GetRecordGroupName(entityType, entityId);
        
        await Clients.OthersInGroup(groupName).SendAsync("UserEditingRecord", new
        {
            EntityType = entityType,
            EntityId = entityId,
            UserId = Context.User?.FindFirst("sub")?.Value,
            UserName = Context.User?.Identity?.Name,
            Timestamp = DateTime.UtcNow,
            IsEditing = true
        });
    }
    
    /// <summary>
    /// Notify clients that a user stopped editing a record.
    /// </summary>
    /// <param name="entityType">The type of entity</param>
    /// <param name="entityId">The unique ID of the entity</param>
    public async Task StopEditing(string entityType, int entityId)
    {
        var groupName = GetRecordGroupName(entityType, entityId);
        
        await Clients.OthersInGroup(groupName).SendAsync("UserEditingRecord", new
        {
            EntityType = entityType,
            EntityId = entityId,
            UserId = Context.User?.FindFirst("sub")?.Value,
            UserName = Context.User?.Identity?.Name,
            Timestamp = DateTime.UtcNow,
            IsEditing = false
        });
    }
    
    // Helper methods for group names
    private static string GetRecordGroupName(string entityType, int entityId) 
        => $"record:{entityType.ToLowerInvariant()}:{entityId}";
    
    private static string GetEntityTypeGroupName(string entityType) 
        => $"entitytype:{entityType.ToLowerInvariant()}";
}

/// <summary>
/// Service for broadcasting notifications from server-side code (controllers, services).
/// Inject this service to send notifications from anywhere in the application.
/// </summary>
public interface ICrmNotificationService
{
    /// <summary>
    /// Notify subscribers that a record was created.
    /// </summary>
    Task NotifyRecordCreatedAsync(string entityType, int entityId, object record, string? createdByUserId = null);
    
    /// <summary>
    /// Notify subscribers that a record was updated.
    /// </summary>
    Task NotifyRecordUpdatedAsync(string entityType, int entityId, object record, string? updatedByUserId = null);
    
    /// <summary>
    /// Notify subscribers that a record was deleted.
    /// </summary>
    Task NotifyRecordDeletedAsync(string entityType, int entityId, string? deletedByUserId = null);
}

/// <summary>
/// Implementation of ICrmNotificationService using SignalR.
/// </summary>
public class CrmNotificationService : ICrmNotificationService
{
    private readonly IHubContext<CrmNotificationHub> _hubContext;
    private readonly ILogger<CrmNotificationService> _logger;
    
    public CrmNotificationService(
        IHubContext<CrmNotificationHub> hubContext,
        ILogger<CrmNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }
    
    public async Task NotifyRecordCreatedAsync(string entityType, int entityId, object record, string? createdByUserId = null)
    {
        var notification = new
        {
            Action = "Created",
            EntityType = entityType,
            EntityId = entityId,
            Record = record,
            UserId = createdByUserId,
            Timestamp = DateTime.UtcNow
        };
        
        // Notify entity type subscribers (list views)
        var entityTypeGroup = $"entitytype:{entityType.ToLowerInvariant()}";
        await _hubContext.Clients.Group(entityTypeGroup).SendAsync("RecordCreated", notification);
        
        _logger.LogDebug("Sent RecordCreated notification for {EntityType}:{EntityId}", entityType, entityId);
    }
    
    public async Task NotifyRecordUpdatedAsync(string entityType, int entityId, object record, string? updatedByUserId = null)
    {
        var notification = new
        {
            Action = "Updated",
            EntityType = entityType,
            EntityId = entityId,
            Record = record,
            UserId = updatedByUserId,
            Timestamp = DateTime.UtcNow
        };
        
        // Notify record subscribers (detail/edit views)
        var recordGroup = $"record:{entityType.ToLowerInvariant()}:{entityId}";
        await _hubContext.Clients.Group(recordGroup).SendAsync("RecordUpdated", notification);
        
        // Notify entity type subscribers (list views)
        var entityTypeGroup = $"entitytype:{entityType.ToLowerInvariant()}";
        await _hubContext.Clients.Group(entityTypeGroup).SendAsync("RecordUpdated", notification);
        
        _logger.LogDebug("Sent RecordUpdated notification for {EntityType}:{EntityId}", entityType, entityId);
    }
    
    public async Task NotifyRecordDeletedAsync(string entityType, int entityId, string? deletedByUserId = null)
    {
        var notification = new
        {
            Action = "Deleted",
            EntityType = entityType,
            EntityId = entityId,
            UserId = deletedByUserId,
            Timestamp = DateTime.UtcNow
        };
        
        // Notify record subscribers (detail/edit views)
        var recordGroup = $"record:{entityType.ToLowerInvariant()}:{entityId}";
        await _hubContext.Clients.Group(recordGroup).SendAsync("RecordDeleted", notification);
        
        // Notify entity type subscribers (list views)
        var entityTypeGroup = $"entitytype:{entityType.ToLowerInvariant()}";
        await _hubContext.Clients.Group(entityTypeGroup).SendAsync("RecordDeleted", notification);
        
        _logger.LogDebug("Sent RecordDeleted notification for {EntityType}:{EntityId}", entityType, entityId);
    }
}
