// CRM Solution - Customer Relationship Management System// CRM Solution - Customer Relationship Management System






























































































































































































































































































































}    }        public string CardHash { get; set; } = string.Empty;        public DateTime CreatedAt { get; set; }        public int? CustomerId { get; set; }        public string? CardholderName { get; set; }        public int ExpirationYear { get; set; }        public int ExpirationMonth { get; set; }        public string CardBrand { get; set; } = string.Empty;        public string LastFourDigits { get; set; } = string.Empty;        public string Token { get; set; } = string.Empty;    {    private class TokenEntry    #endregion    }        return "Unknown";            return "Discover";        if (cardNumber.StartsWith("6"))            return "AmericanExpress";        if (cardNumber.StartsWith("34") || cardNumber.StartsWith("37"))            return "MasterCard";        if (cardNumber.StartsWith("5") || cardNumber.StartsWith("2"))            return "Visa";        if (cardNumber.StartsWith("4"))            return "Unknown";        if (string.IsNullOrEmpty(cardNumber))    {    private static string DetectCardBrand(string cardNumber)    }        return true;            return false;        if (year == now.Year && month < now.Month)            return false;        if (year < now.Year)        var now = DateTime.UtcNow;            return false;        if (month < 1 || month > 12)    {    private static bool IsValidExpiration(int month, int year)    }        return sum % 10 == 0;        }            alternate = !alternate;            sum += digit;            }                    digit -= 9;                if (digit > 9)                digit *= 2;            {            if (alternate)            var digit = cardNumber[i] - '0';                return false;            if (!char.IsDigit(cardNumber[i]))        {        for (var i = cardNumber.Length - 1; i >= 0; i--)        var alternate = false;        var sum = 0;        // Luhn algorithm            return false;        if (string.IsNullOrEmpty(cardNumber) || cardNumber.Length < 13 || cardNumber.Length > 19)    {    private static bool IsValidCardNumber(string cardNumber)    }        return Convert.ToBase64String(bytes);        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));        using var sha256 = SHA256.Create();    {    private static string ComputeHash(string input)    }        return $"tok_{Convert.ToBase64String(bytes).Replace("+", "").Replace("/", "").Replace("=", "").Substring(0, 24)}";        rng.GetBytes(bytes);        using var rng = RandomNumberGenerator.Create();        var bytes = new byte[32];    {    private static string GenerateSecureToken()    #region Private Helpers    }        return Task.FromResult<IEnumerable<PaymentTokenizationResultDto>>(customerTokens);            .ToList();            })                CreatedAt = t.CreatedAt                Success = true,                ExpirationYear = t.ExpirationYear,                ExpirationMonth = t.ExpirationMonth,                CardBrand = t.CardBrand,                LastFourDigits = t.LastFourDigits,                Token = t.Token,            {            .Select(t => new PaymentTokenizationResultDto            .Where(t => t.CustomerId == customerId)        var customerTokens = _tokenStore.Values    {        CancellationToken cancellationToken = default)        int customerId,    public Task<IEnumerable<PaymentTokenizationResultDto>> GetCustomerPaymentMethodsAsync(    }        return Task.FromResult(removed);        }            _logger.LogInformation("Token revoked: {TokenPrefix}...", token.Substring(0, 8));        {        if (removed)        var removed = _tokenStore.Remove(token);            return Task.FromResult(false);        if (string.IsNullOrEmpty(token))    {    public Task<bool> RevokeTokenAsync(string token, CancellationToken cancellationToken = default)    }        return Task.FromResult(true);        }            return Task.FromResult(false);        {            (entry.ExpirationYear == now.Year && entry.ExpirationMonth < now.Month))        if (entry.ExpirationYear < now.Year ||        var now = DateTime.UtcNow;        // Check if card has expired            return Task.FromResult(false);        if (!_tokenStore.TryGetValue(token, out var entry))            return Task.FromResult(false);        if (string.IsNullOrEmpty(token))    {    public Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)    }        }            };                ErrorCode = "processing_error"                ErrorMessage = "An error occurred during charge processing",                Success = false,            {            return new ChargeResultDto            _logger.LogError(ex, "Error charging token");        {        catch (Exception ex)        }            };                ReceiptUrl = $"https://receipts.example.com/{chargeId}"                ChargedAt = DateTime.UtcNow,                Currency = request.Currency,                Amount = request.Amount,                Status = "succeeded",                Success = true,                ChargeId = chargeId,            {            return new ChargeResultDto                chargeId, request.Amount, request.Currency);            _logger.LogInformation("Charge processed successfully. ChargeId: {ChargeId}, Amount: {Amount} {Currency}",            var chargeId = $"ch_{Guid.NewGuid():N}";            // In production, this would call the payment gateway API            // Simulate charge processing            }                };                    ErrorCode = "invalid_amount"                    ErrorMessage = "Amount must be greater than zero",                    Success = false,                {                return new ChargeResultDto            {            if (request.Amount <= 0)            // Validate amount            }                _logger.LogInformation("Idempotency key provided: {Key}", request.IdempotencyKey);                // In production, check idempotency key storage            {            if (!string.IsNullOrEmpty(request.IdempotencyKey))            // Check for duplicate charge (idempotency)            }                };                    ErrorCode = "invalid_token"                    ErrorMessage = "Invalid or expired token",                    Success = false,                {                return new ChargeResultDto            {            if (!_tokenStore.TryGetValue(request.Token, out var tokenEntry))            // Validate token exists        {        try    {        CancellationToken cancellationToken = default)        ChargeTokenRequestDto request,    public async Task<ChargeResultDto> ChargeTokenAsync(    }        }            };                ErrorMessage = "An error occurred during tokenization"                Success = false,            {            return new PaymentTokenizationResultDto            _logger.LogError(ex, "Error tokenizing card");        {        catch (Exception ex)        }            };                CreatedAt = DateTime.UtcNow                Success = true,                ExpirationYear = request.ExpirationYear,                ExpirationMonth = request.ExpirationMonth,                CardBrand = cardBrand,                LastFourDigits = lastFour,                Token = token,            {            return new PaymentTokenizationResultDto                token.Substring(0, 8), cardBrand, lastFour);            _logger.LogInformation("Card tokenized successfully. Token: {TokenPrefix}..., Brand: {Brand}, Last4: {Last4}",            _tokenStore[token] = tokenEntry;            };                CardHash = ComputeHash(request.CardNumber + request.ExpirationMonth + request.ExpirationYear)                // Store encrypted hash of card for duplicate detection (NOT the card itself)                CreatedAt = DateTime.UtcNow,                CustomerId = request.CustomerId,                CardholderName = request.CardholderName,                ExpirationYear = request.ExpirationYear,                ExpirationMonth = request.ExpirationMonth,                CardBrand = cardBrand,                LastFourDigits = lastFour,                Token = token,            {            var tokenEntry = new TokenEntry            // Store encrypted reference (NOT the actual card data)            var cardBrand = DetectCardBrand(request.CardNumber);            var lastFour = request.CardNumber.Substring(request.CardNumber.Length - 4);            var token = GenerateSecureToken();            // Generate secure token            }                };                    ErrorMessage = "Card has expired or invalid expiration date"                    Success = false,                {                return new PaymentTokenizationResultDto            {            if (!IsValidExpiration(request.ExpirationMonth, request.ExpirationYear))            // Validate expiration            }                };                    ErrorMessage = "Invalid card number"                    Success = false,                {                return new PaymentTokenizationResultDto            {            if (!IsValidCardNumber(request.CardNumber))            // Validate card number (basic Luhn check)        {        try    {        CancellationToken cancellationToken = default)        PaymentTokenizationRequestDto request,    public async Task<PaymentTokenizationResultDto> TokenizeCardAsync(    }        _logger = logger ?? throw new ArgumentNullException(nameof(logger));        _context = context ?? throw new ArgumentNullException(nameof(context));    {    public PaymentTokenizationService(ICrmDbContext context, ILogger<PaymentTokenizationService> logger)    private static readonly Dictionary<string, TokenEntry> _tokenStore = new();    // In-memory token storage (for stub purposes only - use secure storage in production)        private readonly ILogger<PaymentTokenizationService> _logger;    private readonly ICrmDbContext _context;{public class PaymentTokenizationService : IPaymentTokenizationService/// </summary>/// In production, integrate with a PCI-compliant payment gateway (Stripe, Braintree, etc.)./// This is a stub implementation that simulates tokenization./// Implementation of IPaymentTokenizationService for PCI-compliant payment tokenization./// <summary>namespace CRM.Infrastructure.Services;using System.Text.Json;using System.Text;using System.Security.Cryptography;using Microsoft.Extensions.Logging;using Microsoft.EntityFrameworkCore;using CRM.Infrastructure.Data;using CRM.Core.Interfaces;using CRM.Core.Entities;using CRM.Core.Dtos;// See the LICENSE file in the root directory for full terms.// the terms of the LICENSE file. Commercial use requires a separate license.// This software is source-available. Non-commercial use is permitted under//// Copyright (C) 2024-2026 Abhishek Lal// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Security.Cryptography;
using System.Text;
using CRM.Core.Dtos;
using CRM.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// PCI-compliant payment tokenization service.
/// This is a stub implementation - in production, integrate with a real payment gateway.
/// </summary>
public class PaymentTokenizationService : IPaymentTokenizationService
{
    private readonly ILogger<PaymentTokenizationService> _logger;
    
    // In-memory token store (in production, use secure encrypted storage)
    private static readonly Dictionary<string, TokenizedCard> _tokenStore = new();
    private static readonly object _lock = new();

    public PaymentTokenizationService(ILogger<PaymentTokenizationService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<PaymentTokenizationResultDto> TokenizeCardAsync(
        PaymentTokenizationRequestDto request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate card number format
            if (string.IsNullOrWhiteSpace(request.CardNumber) || request.CardNumber.Length < 13)
            {
                return Task.FromResult(new PaymentTokenizationResultDto
                {
                    Success = false,
                    ErrorMessage = "Invalid card number format"
                });
            }

            // Generate secure token
            var token = GenerateSecureToken();
            var lastFour = request.CardNumber.Length >= 4 
                ? request.CardNumber[^4..] 
                : request.CardNumber;
            var cardBrand = DetectCardBrand(request.CardNumber);

            // Store token reference (NOT the actual card data in production)
            var tokenizedCard = new TokenizedCard
            {
                Token = token,
                LastFourDigits = lastFour,
                CardBrand = cardBrand,
                ExpirationMonth = request.ExpirationMonth,
                ExpirationYear = request.ExpirationYear,
                CardholderName = request.CardholderName,
                CustomerId = request.CustomerId,
                CreatedAt = DateTime.UtcNow,
                // Store encrypted reference (stub - in production, send to payment gateway)
                EncryptedReference = EncryptCardReference(request.CardNumber)
            };

            lock (_lock)
            {
                _tokenStore[token] = tokenizedCard;
            }

            _logger.LogInformation("Card tokenized successfully: {LastFour}, Brand: {Brand}", lastFour, cardBrand);

            return Task.FromResult(new PaymentTokenizationResultDto
            {
                Token = token,
                LastFourDigits = lastFour,
                CardBrand = cardBrand,
                ExpirationMonth = request.ExpirationMonth,
                ExpirationYear = request.ExpirationYear,
                CreatedAt = DateTime.UtcNow,
                Success = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tokenizing card");
            return Task.FromResult(new PaymentTokenizationResultDto
            {
                Success = false,
                ErrorMessage = "Tokenization failed"
            });
        }
    }

    public Task<ChargeResultDto> ChargeTokenAsync(
        ChargeTokenRequestDto request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate token exists
            TokenizedCard? card;
            lock (_lock)
            {
                if (!_tokenStore.TryGetValue(request.Token, out card))
                {
                    return Task.FromResult(new ChargeResultDto
                    {
                        Success = false,
                        Status = "failed",
                        ErrorMessage = "Invalid or expired payment token",
                        ErrorCode = "INVALID_TOKEN"
                    });
                }
            }

            // Check expiration
            var now = DateTime.UtcNow;
            if (card.ExpirationYear < now.Year || 
                (card.ExpirationYear == now.Year && card.ExpirationMonth < now.Month))
            {
                return Task.FromResult(new ChargeResultDto
                {
                    Success = false,
                    Status = "failed",
                    ErrorMessage = "Card has expired",
                    ErrorCode = "CARD_EXPIRED"
                });
            }

            // Validate amount
            if (request.Amount <= 0)
            {
                return Task.FromResult(new ChargeResultDto
                {
                    Success = false,
                    Status = "failed",
                    ErrorMessage = "Invalid charge amount",
                    ErrorCode = "INVALID_AMOUNT"
                });
            }

            // Simulate charge (in production, call actual payment gateway)
            var chargeId = $"ch_{Guid.NewGuid():N}";

            _logger.LogInformation("Charge processed: {ChargeId}, Amount: {Amount} {Currency}", 
                chargeId, request.Amount, request.Currency);

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
                ErrorMessage = "Charge processing failed",
                ErrorCode = "PROCESSING_ERROR"
            });
        }
    }

    public Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            if (!_tokenStore.TryGetValue(token, out var card))
            {
                return Task.FromResult(false);
            }

            // Check if card is expired
            var now = DateTime.UtcNow;
            if (card.ExpirationYear < now.Year || 
                (card.ExpirationYear == now.Year && card.ExpirationMonth < now.Month))
            {
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }
    }

    public Task<bool> RevokeTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var removed = _tokenStore.Remove(token);
            if (removed)
            {
                _logger.LogInformation("Token revoked: {Token}", token[..8] + "...");
            }
            return Task.FromResult(removed);
        }
    }

    public Task<IEnumerable<PaymentTokenizationResultDto>> GetCustomerPaymentMethodsAsync(
        int customerId,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var methods = _tokenStore.Values
                .Where(c => c.CustomerId == customerId)
                .Select(c => new PaymentTokenizationResultDto
                {
                    Token = c.Token,
                    LastFourDigits = c.LastFourDigits,
                    CardBrand = c.CardBrand,
                    ExpirationMonth = c.ExpirationMonth,
                    ExpirationYear = c.ExpirationYear,
                    CreatedAt = c.CreatedAt,
                    Success = true
                })
                .ToList();

            return Task.FromResult<IEnumerable<PaymentTokenizationResultDto>>(methods);
        }
    }

    #region Private Helper Methods

    private static string GenerateSecureToken()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[32];
        rng.GetBytes(bytes);
        return $"tok_{Convert.ToBase64String(bytes).Replace("+", "").Replace("/", "").Replace("=", "")[..24]}";
    }

    private static string DetectCardBrand(string cardNumber)
    {
        if (string.IsNullOrEmpty(cardNumber)) return "Unknown";

        var number = cardNumber.Replace(" ", "").Replace("-", "");
        
        if (number.StartsWith("4")) return "Visa";
        if (number.StartsWith("5") && number.Length >= 2 && int.Parse(number[1].ToString()) >= 1 && int.Parse(number[1].ToString()) <= 5) return "MasterCard";
        if (number.StartsWith("34") || number.StartsWith("37")) return "American Express";
        if (number.StartsWith("6011") || number.StartsWith("65")) return "Discover";
        if (number.StartsWith("35")) return "JCB";
        if (number.StartsWith("30") || number.StartsWith("36") || number.StartsWith("38")) return "Diners Club";
        
        return "Unknown";
    }

    private static string EncryptCardReference(string cardNumber)
    {
        // Stub: In production, use proper encryption or send to payment gateway
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(cardNumber + DateTime.UtcNow.Ticks));
        return Convert.ToBase64String(hash);
    }

    #endregion

    #region Private Classes

    private class TokenizedCard
    {
        public string Token { get; set; } = string.Empty;
        public string LastFourDigits { get; set; } = string.Empty;
        public string CardBrand { get; set; } = string.Empty;
        public int ExpirationMonth { get; set; }
        public int ExpirationYear { get; set; }
        public string CardholderName { get; set; } = string.Empty;
        public int? CustomerId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string EncryptedReference { get; set; } = string.Empty;
    }

    #endregion
}
