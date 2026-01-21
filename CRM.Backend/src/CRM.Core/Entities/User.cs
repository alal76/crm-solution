namespace CRM.Core.Entities;

/// <summary>
/// User roles for role-based access control (RBAC) in the CRM system.
/// 
/// FUNCTIONAL VIEW:
/// - Defines the permission levels available to users
/// - Controls access to features, data, and administrative functions
/// - Higher roles have broader access permissions
/// 
/// TECHNICAL VIEW:
/// - Stored as integer in database for efficiency
/// - Role checking performed via middleware and service layer
/// - Maps to frontend route guards and UI element visibility
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Full system access - can manage users, settings, and all data
    /// </summary>
    Admin = 0,
    
    /// <summary>
    /// Department/team management access - can view reports and manage team members
    /// </summary>
    Manager = 1,
    
    /// <summary>
    /// Sales user access - can manage customers, opportunities, and quotes
    /// </summary>
    Sales = 2,
    
    /// <summary>
    /// Support access - can view customers and manage support interactions
    /// </summary>
    Support = 3,
    
    /// <summary>
    /// Limited read-only access - can view but not modify data
    /// </summary>
    Guest = 4
}

/// <summary>
/// User entity representing authenticated users of the CRM system.
/// 
/// FUNCTIONAL VIEW:
/// - Represents employees, contractors, or partners using the CRM
/// - Each user has credentials, role, and optional department assignment
/// - Users can be linked to a Contact record for self-service scenarios
/// - Supports two-factor authentication for enhanced security
/// 
/// TECHNICAL VIEW:
/// - Inherits from BaseEntity (Id, CreatedAt, UpdatedAt, IsDeleted)
/// - Password stored as BCrypt hash, never in plain text
/// - JWT tokens generated based on user claims
/// - Soft-deletable via IsDeleted flag
/// </summary>
public class User : BaseEntity
{
    /// <summary>
    /// Unique login identifier for the user
    /// </summary>
    public string Username { get; set; } = string.Empty;
    
    /// <summary>
    /// User's email address - used for notifications and password reset
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// User's first/given name
    /// </summary>
    public string FirstName { get; set; } = string.Empty;
    
    /// <summary>
    /// User's last/family name
    /// </summary>
    public string LastName { get; set; } = string.Empty;
    
    /// <summary>
    /// BCrypt hashed password - never store plain text passwords
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;
    
    /// <summary>
    /// User's role level (0=Admin, 1=Manager, 2=Sales, 3=Support, 4=Guest)
    /// </summary>
    public int Role { get; set; } = 0;
    
    /// <summary>
    /// Whether the user can currently log in
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Last successful login timestamp
    /// </summary>
    public DateTime? LastLoginDate { get; set; }

    // === Two-Factor Authentication ===
    
    /// <summary>
    /// Whether 2FA is enabled for this account
    /// </summary>
    public bool TwoFactorEnabled { get; set; } = false;
    
    /// <summary>
    /// TOTP secret key for authenticator apps (Google Authenticator, Authy, etc.)
    /// </summary>
    public string? TwoFactorSecret { get; set; }
    
    /// <summary>
    /// JSON array of one-time backup codes for 2FA recovery
    /// </summary>
    public string? BackupCodes { get; set; }

    // === Password Reset ===
    
    /// <summary>
    /// Token for password reset flow - expires after use
    /// </summary>
    public string? PasswordResetToken { get; set; }
    
    /// <summary>
    /// Expiration time for password reset token
    /// </summary>
    public DateTime? PasswordResetTokenExpiry { get; set; }

    // === Email Verification ===
    
    /// <summary>
    /// Whether the user's email has been verified
    /// </summary>
    public bool EmailVerified { get; set; } = false;
    
    /// <summary>
    /// Token for email verification flow
    /// </summary>
    public string? EmailVerificationToken { get; set; }

    // === Department and Profile Management ===
    
    /// <summary>
    /// Foreign key to the user's department
    /// </summary>
    public int? DepartmentId { get; set; }
    
    /// <summary>
    /// Foreign key to the user's profile (permissions, preferences)
    /// </summary>
    public int? UserProfileId { get; set; }
    
    /// <summary>
    /// Optional link to a Contact record for unified identity
    /// </summary>
    public int? ContactId { get; set; }
    
    /// <summary>
    /// Primary group for this user (required - every user belongs to a group)
    /// </summary>
    public int? PrimaryGroupId { get; set; }

    // === Customization ===
    
    /// <summary>
    /// Custom header color for this user (hex format, e.g., #FF0000)
    /// Admin users default to red header
    /// </summary>
    public string? HeaderColor { get; set; }
    
    /// <summary>
    /// URL to user's profile photo
    /// </summary>
    public string? PhotoUrl { get; set; }

    // === Navigation Properties ===
    
    /// <summary>
    /// Navigation to user's department
    /// </summary>
    public virtual Department? Department { get; set; }
    
    /// <summary>
    /// Navigation to user's profile
    /// </summary>
    public virtual UserProfile? UserProfile { get; set; }
    
    /// <summary>
    /// Navigation to user's primary group
    /// </summary>
    public virtual UserGroup? PrimaryGroup { get; set; }
    
    /// <summary>
    /// Group memberships for this user
    /// </summary>
    public virtual ICollection<UserGroupMember> GroupMemberships { get; set; } = new List<UserGroupMember>();
    
    /// <summary>
    /// OAuth tokens for social login (Google, Microsoft, etc.)
    /// </summary>
    public virtual ICollection<OAuthToken> OAuthTokens { get; set; } = new List<OAuthToken>();
}
