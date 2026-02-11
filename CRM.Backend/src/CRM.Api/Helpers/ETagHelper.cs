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

using System.Security.Cryptography;
using System.Text;
using CRM.Core.Entities;

namespace CRM.Api.Helpers;

#pragma warning disable SA1011 // Closing square bracket should be followed by a space

/// <summary>
/// Helper class for ETag (Entity Tag) operations for optimistic concurrency control
/// </summary>
public static class ETagHelper
{
    /// <summary>
    /// Generates an ETag from a RowVersion byte array
    /// </summary>
    public static string GenerateETag(byte[]? rowVersion)
    {
        if (rowVersion == null || rowVersion.Length == 0)
            return "\"0\"";

        return $"\"{Convert.ToBase64String(rowVersion)}\"";
    }

    /// <summary>
    /// Generates an ETag from a BaseEntity's RowVersion
    /// </summary>
    public static string GenerateETag(BaseEntity entity)
    {
        return GenerateETag(entity.RowVersion);
    }

    /// <summary>
    /// Parses an ETag string to a byte array
    /// </summary>
    public static byte[]? ParseETag(string? etag)
    {
        if (string.IsNullOrWhiteSpace(etag))
            return null;

        // Remove quotes if present
        var cleanEtag = etag.Trim('"');

        if (cleanEtag == "0")
            return null;

        try
        {
            return Convert.FromBase64String(cleanEtag);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Checks if the If-Match header matches the current RowVersion
    /// </summary>
    public static bool IsMatch(string? ifMatch, byte[]? currentRowVersion)
    {
        if (string.IsNullOrWhiteSpace(ifMatch))
            return true; // No If-Match header means unconditional update

        if (ifMatch == "*")
            return true; // Wildcard matches everything

        var requestedVersion = ParseETag(ifMatch);

        if (requestedVersion == null || currentRowVersion == null)
            return false;

        return requestedVersion.SequenceEqual(currentRowVersion);
    }

    /// <summary>
    /// Checks if the If-None-Match header doesn't match (for GET caching)
    /// </summary>
    public static bool IsNoneMatch(string? ifNoneMatch, byte[]? currentRowVersion)
    {
        if (string.IsNullOrWhiteSpace(ifNoneMatch))
            return true; // No header means return the resource

        var currentEtag = GenerateETag(currentRowVersion);

        // If ETags match, return false (304 Not Modified should be returned)
        return !ifNoneMatch.Split(',').Any(tag =>
            tag.Trim().Equals(currentEtag, StringComparison.OrdinalIgnoreCase) ||
            tag.Trim() == "*");
    }
}
