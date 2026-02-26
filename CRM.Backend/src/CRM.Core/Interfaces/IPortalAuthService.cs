// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;

namespace CRM.Core.Interfaces;

/// <summary>
/// Authentication contract for the Customer Portal.
/// Handles portal-specific login, registration, password reset and email verification.
/// </summary>
public interface IPortalAuthService
{
    /// <summary>Authenticate a portal user and return a signed JWT.</summary>
    Task<PortalTokenResponseDto?> LoginAsync(PortalLoginDto dto, CancellationToken ct = default);

    /// <summary>Register a new portal user.  Throws if email already exists or portal is disabled.</summary>
    Task<PortalUserDto> RegisterAsync(PortalRegisterDto dto, CancellationToken ct = default);

    /// <summary>Initiate a password reset flow — sets a reset token on the user record.</summary>
    Task<bool> ForgotPasswordAsync(string email, CancellationToken ct = default);

    /// <summary>Complete a password reset using a token.</summary>
    Task<bool> ResetPasswordAsync(string token, string newPassword, CancellationToken ct = default);

    /// <summary>Verify an email address using the verification token.</summary>
    Task<bool> VerifyEmailAsync(string token, CancellationToken ct = default);
}
