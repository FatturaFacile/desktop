// <copyright file="LocalAnalyticalOperation.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// A manual analytical entry captured offline (Mod 018) and waiting to be sent to the web-api. The management ledger is separate
/// from the statutory ledger, so an operation never touches the general ledger.
/// </summary>
public class LocalAnalyticalOperation : ITenantScopedEntity
{
    /// <summary>
    /// The operation has not been sent yet.
    /// </summary>
    public const string Pending = "Pending";

    /// <summary>
    /// The web-api accepted the operation.
    /// </summary>
    public const string Sent = "Sent";

    /// <summary>
    /// The web-api refused the operation; it stays for the user to review.
    /// </summary>
    public const string Failed = "Failed";

    /// <summary>
    /// Gets or sets the operation identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the owning tenant identifier.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the JSON body of the `POST /analytical/entries` request.
    /// </summary>
    public string PayloadJson { get; set; } = "{}";

    /// <summary>
    /// Gets or sets the state of the operation: <see cref="Pending"/>, <see cref="Sent"/> or <see cref="Failed"/>.
    /// </summary>
    public string Status { get; set; } = Pending;

    /// <summary>
    /// Gets or sets how many times a send was attempted.
    /// </summary>
    public int Attempts { get; set; }

    /// <summary>
    /// Gets or sets the reason of the last refusal.
    /// </summary>
    public string? LastError { get; set; }

    /// <summary>
    /// Gets or sets the creation instant in UTC.
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last update instant in UTC.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
