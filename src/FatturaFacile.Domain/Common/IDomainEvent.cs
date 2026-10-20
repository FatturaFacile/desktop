namespace FatturaFacile.Domain.Common;

/// <summary>
/// Marker interface for all domain events.
/// </summary>
public interface IDomainEvent
{
    public Guid EventId { get; }
    public DateTime OccurredOnUtc { get; }
}