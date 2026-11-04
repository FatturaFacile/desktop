// <copyright file="OutboxDomainEventsInterceptor.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.Json;
using FatturaFacile.Domain.Shared.Primitives;
using FatturaFacile.Infrastructure.Common.Outbox;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace FatturaFacile.Infrastructure.Common.Interceptors;

/// <summary>
/// EF Core interceptor that captures domain events raised by aggregate roots during SaveChanges
/// and persists them as transactional outbox messages for reliable asynchronous processing.
/// </summary>
public sealed class OutboxDomainEventsInterceptor : SaveChangesInterceptor
{
    /// <inheritdoc />
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
        {
        ArgumentNullException.ThrowIfNull(eventData);
        ConvertDomainEventsToOutboxMessages(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    /// <inheritdoc />
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
        {
        ArgumentNullException.ThrowIfNull(eventData);
        ConvertDomainEventsToOutboxMessages(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void ConvertDomainEventsToOutboxMessages(
        Microsoft.EntityFrameworkCore.DbContext? context)
        {
        if (context == null)
        {
            return;
        }

        var aggregateRoots = context.ChangeTracker.Entries().Select(e => e.Entity).Where(e =>
            e.GetType().BaseType is { IsGenericType: true } bt &&
            bt.GetGenericTypeDefinition() == typeof(AggregateRoot<>)).ToList();

        var outboxMessages = new List<OutboxMessage>();

        foreach (var aggregate in aggregateRoots)
        {
            var clearMethod = aggregate.GetType().GetMethod("ClearDomainEvents");
            var prop = aggregate.GetType().GetProperty("DomainEvents");

            if (prop?.GetValue(aggregate) is IEnumerable<dynamic> events)
            {
                foreach (var domainEvent in events)
                {
                    outboxMessages.Add(
                        new OutboxMessage
                        {
                            Id = Guid.NewGuid(),
                            OccurredOnUtc = domainEvent.OccurredOn,
                            Type = domainEvent.GetType().AssemblyQualifiedName ??
                                   domainEvent.GetType().Name,
                            Content = JsonSerializer.Serialize((object)domainEvent),
                        });
                }
            }

            _ = clearMethod?.Invoke(aggregate, parameters: null);
        }

        if (outboxMessages.Count > 0)
        {
            context.Set<OutboxMessage>().AddRange(outboxMessages);
        }
    }
}
