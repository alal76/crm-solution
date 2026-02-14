using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CRM.Core.Interfaces
{
    /// <summary>
    /// Service interface for Time-based One-Time Password (TOTP) authentication.
    /// Implements RFC 6238 standard with HMAC-SHA1, 30-second time windows.
    /// </summary>
    public interface ITotpService
    {
        /// <summary>
        /// Initiates TOTP setup for a user. Generates a secret and returns setup data including QR code.
        /// </summary>
        /// <param name="userId">User ID initiating setup</param>
        /// <param name="userEmail">User email for authenticator app</param>
        /// <returns>Setup data with secret and QR code URL</returns>
        Task<TotpSetupDto> InitializeSetupAsync(int userId, string userEmail);

        /// <summary>
        /// Verifies the user's TOTP setup by validating a test code.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="totp">6-digit TOTP code from authenticator app</param>
        /// <param name="secret">The secret to verify against (temporary during setup)</param>
        /// <returns>Success status</returns>
        Task<bool> VerifySetupAsync(int userId, string totp, string secret);

        /// <summary>
        /// Completes TOTP setup by storing the secret and generating backup codes.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="secret">Verified TOTP secret</param>
        /// <returns>Backup codes for account recovery</returns>
        Task<BackupCodesDto> CompleteSetupAsync(int userId, string secret);

        /// <summary>
        /// Verifies a TOTP code for authentication.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="totp">6-digit TOTP code from authenticator app</param>
        /// <returns>Verification result with grace period details</returns>
        Task<TotpVerificationResultDto> VerifyAsync(int userId, string totp);

        /// <summary>
        /// Validates a backup code and marks it as used (one-time use).
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="code">Backup code (8 characters)</param>
        /// <returns>Success status</returns>
        Task<bool> VerifyBackupCodeAsync(int userId, string code);

        /// <summary>
        /// Disables TOTP for a user (requires authentication).
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Success status</returns>
        Task<bool> DisableAsync(int userId);

        /// <summary>
        /// Gets remaining backup codes count for a user.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Number of unused backup codes</returns>
        Task<int> GetRemainingBackupCodesAsync(int userId);

        /// <summary>
        /// Regenerates backup codes (invalidates old ones).
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>New backup codes</returns>
        Task<BackupCodesDto> RegenerateBackupCodesAsync(int userId);

        /// <summary>
        /// Checks if TOTP is enabled for a user.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>True if TOTP is active</returns>
        Task<bool> IsEnabledAsync(int userId);
    }

    /// <summary>
    /// TOTP setup data with secret and QR code information.
    /// </summary>
    public class TotpSetupDto
    {
        /// <summary>Base32-encoded secret key for manual entry.</summary>
        public string Secret { get; set; } = string.Empty;

        /// <summary>URL for QR code generation (otpauth:// scheme).</summary>
        public string QrCodeUrl { get; set; } = string.Empty;

        /// <summary>Manual entry key (formatted for readability).</summary>
        public string ManualEntryKey { get; set; } = string.Empty;

        /// <summary>Expected time window of next code (for user reference).</summary>
        public DateTime ExpiresAt { get; set; }
    }

    /// <summary>
    /// Backup codes for account recovery when TOTP authenticator is unavailable.
    /// </summary>
    public class BackupCodesDto
    {
        /// <summary>List of unused backup codes (8 characters each, typically 10 total).</summary>
        public IEnumerable<string> Codes { get; set; } = new List<string>();

        /// <summary>Total number of codes provided.</summary>
        public int TotalCodes { get; set; }

        /// <summary>Instruction text for saving backup codes securely.</summary>
        public string InstructionText { get; set; } = "Save these codes in a secure location. Each code can be used once to log in if you lose access to your authenticator app.";
    }

    /// <summary>
    /// Result of TOTP verification with additional context.
    /// </summary>
    public class TotpVerificationResultDto
    {
        /// <summary>Whether the TOTP code was valid.</summary>
        public bool IsValid { get; set; }

        /// <summary>Optional error message if invalid.</summary>
        public string? ErrorMessage { get; set; }

        /// <summary>Time when the next code will become valid (RFC 6238).</summary>
        public DateTime? NextCodeValidAt { get; set; }

        /// <summary>Whether grace period is still active for retry.</summary>
        public bool IsGracePeriodActive { get; set; }

        /// <summary>Remaining attempts in grace period (typically 1 for security).</summary>
        public int RemainingGraceAttempts { get; set; }
    }
}
