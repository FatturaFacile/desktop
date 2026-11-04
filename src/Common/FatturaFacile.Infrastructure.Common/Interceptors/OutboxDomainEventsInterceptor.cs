using System.Text.Json;
using Microsoft.EntityFrameworkCore.Diagnostics;
using FatturaFacile.Domain.Shared.Primitives;
using FatturaFacile.Infrastructure.Common.Outbox;

namespace FatturaFacile.Infrastructure.Common.Interceptors;

public sealed class OutboxDomainEventsInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        ConvertDomainEventsToOutboxMessages(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        ConvertDomainEventsToOutboxMessages(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void ConvertDomainEventsToOutboxMessages(Microsoft.EntityFrameworkCore.DbContext? context)
    {
        if (context == null) return;

        var aggregateRoots = context.ChangeTracker
            .Entries()
            .Select(e => e.Entity)
            .Where(e => e.GetType().BaseType is { IsGenericType: true } bt && 
                        bt.GetGenericTypeDefinition() == typeof(AggregateRoot<>))
            .ToList();

        var outboxMessages = new List<OutboxMessage>();

        foreach (var aggregate in aggregateRoots)
        {
            var clearMethod = aggregate.GetType().GetMethod("ClearDomainEvents");
            var prop = aggregate.GetType().GetProperty("DomainEvents");
            if (prop?.GetValue(aggregate) is IEnumerable<dynamic> events)
            {
                foreach (var domainEvent in events)
                {
                    outboxMessages.Add(new OutboxMessage
                    {
                        Id = Guid.NewGuid(),
                        OccurredOnUtc = domainEvent.OccurredOn,
                        Type = domainEvent.GetType().AssemblyQualifiedName ?? domainEvent.GetType().Name,
                        Content = JsonSerializer.Serialize((object)domainEvent)
                    });
                }
            }
            clearMethod?.Invoke(aggregate, null);
        }

        if (outboxMessages.Count > 0)
        {
            context.Set<OutboxMessage>().AddRange(outboxMessages);
        }
    }
}
