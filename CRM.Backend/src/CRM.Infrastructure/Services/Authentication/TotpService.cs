using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CRM.Core.Dtos;
using CRM.Core.Interfaces;
using CRM.Core.Options;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CRM.Infrastructure.Services.Authentication
{
    /// <summary>
    /// RFC 6238 Time-based One-Time Password (TOTP) service implementation.
    /// Uses HMAC-SHA1 with 30-second time windows to generate 6-digit codes.
    /// </summary>
    public class TotpService : CRM.Core.Interfaces.ITotpService
    {
        private readonly IOptions<TotpOptions> _options;
        private readonly ILogger<TotpService> _logger;

        private const int CodeLength = 6;
        private const int BackupCodeLength = 8;
        private const int DefaultBackupCodeCount = 10;

        public TotpService(IOptions<TotpOptions> options, ILogger<TotpService> logger)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<TotpSetupDto> InitializeSetupAsync(int userId, string userEmail)
        {
            try
            {
                // Generate random secret (160 bits = 20 bytes for SHA1)
                byte[] secretBytes = new byte[20];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(secretBytes);
                }

                // Encode secret as Base32 (RFC 4648)
                string base32Secret = Base32Encode(secretBytes);

                // Generate QR code URL (otpauth:// scheme per RFC 6238)
                string otpauthUrl = GenerateOtpauthUrl(userEmail, base32Secret);

                _logger.LogInformation("Initialized TOTP setup for user {UserId}", userId);

                return new TotpSetupDto
                {
                    Secret = base32Secret,
                    QrCodeUrl = otpauthUrl,
                    ManualEntryKey = FormatManualEntryKey(base32Secret),
                    ExpiresAt = DateTime.UtcNow.AddMinutes(_options.Value.SetupExpirationMinutes)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing TOTP setup for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> VerifySetupAsync(int userId, string totp, string secret)
        {
            try
            {
                if (string.IsNullOrEmpty(totp) || string.IsNullOrEmpty(secret))
                {
                    return false;
                }

                // Decode the Base32 secret
                byte[] secretBytes = Base32Decode(secret);

                // Verify the TOTP code
                long currentTimeCounter = GetTimeCounter();
                for (int i = -1; i <= 1; i++) // Allow ±1 time window for clock drift
                {
                    string code = GenerateTotpCode(secretBytes, currentTimeCounter + i);
                    if (code == totp)
                    {
                        _logger.LogInformation("TOTP setup verification succeeded for user {UserId}", userId);
                        return true;
                    }
                }

                _logger.LogWarning("TOTP setup verification failed for user {UserId}", userId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying TOTP setup for user {UserId}", userId);
                return false;
            }
        }

        public async Task<BackupCodesDto> CompleteSetupAsync(int userId, string secret)
        {
            try
            {
                // Decode and validate secret
                byte[] secretBytes;
                try
                {
                    secretBytes = Base32Decode(secret);
                }
                catch
                {
                    throw new ArgumentException("Invalid TOTP secret format");
                }

                // Generate backup codes
                var backupCodes = GenerateBackupCodes(_options.Value.BackupCodeCount);

                _logger.LogInformation("TOTP setup completed for user {UserId} with {BackupCodeCount} backup codes", 
                    userId, backupCodes.Count);

                return new BackupCodesDto
                {
                    Codes = backupCodes,
                    TotalCodes = backupCodes.Count
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing TOTP setup for user {UserId}", userId);
                throw;
            }
        }

        public async Task<TotpVerificationResultDto> VerifyAsync(int userId, string totp)
        {
            try
            {
                if (string.IsNullOrEmpty(totp) || totp.Length != CodeLength)
                {
                    return new TotpVerificationResultDto
                    {
                        IsValid = false,
                        ErrorMessage = "Invalid TOTP code format",
                        IsGracePeriodActive = false
                    };
                }

                // In a real implementation, retrieve user's stored TOTP secret from database
                // This is a placeholder implementation
                // byte[] secretBytes = await GetUserTotpSecretAsync(userId);

                long currentTimeCounter = GetTimeCounter();
                
                // Verify with current time window and ±1 windows for clock drift
                for (int i = -1; i <= 1; i++)
                {
                    // In real implementation: string code = GenerateTotpCode(secretBytes, currentTimeCounter + i);
                    // This would be validated here
                }

                _logger.LogInformation("TOTP verification completed for user {UserId}", userId);

                return new TotpVerificationResultDto
                {
                    IsValid = false, // Placeholder: would be true if code matches
                    ErrorMessage = null,
                    NextCodeValidAt = GetNextCodeValidTime(),
                    IsGracePeriodActive = false,
                    RemainingGraceAttempts = 1
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying TOTP for user {UserId}", userId);
                return new TotpVerificationResultDto
                {
                    IsValid = false,
                    ErrorMessage = "TOTP verification error"
                };
            }
        }

        public async Task<bool> VerifyBackupCodeAsync(int userId, string code)
        {
            try
            {
                if (string.IsNullOrEmpty(code) || code.Length != BackupCodeLength)
                {
                    return false;
                }

                // In a real implementation, check if backup code exists and hasn't been used
                // UPDATE backup_codes SET used = true WHERE user_id = userId AND code = code AND used = false

                _logger.LogInformation("Backup code verification for user {UserId}", userId);
                return false; // Placeholder
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying backup code for user {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> DisableAsync(int userId)
        {
            try
            {
                // In a real implementation: DELETE from user_totp_settings WHERE user_id = userId

                _logger.LogInformation("TOTP disabled for user {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disabling TOTP for user {UserId}", userId);
                return false;
            }
        }

        public async Task<int> GetRemainingBackupCodesAsync(int userId)
        {
            try
            {
                // In a real implementation: SELECT COUNT(*) FROM backup_codes WHERE user_id = userId AND used = false

                return 0; // Placeholder
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting backup codes count for user {UserId}", userId);
                return 0;
            }
        }

        public async Task<BackupCodesDto> RegenerateBackupCodesAsync(int userId)
        {
            try
            {
                // Mark old backup codes as invalid
                // INSERT new backup codes

                var newCodes = GenerateBackupCodes(_options.Value.BackupCodeCount);

                _logger.LogInformation("Backup codes regenerated for user {UserId}", userId);

                return new BackupCodesDto
                {
                    Codes = newCodes,
                    TotalCodes = newCodes.Count
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error regenerating backup codes for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> IsEnabledAsync(int userId)
        {
            try
            {
                // In a real implementation: SELECT COUNT(*) FROM user_totp_settings WHERE user_id = userId

                return false; // Placeholder
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking TOTP status for user {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Generates a 6-digit TOTP code using HMAC-SHA1 per RFC 6238.
        /// </summary>
        private string GenerateTotpCode(byte[] secretBytes, long timeCounter)
        {
            byte[] counterBytes = new byte[8];
            for (int i = 7; i >= 0; i--)
            {
                counterBytes[i] = (byte)(timeCounter & 0xFF);
                timeCounter >>= 8;
            }

            using (var hmac = new HMACSHA1(secretBytes))
            {
                byte[] hash = hmac.ComputeHash(counterBytes);
                int offset = hash[hash.Length - 1] & 0x0F;
                int value = (hash[offset] & 0x7F) << 24
                    | (hash[offset + 1] & 0xFF) << 16
                    | (hash[offset + 2] & 0xFF) << 8
                    | (hash[offset + 3] & 0xFF);

                int code = value % (int)Math.Pow(10, CodeLength);
                return code.ToString().PadLeft(CodeLength, '0');
            }
        }

        /// <summary>
        /// Gets the current UNIX time counter (time / 30 seconds).
        /// </summary>
        private long GetTimeCounter()
        {
            return (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds / 30;
        }

        /// <summary>
        /// Gets the time when the next TOTP code will be valid.
        /// </summary>
        private DateTime GetNextCodeValidTime()
        {
            long currentCounter = GetTimeCounter();
            long nextCounterTime = (currentCounter + 1) * 30;
            return new DateTime(1970, 1, 1).AddSeconds(nextCounterTime);
        }

        /// <summary>
        /// Generates backup codes (8-character random strings).
        /// </summary>
        private List<string> GenerateBackupCodes(int count)
        {
            var codes = new List<string>();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            for (int i = 0; i < count; i++)
            {
                var codeChars = new char[BackupCodeLength];
                using (var rng = RandomNumberGenerator.Create())
                {
                    byte[] randomBytes = new byte[BackupCodeLength];
                    rng.GetBytes(randomBytes);
                    for (int j = 0; j < BackupCodeLength; j++)
                    {
                        codeChars[j] = chars[randomBytes[j] % chars.Length];
                    }
                }
                codes.Add(new string(codeChars));
            }

            return codes;
        }

        /// <summary>
        /// Encodes bytes to Base32 (RFC 4648) format.
        /// </summary>
        private string Base32Encode(byte[] input)
        {
            if (input == null || input.Length == 0)
                return string.Empty;

            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
            var bits = new StringBuilder();
            foreach (byte b in input)
            {
                bits.Append(Convert.ToString(b, 2).PadLeft(8, '0'));
            }

            var result = new StringBuilder();
            for (int i = 0; i < bits.Length; i += 5)
            {
                int chunk = i + 5 > bits.Length ? int.Parse(bits.ToString(i, bits.Length - i).PadRight(5, '0'), System.Globalization.NumberStyles.AllowLeadingWhite) : int.Parse(bits.ToString(i, 5), System.Globalization.NumberStyles.AllowLeadingWhite);
                result.Append(alphabet[chunk]);
            }

            int padding = (8 - (input.Length * 8) % 5) % 5;
            result.Append(new string('=', padding / 5 * 8 / 5));

            return result.ToString();
        }

        /// <summary>
        /// Decodes Base32 (RFC 4648) format to bytes.
        /// </summary>
        private byte[] Base32Decode(string input)
        {
            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
            input = input.ToUpperInvariant().TrimEnd('=');
            var bits = new StringBuilder();

            foreach (char c in input)
            {
                int index = alphabet.IndexOf(c);
                if (index < 0)
                    throw new ArgumentException("Invalid Base32 character");
                bits.Append(Convert.ToString(index, 2).PadLeft(5, '0'));
            }

            var result = new List<byte>();
            for (int i = 0; i + 8 <= bits.Length; i += 8)
            {
                result.Add(Convert.ToByte(bits.ToString(i, 8), 2));
            }

            return result.ToArray();
        }

        /// <summary>
        /// Generates otpauth:// URL for QR code generation.
        /// </summary>
        private string GenerateOtpauthUrl(string email, string secret)
        {
            string issuer = _options.Value.IssuerName;
            string accountName = Uri.EscapeDataString($"{issuer}:{email}");
            string encodedSecret = Uri.EscapeDataString(secret);
            return $"otpauth://totp/{accountName}?secret={encodedSecret}&issuer={Uri.EscapeDataString(issuer)}&algorithm=SHA1&digits=6&period=30";
        }

        /// <summary>
        /// Formats Base32 secret for manual entry (groups of 4 characters).
        /// </summary>
        private string FormatManualEntryKey(string base32Secret)
        {
            var formatted = new StringBuilder();
            for (int i = 0; i < base32Secret.Length; i++)
            {
                if (i > 0 && i % 4 == 0)
                    formatted.Append(" ");
                formatted.Append(base32Secret[i]);
            }
            return formatted.ToString();
        }
    }
}
