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

namespace CRM.Core.Entities;

/// <summary>
/// Stored WebAuthn credential for passwordless authentication.
/// </summary>
public class WebAuthnCredential : BaseEntity
{
    public int UserId { get; set; }
    public string CredentialId { get; set; } = string.Empty;
    public byte[] CredentialIdBytes { get; set; } = Array.Empty<byte>();
    public byte[] PublicKey { get; set; } = Array.Empty<byte>();
    public long SignatureCounter { get; set; }
    public string AttestationFormat { get; set; } = string.Empty;
    public List<string>? Transports { get; set; }
    public string Name { get; set; } = string.Empty;
}
