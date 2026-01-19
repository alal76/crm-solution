namespace CRM.Core.Entities;

/// <summary>
/// User roles for role-based access control
/// </summary>
public enum UserRole
{
    Admin = 0,
    Manager = 1,
    Sales = 2,
    Support = 3,
    Guest = 4
}

/// <summary>
/// User entity for managing CRM users
/// </summary>
public class User : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public int Role { get; set; } = 0; // Admin, Manager, Sales, Support
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginDate { get; set; }

    // Two-Factor Authentication
    public bool TwoFactorEnabled { get; set; } = false;
    public string? TwoFactorSecret { get; set; } // TOTP secret key
    public string? BackupCodes { get; set; } // JSON array of backup codes

    // Password Reset
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiry { get; set; }

    // Email Verification
    public bool EmailVerified { get; set; } = false;
    public string? EmailVerificationToken { get; set; }

    // Department and Profile Management
    public int? DepartmentId { get; set; }
    public int? UserProfileId { get; set; }

    // Navigation properties
    public virtual Department? Department { get; set; }
    public virtual UserProfile? UserProfile { get; set; }
    public virtual ICollection<OAuthToken> OAuthTokens { get; set; } = new List<OAuthToken>();
}
