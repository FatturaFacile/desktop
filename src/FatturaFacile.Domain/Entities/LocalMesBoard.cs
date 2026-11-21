// <copyright file="LocalMesBoard.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// The last MES operator board pulled from the web-api for one work centre, kept so the board opens offline. The payload is the
/// `data` object of the API answer, stored as received. A row with the empty work centre key holds the list of work centres.
/// </summary>
public class LocalMesBoard : ITenantScopedEntity
{
    /// <summary>
    /// The work centre key of the row that caches the list of work centres.
    /// </summary>
    public static readonly Guid WorkCentreList = Guid.Empty;

    /// <summary>
    /// Gets or sets the row identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the owning tenant identifier.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the work centre identifier of the server.
    /// </summary>
    public Guid WorkCenterId { get; set; }

    /// <summary>
    /// Gets or sets the payload.
    /// </summary>
    public string BoardJson { get; set; } = "{}";

    /// <summary>
    /// Gets or sets the instant the payload was pulled (UTC).
    /// </summary>
    public DateTime SyncedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the creation instant.
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last change instant.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
