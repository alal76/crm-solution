namespace CRM.Core.Dtos
{
    /// <summary>
    /// Data transfer object for password policy configuration.
    /// Enforces password complexity and security requirements.
    /// </summary>
    public class PasswordPolicyDto
    {
        /// <summary>
        /// Minimum password length (default: 8).
        /// </summary>
        public int MinimumLength { get; set; } = 8;

        /// <summary>
        /// Maximum password length (0 = no limit).
        /// </summary>
        public int MaximumLength { get; set; } = 128;

        /// <summary>
        /// Require uppercase letters (A-Z).
        /// </summary>
        public bool RequireUppercase { get; set; } = true;

        /// <summary>
        /// Require lowercase letters (a-z).
        /// </summary>
        public bool RequireLowercase { get; set; } = true;

        /// <summary>
        /// Require numeric digits (0-9).
        /// </summary>
        public bool RequireNumbers { get; set; } = true;

        /// <summary>
        /// Require special characters (!@#$%^&*).
        /// </summary>
        public bool RequireSpecialCharacters { get; set; } = false;

        /// <summary>
        /// Allowed special characters.
        /// </summary>
        public string AllowedSpecialCharacters { get; set; } = "!@#$%^&*()_+-=[]{}|;:,.<>?";

        /// <summary>
        /// Password expiration in days (0 = never expires).
        /// </summary>
        public int ExpirationDays { get; set; } = 0;

        /// <summary>
        /// Days before expiration to show warning.
        /// </summary>
        public int ExpirationWarningDays { get; set; } = 7;

        /// <summary>
        /// Number of previous passwords to remember (prevent reuse).
        /// </summary>
        public int HistoryCount { get; set; } = 5;

        /// <summary>
        /// Days locked out after failed attempts.
        /// </summary>
        public int LockoutDurationMinutes { get; set; } = 30;

        /// <summary>
        /// Number of failed attempts before lockout.
        /// </summary>
        public int FailedAttemptLockout { get; set; } = 5;

        /// <summary>
        /// Minimum days between password changes.
        /// </summary>
        public int MinimumAgeDays { get; set; } = 1;

        /// <summary>
        /// Require password change on first login.
        /// </summary>
        public bool ForcePasswordChangeOnFirstLogin { get; set; } = false;

        /// <summary>
        /// Prevent passwords matching username.
        /// </summary>
        public bool PreventUsernameInPassword { get; set; } = true;

        /// <summary>
        /// When the policy was last updated.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}
