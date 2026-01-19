using System.Security.Cryptography;
using System.Text;

namespace CRM.Infrastructure.Services;

/// <summary>
/// TOTP (Time-based One-Time Password) service for two-factor authentication
/// </summary>
public interface ITotpService
{
    string GenerateSecret();
    bool VerifyCode(string secret, string code);
    string GetQrCodeUrl(string secret, string email, string issuer);
    List<string> GenerateBackupCodes(int count = 10);
}

public class TotpService : ITotpService
{
    private const int TimeStepSeconds = 30;
    private const int CodeDigits = 6;

    public string GenerateSecret()
    {
        var randomBytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }
        return Convert.ToBase64String(randomBytes);
    }

    public bool VerifyCode(string secret, string code)
    {
        if (string.IsNullOrEmpty(secret) || string.IsNullOrEmpty(code))
            return false;

        var secretBytes = Convert.FromBase64String(secret);
        var codeValue = code.Trim();

        if (!int.TryParse(codeValue, out var codeInt) || codeValue.Length != CodeDigits)
            return false;

        // Check current time window and ±1 time windows for tolerance
        var unixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        for (int i = -1; i <= 1; i++)
        {
            var timeWindow = (unixTime / TimeStepSeconds) + i;
            var hmac = new HMACSHA1(secretBytes);
            var hash = hmac.ComputeHash(BitConverter.GetBytes(timeWindow));
            var offset = hash[hash.Length - 1] & 0x0f;
            var truncated = (hash[offset] & 0x7f) << 24
                | (hash[offset + 1] & 0xff) << 16
                | (hash[offset + 2] & 0xff) << 8
                | (hash[offset + 3] & 0xff);
            var totp = truncated % (int)Math.Pow(10, CodeDigits);

            if (totp == codeInt)
                return true;
        }

        return false;
    }

    public string GetQrCodeUrl(string secret, string email, string issuer)
    {
        var encodedSecret = Uri.EscapeDataString(secret);
        var encodedEmail = Uri.EscapeDataString(email);
        var encodedIssuer = Uri.EscapeDataString(issuer);

        return $"otpauth://totp/{encodedEmail}?secret={encodedSecret}&issuer={encodedIssuer}";
    }

    public List<string> GenerateBackupCodes(int count = 10)
    {
        var codes = new List<string>();
        for (int i = 0; i < count; i++)
        {
            var randomBytes = new byte[8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            var code = BitConverter.ToString(randomBytes).Replace("-", "").Substring(0, 16);
            codes.Add(code);
        }
        return codes;
    }
}
