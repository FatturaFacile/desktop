// <copyright file="LocalDssSnapshot.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// The last Executive DSS read models pulled from the web-api for one filter, kept so the cockpit works offline. The payloads
/// are the `data` objects of the API answers, stored as received.
/// </summary>
public class LocalDssSnapshot : ITenantScopedEntity
{
    /// <summary>
    /// The legal entity key of a snapshot that spans every legal entity.
    /// </summary>
    public const string AllLegalEntities = "";

    /// <summary>
    /// Gets or sets the snapshot identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the owning tenant identifier.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the legal entity identifier of the server, or <see cref="AllLegalEntities"/>.
    /// </summary>
    public string LegalEntityId { get; set; } = AllLegalEntities;

    /// <summary>
    /// Gets or sets the reporting currency (CHF or EUR).
    /// </summary>
    public string Currency { get; set; } = "CHF";

    /// <summary>
    /// Gets or sets the period preset (MTD, QTD, YTD or L12M).
    /// </summary>
    public string PeriodPreset { get; set; } = "YTD";

    /// <summary>
    /// Gets or sets the comparison baseline (PRIOR_PERIOD, PRIOR_YEAR or BUDGET).
    /// </summary>
    public string ComparisonBaseline { get; set; } = "PRIOR_YEAR";

    /// <summary>
    /// Gets or sets the overview payload (balanced scorecard, alerts, working capital).
    /// </summary>
    public string OverviewJson { get; set; } = "{}";

    /// <summary>
    /// Gets or sets the profitability matrix payload.
    /// </summary>
    public string ProfitabilityJson { get; set; } = "{}";

    /// <summary>
    /// Gets or sets the variance bridge payload.
    /// </summary>
    public string VarianceBridgeJson { get; set; } = "{}";

    /// <summary>
    /// Gets or sets the what-if inputs payload (the actuals the server simulates on), or null when it could not be fetched.
    /// </summary>
    public string? SimulationInputsJson { get; set; }

    /// <summary>
    /// Gets or sets the legal entities payload, or null when it could not be fetched.
    /// </summary>
    public string? LegalEntitiesJson { get; set; }

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
