// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
#pragma warning disable SA1649 // file name should match first type name
namespace CRM.Core.Dtos;

/// <summary>
/// Generic pagination wrapper for paginated responses.
/// </summary>
/// <typeparam name="T">The type of items in the paginated result.</typeparam>
public class PaginatedDto<T>
{
    /// <summary>
    /// The collection of items for the current page.
    /// </summary>
    public List<T> Items { get; set; } = new();

    /// <summary>
    /// The total count of all items across all pages.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// The current page number (1-based).
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// The number of items per page.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// The total number of pages.
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Indicates whether there are more pages after the current one.
    /// </summary>
    public bool HasNextPage => Page < TotalPages;

    /// <summary>
    /// Indicates whether there are pages before the current one.
    /// </summary>
    public bool HasPreviousPage => Page > 1;
}
