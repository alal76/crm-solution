// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Security.Cryptography;
using CRM.Core.Dtos;
using CRM.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Implementation of IPaymentTokenizationService for PCI-compliant payment tokenization.
/// This is a stub implementation that simulates tokenization.
/// In production, integrate with a PCI-compliant payment gateway (Stripe, Braintree, etc.).
/// </summary>
/// <remarks>
/// DEPRECATED: Confirmed dead code — no controller or other service injects this type (only the DI
/// registration in Program.cs references it). It stores raw card numbers in an in-memory dictionary and
/// always fakes a successful charge with a generated charge ID, which is not PCI-compliant. Real payment
/// processing already goes through <see cref="CRM.Infrastructure.Services.StripeIntegrationService"/>.
/// Kept as a working shim rather than deleted; do not wire this into any controller.
/// </remarks>
[Obsolete("Superseded by StripeIntegrationService for real payment processing; this service used insecure in-memory raw card storage and is not PCI-compliant. Do not use in new code.")]
public class PaymentTokenizationService : IPaymentTokenizationService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<PaymentTokenizationService> _logger;

    // In-memory token storage (for stub purposes only - use secure storage in production)
    private static readonly Dictionary<string, TokenEntry> _tokenStore = new();
    private static readonly object _lock = new();

    public PaymentTokenizationService(ICrmDbContext context, ILogger<PaymentTokenizationService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public Task<PaymentTokenizationResultDto> TokenizeCardAsync(
        PaymentTokenizationRequestDto request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate card number (basic Luhn check)
            if (string.IsNullOrWhiteSpace(request.CardNumber) || request.CardNumber.Length < 13)
            {
                return Task.FromResult(new PaymentTokenizationResultDto
                {
                    Success = false,
                    ErrorMessage = "Invalid card number."
                });
            }

            if (string.IsNullOrWhiteSpace(request.SecurityCode))
            {
                return Task.FromResult(new PaymentTokenizationResultDto
                {
                    Success = false,
                    ErrorMessage = "Security code is required."
                });
            }

            if (request.ExpirationMonth < 1 || request.ExpirationMonth > 12)
            {
                return Task.FromResult(new PaymentTokenizationResultDto
                {
                    Success = false,
                    ErrorMessage = "Invalid expiration month."
                });
            }

            var now = DateTime.UtcNow;
            if (request.ExpirationYear < now.Year ||
                (request.ExpirationYear == now.Year && request.ExpirationMonth < now.Month))
            {
                return Task.FromResult(new PaymentTokenizationResultDto
                {
                    Success = false,
                    ErrorMessage = "Card is expired."
                });
            }

            // Generate secure token
            var token = GenerateToken();
            var lastFour = request.CardNumber[^4..];
            var cardBrand = DetectCardBrand(request.CardNumber);

            var entry = new TokenEntry
            {
                Token = token,
                LastFourDigits = lastFour,
                CardBrand = cardBrand,
                ExpirationMonth = request.ExpirationMonth,
                ExpirationYear = request.ExpirationYear,
                CardholderName = request.CardholderName,
                CustomerId = request.CustomerId,
                CreatedAt = now,
                IsActive = true
            };

            lock (_lock)
            {
                _tokenStore[token] = entry;
            }

            _logger.LogInformation(
                "Card tokenized successfully. Token={Token}, Brand={Brand}, Last4={Last4}, CustomerId={CustomerId}",
                token[..8] + "...", cardBrand, lastFour, request.CustomerId);

            return Task.FromResult(new PaymentTokenizationResultDto
            {
                Token = token,
                LastFourDigits = lastFour,
                CardBrand = cardBrand,
                ExpirationMonth = request.ExpirationMonth,
                ExpirationYear = request.ExpirationYear,
                CreatedAt = now,
                Success = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tokenizing card");
            return Task.FromResult(new PaymentTokenizationResultDto
            {
                Success = false,
                ErrorMessage = "An error occurred during tokenization."
            });
        }
    }

    /// <inheritdoc />
    public Task<ChargeResultDto> ChargeTokenAsync(
        ChargeTokenRequestDto request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Token))
            {
                return Task.FromResult(new ChargeResultDto
                {
                    Success = false,
                    Status = "failed",
                    ErrorMessage = "Payment token is required.",
                    ErrorCode = "missing_token"
                });
            }

            if (request.Amount <= 0)
            {
                return Task.FromResult(new ChargeResultDto
                {
                    Success = false,
                    Status = "failed",
                    ErrorMessage = "Amount must be greater than zero.",
                    ErrorCode = "invalid_amount"
                });
            }

            TokenEntry? entry;
            lock (_lock)
            {
                _tokenStore.TryGetValue(request.Token, out entry);
            }

            if (entry == null || !entry.IsActive)
            {
                return Task.FromResult(new ChargeResultDto
                {
                    Success = false,
                    Status = "failed",
                    ErrorMessage = "Invalid or expired payment token.",
                    ErrorCode = "invalid_token"
                });
            }

            // Check card expiration
            var now = DateTime.UtcNow;
            if (entry.ExpirationYear < now.Year ||
                (entry.ExpirationYear == now.Year && entry.ExpirationMonth < now.Month))
            {
                return Task.FromResult(new ChargeResultDto
                {
                    Success = false,
                    Status = "failed",
                    ErrorMessage = "Card associated with token has expired.",
                    ErrorCode = "card_expired"
                });
            }

            // Simulate successful charge
            var chargeId = $"ch_{Guid.NewGuid():N}";

            _logger.LogInformation(
                "Charge successful. ChargeId={ChargeId}, Amount={Amount} {Currency}, Token={Token}",
                chargeId, request.Amount, request.Currency, request.Token[..8] + "...");

            return Task.FromResult(new ChargeResultDto
            {
                ChargeId = chargeId,
                Success = true,
                Status = "succeeded",
                Amount = request.Amount,
                Currency = request.Currency,
                ChargedAt = DateTime.UtcNow,
                ReceiptUrl = $"https://receipts.example.com/{chargeId}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error charging token");
            return Task.FromResult(new ChargeResultDto
            {
                Success = false,
                Status = "failed",
                ErrorMessage = "An error occurred during charge processing.",
                ErrorCode = "processing_error"
            });
        }
    }

    /// <inheritdoc />
    public Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return Task.FromResult(false);
        }

        lock (_lock)
        {
            if (_tokenStore.TryGetValue(token, out var entry))
            {
                if (!entry.IsActive) return Task.FromResult(false);

                var now = DateTime.UtcNow;
                if (entry.ExpirationYear < now.Year ||
                    (entry.ExpirationYear == now.Year && entry.ExpirationMonth < now.Month))
                {
                    entry.IsActive = false;
                    return Task.FromResult(false);
                }

                return Task.FromResult(true);
            }
        }

        return Task.FromResult(false);
    }

    /// <inheritdoc />
    public Task<bool> RevokeTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return Task.FromResult(false);
        }

        lock (_lock)
        {
            if (_tokenStore.TryGetValue(token, out var entry))
            {
                entry.IsActive = false;
                _logger.LogInformation("Token revoked: {Token}", token[..8] + "...");
                return Task.FromResult(true);
            }
        }

        return Task.FromResult(false);
    }

    /// <inheritdoc />
    public Task<IEnumerable<PaymentTokenizationResultDto>> GetCustomerPaymentMethodsAsync(
        int customerId,
        CancellationToken cancellationToken = default)
    {
        List<PaymentTokenizationResultDto> results;

        lock (_lock)
        {
            results = _tokenStore.Values
                .Where(t => t.CustomerId == customerId && t.IsActive)
                .Select(t => new PaymentTokenizationResultDto
                {
                    Token = t.Token,
                    LastFourDigits = t.LastFourDigits,
                    CardBrand = t.CardBrand,
                    ExpirationMonth = t.ExpirationMonth,
                    ExpirationYear = t.ExpirationYear,
                    CreatedAt = t.CreatedAt,
                    Success = true
                })
                .ToList();
        }

        return Task.FromResult<IEnumerable<PaymentTokenizationResultDto>>(results);
    }

    #region Private Helpers

    private static string GenerateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return $"tok_{Convert.ToHexString(bytes).ToLowerInvariant()}";
    }

    private static string DetectCardBrand(string cardNumber)
    {
        var cleaned = cardNumber.Replace(" ", "").Replace("-", "");
        return cleaned[0] switch
        {
            '4' => "Visa",
            '5' => "MasterCard",
            '3' when cleaned.Length >= 2 && (cleaned[1] == '4' || cleaned[1] == '7') => "American Express",
            '6' => "Discover",
            _ => "Unknown"
        };
    }

    #endregion

    #region Internal Types

    private class TokenEntry
    {
        public string Token { get; set; } = string.Empty;
        public string LastFourDigits { get; set; } = string.Empty;
        public string CardBrand { get; set; } = string.Empty;
        public int ExpirationMonth { get; set; }
        public int ExpirationYear { get; set; }
        public string CardholderName { get; set; } = string.Empty;
        public int? CustomerId { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    #endregion
}
