using System.Security.Cryptography;
using System.Text;
using CRM.Core.Entities;

namespace CRM.Api.Helpers;

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
