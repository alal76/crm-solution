// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Ports.Output.Events;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CRM.Infrastructure.Data.Interceptors;

/// <summary>
/// AP-059: EF Core <see cref="SaveChangesInterceptor"/> that dispatches domain events
/// after every successful <c>SaveChangesAsync</c> call.
/// <para>
/// After the database write completes, the interceptor iterates all tracked entities
/// that implement <see cref="IHasDomainEvents"/>, publishes their pending events via
/// <see cref="IDomainEventPublisher.PublishAndClearAsync"/>, and clears the events
/// from the entity to prevent re-dispatch on subsequent saves.
/// </para>
/// </summary>
public sealed class DomainEventDispatchInterceptor : SaveChangesInterceptor
{
    private readonly IDomainEventPublisher _publisher;

    public DomainEventDispatchInterceptor(IDomainEventPublisher publisher)
    {
        _publisher = publisher;
    }

    /// <inheritdoc/>
    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData data,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (data.Context is not null)
        {
            var eventEntities = data.Context.ChangeTracker
                .Entries<IHasDomainEvents>()
                .ToList();

            foreach (var entry in eventEntities)
            {
                await _publisher.PublishAndClearAsync(entry.Entity, cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        return result;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Synchronous saves are rare in this codebase; domain events are handled by
    /// the async override above. The sync path is a pass-through.
    /// </remarks>
    public override int SavedChanges(SaveChangesCompletedEventData data, int result)
    {
        return base.SavedChanges(data, result);
    }
}
