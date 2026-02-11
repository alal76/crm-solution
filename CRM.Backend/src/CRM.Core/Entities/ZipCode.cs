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
/// Master data entity for postal codes / ZIP codes
/// Used for address auto-population when user enters a postal/zip code
/// Supports cascading dropdown selection: Country > State > City > Postal Code
/// </summary>
public class ZipCode
{
    public int Id { get; set; }

    /// <summary>
    /// Country name (e.g., "United States", "Canada")
    /// </summary>
    public string Country { get; set; } = string.Empty;

    /// <summary>
    /// ISO 3166-1 alpha-2 country code (e.g., "US", "CA")
    /// </summary>
    public string CountryCode { get; set; } = string.Empty;

    /// <summary>
    /// Postal/ZIP code (e.g., "90210", "V6B 1A9")
    /// </summary>
    public string PostalCode { get; set; } = string.Empty;

    /// <summary>
    /// City/locality name
    /// </summary>
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// State/province/region name
    /// </summary>
    public string? State { get; set; }

    /// <summary>
    /// State/province/region code (e.g., "CA", "NY")
    /// </summary>
    public string? StateCode { get; set; }

    /// <summary>
    /// County name (where applicable)
    /// </summary>
    public string? County { get; set; }

    /// <summary>
    /// County code (where applicable)
    /// </summary>
    public string? CountyCode { get; set; }

    /// <summary>
    /// Community/municipality name
    /// </summary>
    public string? Community { get; set; }

    /// <summary>
    /// Community/municipality code
    /// </summary>
    public string? CommunityCode { get; set; }

    /// <summary>
    /// Geographic latitude
    /// </summary>
    public decimal? Latitude { get; set; }

    /// <summary>
    /// Geographic longitude
    /// </summary>
    public decimal? Longitude { get; set; }

    /// <summary>
    /// Accuracy level of coordinates (1-6 scale, 6 being most accurate)
    /// </summary>
    public int? Accuracy { get; set; }

    /// <summary>
    /// Is this record active/valid
    /// </summary>
    public bool IsActive { get; set; } = true;

    // Navigation Properties
    public ICollection<Locality>? Localities { get; set; }
    public ICollection<Address>? Addresses { get; set; }
}
