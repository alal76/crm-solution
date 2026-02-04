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

using CRM.Core.Entities;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for normalization operations (tags, custom fields, contact info)
/// </summary>
public interface INormalizationService
{
    /// <summary>
    /// Get tags for an entity
    /// </summary>
    Task<string?> GetTagsAsync(string entityType, int entityId);

    /// <summary>
    /// Get custom fields for an entity
    /// </summary>
    Task<string?> GetCustomFieldsAsync(string entityType, int entityId);

    /// <summary>
    /// Get primary contact detail value
    /// </summary>
    Task<string?> GetPrimaryContactDetailValueAsync(ContactInfoOwnerType ownerType, int ownerId, ContactDetailType detailType);

    /// <summary>
    /// Get primary email for an entity
    /// </summary>
    Task<string?> GetPrimaryEmailAsync(ContactInfoOwnerType ownerType, int ownerId);

    /// <summary>
    /// Get primary phone for an entity
    /// </summary>
    Task<string?> GetPrimaryPhoneAsync(ContactInfoOwnerType ownerType, int ownerId);

    /// <summary>
    /// Get primary fax for an entity
    /// </summary>
    Task<string?> GetPrimaryFaxAsync(ContactInfoOwnerType ownerType, int ownerId);

    /// <summary>
    /// Get primary address for an entity
    /// </summary>
    Task<Address?> GetPrimaryAddressAsync(ContactInfoOwnerType ownerType, int ownerId);

    /// <summary>
    /// Get primary social account for an entity
    /// </summary>
    Task<string?> GetPrimarySocialAccountAsync(ContactInfoOwnerType ownerType, int ownerId, SocialNetwork network);
}
