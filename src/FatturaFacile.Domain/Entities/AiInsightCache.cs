// <copyright file="AiInsightCache.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// Local cache of the last predictive insights (cash flow, anomalies, recommendations) fetched from the web-api.
/// </summary>
public class AiInsightCache : ITenantScopedEntity
{
    /// <summary>
    /// Gets or sets the row identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the owning local tenant identifier.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the insights as JSON.
    /// </summary>
    public string PayloadJson { get; set; } = "{}";

    /// <summary>
    /// Gets or sets the instant (UTC) the insights were fetched.
    /// </summary>
    public DateTime FetchedAt { get; set; }

    /// <summary>
    /// Gets or sets the creation instant in UTC.
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last update instant in UTC.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
