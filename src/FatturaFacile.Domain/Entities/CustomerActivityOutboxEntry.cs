// <copyright file="CustomerActivityOutboxEntry.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// A CRM activity (visit or follow-up) created from the customer 360 view while the server was unreachable. Replayed in order
/// when connectivity returns and removed once the server accepted it.
/// </summary>
public class CustomerActivityOutboxEntry : ITenantScopedEntity
{
    /// <summary>
    /// Gets or sets the entry identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <inheritdoc/>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the server identifier of the CRM account the activity relates to.
    /// </summary>
    public Guid AccountId { get; set; }

    /// <summary>
    /// Gets or sets the activity type (meeting or follow_up).
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the subject.
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional due date.
    /// </summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// Gets or sets the instant the user acted.
    /// </summary>
    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the delivery state.
    /// </summary>
    public CustomerActivityOutboxStatus Status { get; set; } = CustomerActivityOutboxStatus.Pending;

    /// <summary>
    /// Gets or sets the number of failed attempts.
    /// </summary>
    public int Attempts { get; set; }

    /// <summary>
    /// Gets or sets the earliest instant of the next attempt.
    /// </summary>
    public DateTime NextAttemptAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last error.
    /// </summary>
    public string? LastError { get; set; }

    /// <inheritdoc/>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <inheritdoc/>
    public DateTime? UpdatedAt { get; set; }
}
