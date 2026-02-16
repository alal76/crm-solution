using System;
using System.ComponentModel.DataAnnotations;

namespace CRM.Core.Dtos;

/// <summary>
/// Base interface for all DTO base classes to enable polymorphic validation and handling.
/// </summary>
public interface IDto
{
    /// <summary>
    /// Gets the DTO version for tracking API contract changes.
    /// </summary>
    string DtoVersion { get; }
}

/// <summary>
/// Base class for all read/response DTOs returned from API endpoints.
/// Includes audit metadata (Id, CreatedAt, UpdatedAt, etc.).
/// </summary>
public abstract class ReadResponseDtoBase : IDto
{
    /// <summary>
    /// Gets or sets the unique identifier for this entity.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the date and time when this entity was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the date and time when this entity was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who last updated this entity.
    /// </summary>
    public int? UpdatedBy { get; set; }

    /// <summary>
    /// Gets or sets the row version for optimistic concurrency control.
    /// </summary>
    public byte[]? RowVersion { get; set; }

    /// <summary>
    /// Gets the DTO version for tracking API contract changes.
    /// </summary>
    public virtual string DtoVersion => "1.0";
}

/// <summary>
/// Base marker class for all create request DTOs (POST /api/entity).
/// Create DTOs typically have all required fields and no audit metadata.
/// </summary>
public abstract class CreateRequestDtoBase : IDto
{
    /// <summary>
    /// Gets the DTO version for tracking API contract changes.
    /// </summary>
    public virtual string DtoVersion => "1.0";
}

/// <summary>
/// Base marker class for all update request DTOs (PATCH /api/entity/{id}).
/// Update DTOs typically have all optional fields (nullable) for partial updates.
/// </summary>
public abstract class UpdateRequestDtoBase : IDto
{
    /// <summary>
    /// Gets or sets the row version for optimistic concurrency control during updates.
    /// </summary>
    public byte[]? RowVersion { get; set; }

    /// <summary>
    /// Gets the DTO version for tracking API contract changes.
    /// </summary>
    public virtual string DtoVersion => "1.0";
}

/// <summary>
/// Base class for list/summary DTOs returned in paginated responses.
/// Contains minimal fields needed for list display (Id, name, status, etc.).
/// </summary>
public abstract class ListResponseDtoBase : IDto
{
    /// <summary>
    /// Gets or sets the unique identifier for this entity.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the display name or summary of this entity.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the date when this entity was created (for sorting/filtering).
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets the DTO version for tracking API contract changes.
    /// </summary>
    public virtual string DtoVersion => "1.0";
}

/// <summary>
/// Base class for representing linked/related entities with temporal validity.
/// Used when entities have time-bounded relationships (e.g., contact duration at account).
/// </summary>
public abstract class LinkedEntityDtoBase : IDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the link.
    /// </summary>
    public int LinkId { get; set; }

    /// <summary>
    /// Gets or sets the date when this relationship became valid.
    /// </summary>
    public DateTime? ValidFrom { get; set; }

    /// <summary>
    /// Gets or sets the date when this relationship expires or ended.
    /// </summary>
    public DateTime? ValidTo { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this link is currently active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets the DTO version for tracking API contract changes.
    /// </summary>
    public virtual string DtoVersion => "1.0";
}

/// <summary>
/// Base class for paginated list responses containing items and pagination metadata.
/// Generic type parameter T represents the item type in the list.
/// </summary>
/// <typeparam name="T">The type of items in the paginated response.</typeparam>
public class PaginatedResponseDtoBase<T> : IDto
{
    /// <summary>
    /// Gets or sets the collection of items for the current page.
    /// </summary>
    public List<T> Items { get; set; } = new();

    /// <summary>
    /// Gets or sets the total count of items across all pages.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Gets or sets the current page number (1-based).
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Gets or sets the number of items per page.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Gets or sets the total number of pages available.
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    /// <summary>
    /// Gets a value indicating whether there are more items on the next page.
    /// </summary>
    public bool HasNextPage => Page < TotalPages;

    /// <summary>
    /// Gets a value indicating whether there are items on the previous page.
    /// </summary>
    public bool HasPreviousPage => Page > 1;

    /// <summary>
    /// Gets the DTO version for tracking API contract changes.
    /// </summary>
    public virtual string DtoVersion => "1.0";
}

