// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

namespace CRM.Core.Dtos;

/// <summary>
/// DTO for 2FA login verification
/// </summary>
public class TwoFactorLoginRequest
{
    /// <summary>
    /// Temporary token received from initial login response
    /// </summary>
    public string TwoFactorToken { get; set; } = string.Empty;

    /// <summary>
    /// TOTP code from authenticator app or backup code
    /// </summary>
    public string Code { get; set; } = string.Empty;
}
