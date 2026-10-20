namespace FatturaFacile.Domain.Common;

/// <summary>
/// Marker interface for all domain events.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Gets the unique identifier of the domain event.
    /// </summary>
    public Guid EventId { get; }

/// <summary>
    /// Gets the UTC timestamp when the domain event occurred.
    /// </summary>
    public DateTime OccurredOnUtc { get; }
}