/// <summary>
/// Generic wrapper for paginated list responses with type parameter for item type.
/// </summary>
/// <typeparam name="T">The type of items in the paginated list.</typeparam>
public class PaginatedList<T>
{
    /// <summary>
    /// Gets or sets the collection of items for the current page.
    /// </summary>
    public required List<T> Items { get; set; }

    /// <summary>
    /// Gets or sets the total count of items across all pages.
    /// </summary>
    public required int TotalCount { get; set; }

    /// <summary>
    /// Gets or sets the current page number (1-based).
    /// </summary>
    public required int Page { get; set; }

    /// <summary>
    /// Gets or sets the number of items per page.
    /// </summary>
    public required int PageSize { get; set; }

    /// <summary>
    /// Gets the total number of pages available.
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    /// <summary>
    /// Gets a value indicating whether there are more items on the next page.
    /// </summary>
    public bool HasNextPage => Page < TotalPages;

    /// <summary>
    /// Gets a value indicating whether there are items on the previous page.
    /// </summary>
    public bool HasPreviousPage => Page > 1;
}

/// <summary>
/// Marker interface for bulk operation DTOs (batch create/update/delete).
/// </summary>
public interface IBulkOperationDto
{
    /// <summary>
    /// Gets the collection of items to process in bulk.
    /// </summary>
    IEnumerable<object> Items { get; }
}

/// <summary>
/// Base class for bulk operation DTOs (batch create/update/delete).
/// </summary>
/// <typeparam name="T">The type of items in the bulk operation.</typeparam>
public abstract class BulkOperationDtoBase<T> : IDto, IBulkOperationDto
    where T : class
{
    /// <summary>
    /// Gets or sets the collection of items to process in bulk.
    /// </summary>
    [Required(ErrorMessage = "Items collection is required for bulk operations.")]
    [MinLength(1, ErrorMessage = "At least one item is required for bulk operations.")]
    public List<T> Items { get; set; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether to continue processing on error.
    /// If false, the operation stops on the first error.
    /// </summary>
    public bool ContinueOnError { get; set; } = false;

    /// <summary>
    /// Gets the items collection for the IBulkOperationDto interface.
    /// </summary>
    IEnumerable<object> IBulkOperationDto.Items => Items.Cast<object>();

    /// <summary>
    /// Gets the DTO version for tracking API contract changes.
    /// </summary>
    public virtual string DtoVersion => "1.0";
}

/// <summary>
/// Base class for search/filter request DTOs to standardize search operations.
/// </summary>
public abstract class SearchRequestDtoBase : IDto
{
    /// <summary>
    /// Gets or sets the search query string (searched across multiple fields).
    /// </summary>
    [StringLength(500, ErrorMessage = "Search query cannot exceed 500 characters.")]
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Gets or sets the current page number for paginated results (1-based).
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Page must be at least 1.")]
    public int Page { get; set; } = 1;

    /// <summary>
    /// Gets or sets the number of items per page.
    /// </summary>
    [Range(1, 200, ErrorMessage = "PageSize must be between 1 and 200.")]
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Gets or sets the field name to sort by.
    /// </summary>
    [StringLength(50, ErrorMessage = "Sort field name cannot exceed 50 characters.")]
    public string? SortBy { get; set; }

    /// <summary>
    /// Gets or sets whether to sort in descending order (default is ascending).
    /// </summary>
    public bool SortDescending { get; set; } = false;

    /// <summary>
    /// Gets the DTO version for tracking API contract changes.
    /// </summary>
    public virtual string DtoVersion => "1.0";
}

/// <summary>
/// Base class for import/export operation DTOs.
/// </summary>
public abstract class ImportExportDtoBase : IDto
{
    /// <summary>
    /// Gets or sets the format of the import/export (CSV, JSON, Excel, etc.).
    /// </summary>
    [Required(ErrorMessage = "Format is required.")]
    [StringLength(20)]
    public string Format { get; set; } = "";

    /// <summary>
    /// Gets or sets the base64-encoded file content or file path.
    /// </summary>
    [Required(ErrorMessage = "File content is required.")]
    public string FileContent { get; set; } = "";

    /// <summary>
    /// Gets or sets the original file name.
    /// </summary>
    [StringLength(255)]
    public string? FileName { get; set; }

    /// <summary>
    /// Gets the DTO version for tracking API contract changes.
    /// </summary>
    public virtual string DtoVersion => "1.0";
}
