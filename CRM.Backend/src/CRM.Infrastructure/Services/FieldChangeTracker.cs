// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Reflection;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Represents a single field-level change between two entity states.
/// </summary>
public class FieldChange
{
    /// <summary>The name of the property that changed.</summary>
    public string FieldName { get; set; } = string.Empty;

    /// <summary>The previous value serialized as a string.</summary>
    public string? OldValue { get; set; }

    /// <summary>The new value serialized as a string.</summary>
    public string? NewValue { get; set; }

    /// <summary>The CLR type name of the property.</summary>
    public string DataType { get; set; } = string.Empty;
}

/// <summary>
/// Represents a persisted field change record including metadata about who and when.
/// </summary>
public class FieldChangeRecord : FieldChange
{
    /// <summary>The unique identifier of the change record.</summary>
    public int Id { get; set; }

    /// <summary>When the change was recorded.</summary>
    public DateTime ChangedAt { get; set; }

    /// <summary>The user ID who made the change.</summary>
    public int ChangedByUserId { get; set; }

    /// <summary>The display name of the user who made the change.</summary>
    public string? ChangedByUserName { get; set; }
}

/// <summary>
/// Interface for field-level change tracking and audit trail.
/// Compares entity states, records differences, and provides change history.
/// </summary>
public interface IFieldChangeTracker
{
    /// <summary>
    /// Compare two entity states and return field-level changes.
    /// Uses reflection to compare all public properties with getters.
    /// </summary>
    /// <typeparam name="T">The entity type to compare.</typeparam>
    /// <param name="before">The entity state before modification.</param>
    /// <param name="after">The entity state after modification.</param>
    /// <returns>A list of field changes detected between the two states.</returns>
    IReadOnlyList<FieldChange> GetChanges<T>(T before, T after) where T : class;

    /// <summary>
    /// Get the change history for a specific entity.
    /// </summary>
    /// <param name="entityType">The entity type name (e.g., "Account").</param>
    /// <param name="entityId">The entity's primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A list of field change records ordered by most recent first.</returns>
    Task<IReadOnlyList<FieldChangeRecord>> GetFieldHistoryAsync(string entityType, int entityId, CancellationToken ct = default);

    /// <summary>
    /// Record field changes to the audit log (FieldChangeLogs table).
    /// </summary>
    /// <param name="entityType">The entity type name.</param>
    /// <param name="entityId">The entity's primary key.</param>
    /// <param name="userId">The user who made the changes.</param>
    /// <param name="changes">The list of field changes to persist.</param>
    /// <param name="ct">Cancellation token.</param>
    Task RecordChangesAsync(string entityType, int entityId, int userId, IReadOnlyList<FieldChange> changes, CancellationToken ct = default);
}

/// <summary>
/// Implementation of field-level change tracking using reflection-based property comparison
/// and EF Core persistence to the FieldChangeLogs table.
/// </summary>
public class FieldChangeTracker : IFieldChangeTracker
{
    private readonly ICrmDbContext _dbContext;
    private readonly ILogger<FieldChangeTracker> _logger;

    // Properties to skip during comparison (infrastructure/metadata fields)
    private static readonly HashSet<string> SkippedProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        "RowVersion",
        "IsDeleted"
    };

    public FieldChangeTracker(ICrmDbContext dbContext, ILogger<FieldChangeTracker> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public IReadOnlyList<FieldChange> GetChanges<T>(T before, T after) where T : class
    {
        if (before == null) throw new ArgumentNullException(nameof(before));
        if (after == null) throw new ArgumentNullException(nameof(after));

        var changes = new List<FieldChange>();
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && !SkippedProperties.Contains(p.Name));

        foreach (var prop in properties)
        {
            try
            {
                var oldVal = prop.GetValue(before);
                var newVal = prop.GetValue(after);

                // Skip if both are null
                if (oldVal == null && newVal == null)
                    continue;

                // Detect change
                bool changed = oldVal == null || newVal == null || !oldVal.Equals(newVal);

                if (changed)
                {
                    changes.Add(new FieldChange
                    {
                        FieldName = prop.Name,
                        OldValue = SerializeValue(oldVal),
                        NewValue = SerializeValue(newVal),
                        DataType = GetFriendlyTypeName(prop.PropertyType)
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to compare property {PropertyName} on type {TypeName}", prop.Name, typeof(T).Name);
            }
        }

        return changes;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<FieldChangeRecord>> GetFieldHistoryAsync(string entityType, int entityId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(entityType))
            throw new ArgumentException("Entity type is required.", nameof(entityType));

        var logs = await _dbContext.FieldChangeLogs
            .Where(f => f.EntityType == entityType && f.EntityId == entityId && !f.IsDeleted)
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => new FieldChangeRecord
            {
                Id = f.Id,
                FieldName = f.FieldName,
                OldValue = f.OldValue,
                NewValue = f.NewValue,
                DataType = f.DataType,
                ChangedAt = f.CreatedAt,
                ChangedByUserId = f.ChangedByUserId,
                ChangedByUserName = _dbContext.Users
                    .Where(u => u.Id == f.ChangedByUserId)
                    .Select(u => u.FirstName + " " + u.LastName)
                    .FirstOrDefault()
            })
            .ToListAsync(ct);

        return logs;
    }

    /// <inheritdoc />
    public async Task RecordChangesAsync(string entityType, int entityId, int userId, IReadOnlyList<FieldChange> changes, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(entityType))
            throw new ArgumentException("Entity type is required.", nameof(entityType));
        if (changes == null || changes.Count == 0)
            return;

        var now = DateTime.UtcNow;
        var entities = changes.Select(c => new FieldChangeLog
        {
            EntityType = entityType,
            EntityId = entityId,
            FieldName = c.FieldName,
            OldValue = c.OldValue,
            NewValue = c.NewValue,
            DataType = c.DataType,
            ChangedByUserId = userId,
            CreatedAt = now
        });

        _dbContext.FieldChangeLogs.AddRange(entities);
        await _dbContext.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Recorded {ChangeCount} field changes for {EntityType}#{EntityId} by user {UserId}",
            changes.Count, entityType, entityId, userId);
    }

    /// <summary>
    /// Serialize a property value to a string representation for storage.
    /// </summary>
    private static string? SerializeValue(object? value)
    {
        if (value == null)
            return null;

        return value switch
        {
            DateTime dt => dt.ToString("O"), // ISO 8601
            DateTimeOffset dto => dto.ToString("O"),
            decimal d => d.ToString("G"),
            double dbl => dbl.ToString("G"),
            float f => f.ToString("G"),
            bool b => b.ToString().ToLowerInvariant(),
            byte[] bytes => Convert.ToBase64String(bytes),
            _ => value.ToString()
        };
    }

    /// <summary>
    /// Get a user-friendly type name for display purposes.
    /// </summary>
    private static string GetFriendlyTypeName(Type type)
    {
        var underlying = Nullable.GetUnderlyingType(type);
        var effectiveType = underlying ?? type;

        return effectiveType.Name switch
        {
            "String" => "String",
            "Int32" => "Int32",
            "Int64" => "Int64",
            "Decimal" => "Decimal",
            "Double" => "Double",
            "Single" => "Float",
            "Boolean" => "Boolean",
            "DateTime" => "DateTime",
            "DateTimeOffset" => "DateTimeOffset",
            "Guid" => "Guid",
            _ => effectiveType.Name
        };
    }
}
