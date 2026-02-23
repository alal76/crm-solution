// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Dtos;

/// <summary>
/// DTO for listing API users (never includes the raw API key)
/// </summary>
public class ApiUserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? ApiKeyPrefix { get; set; }
    public DateTime? ApiKeyCreatedAt { get; set; }
    public DateTime? ApiKeyLastUsedAt { get; set; }
    public DateTime? ApiKeyExpiresAt { get; set; }
    public string? ApiUserDescription { get; set; }
    public int? PrimaryGroupId { get; set; }
    public string? PrimaryGroupName { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Request to create a new API user
/// </summary>
public class CreateApiUserRequest
{
    /// <summary>
    /// Display name for the API user (e.g., "CI/CD Pipeline")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Email for the API user (must be unique, used as identifier)
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Description of the API user's purpose
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Role ID (0=Admin, 1=Manager, 2=Sales, 3=Support, 4=Guest). Default: 4 (Guest)
    /// </summary>
    public int RoleId { get; set; } = 4;

    /// <summary>
    /// Primary group ID to assign API group RBAC permissions
    /// </summary>
    public int? PrimaryGroupId { get; set; }

    /// <summary>
    /// Optional API key expiration date. Null means no expiration.
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
}

/// <summary>
/// Response returned when an API key is created or regenerated.
/// The raw API key is only shown ONCE in this response.
/// </summary>
public class ApiKeyResponse
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// The raw API key — store it securely. It will NOT be shown again.
    /// Format: crm_{random-base64}
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    public string ApiKeyPrefix { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}
