// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for PCI-compliant payment tokenization.
/// Handles secure card data tokenization for payment processing.
/// </summary>
public interface IPaymentTokenizationService
{
    /// <summary>
    /// Tokenizes card details securely.
    /// Card data is NOT stored; only a token reference is returned.
    /// </summary>
    /// <param name="request">Card details to tokenize</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Tokenization result with secure token</returns>
    Task<PaymentTokenizationResultDto> TokenizeCardAsync(
        PaymentTokenizationRequestDto request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Charges a payment token for a specified amount.
    /// </summary>
    /// <param name="request">Charge request with token and amount</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Charge result</returns>
    Task<ChargeResultDto> ChargeTokenAsync(
        ChargeTokenRequestDto request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a payment token is still valid.
    /// </summary>
    /// <param name="token">Token to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if valid, false otherwise</returns>
    Task<bool> ValidateTokenAsync(
        string token,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes/deletes a payment token.
    /// </summary>
    /// <param name="token">Token to revoke</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if revoked successfully</returns>
    Task<bool> RevokeTokenAsync(
        string token,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets stored payment methods for a customer (tokens only, no card data).
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of stored payment tokens</returns>
    Task<IEnumerable<PaymentTokenizationResultDto>> GetCustomerPaymentMethodsAsync(
        int customerId,
        CancellationToken cancellationToken = default);
}
