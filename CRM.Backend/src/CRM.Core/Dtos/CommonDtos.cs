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
