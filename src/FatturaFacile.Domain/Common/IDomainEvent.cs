// <copyright file="IDomainEvent.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Common;

/// <summary>
/// Defines the contract for domain events representing distinct business facts that occurred in the domain.
/// </summary>
public interface IDomainEvent
{

    /// <summary>
    /// Gets the unique identifier distinguishing this specific domain event occurrence.
    /// </summary>
    public Guid EventId { get; }

    /// <summary>
    /// Gets the coordinated universal time (UTC) timestamp indicating when the domain event occurred.
    /// </summary>
    public DateTime OccurredOnUtc { get; }

}
