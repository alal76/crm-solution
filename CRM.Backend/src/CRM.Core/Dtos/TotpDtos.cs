using System;
using System.Collections.Generic;

namespace CRM.Core.Dtos
{
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
