// <copyright file="LocalProjectEvmSnapshot.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// The last profitability and earned value read models of one project pulled from the web-api (Mod 082), kept so the project
/// margin, CPI/SPI, EAC/ETC and WBS breakdown can be read offline. The payloads are the `data` objects of the API answers, stored
/// as received. The project picker is kept in the row whose project is <see cref="ProjectIndexKey"/>.
/// </summary>
public class LocalProjectEvmSnapshot : ITenantScopedEntity
{
    /// <summary>
    /// The project key of the row that holds the project picker (the list of projects) instead of a snapshot.
    /// </summary>
    public const string ProjectIndexKey = "projects";

    /// <summary>
    /// Gets or sets the snapshot identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the owning tenant identifier.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the project identifier of the server, or <see cref="ProjectIndexKey"/>.
    /// </summary>
    public string ProjectId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the WBS breakdown payload (for the project picker row, the list of projects).
    /// </summary>
    public string BreakdownJson { get; set; } = "{}";

    /// <summary>
    /// Gets or sets the profitability waterfall payload.
    /// </summary>
    public string WaterfallJson { get; set; } = "{}";

    /// <summary>
    /// Gets or sets the S-curve payload.
    /// </summary>
    public string ScurveJson { get; set; } = "{}";

    /// <summary>
    /// Gets or sets the instant (UTC) the snapshot was pulled.
    /// </summary>
    public DateTime SyncedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the creation instant in UTC.
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last update instant in UTC.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
