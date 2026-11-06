// <copyright file="CollectiveAgreementLevel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// A level of an Italian collective agreement with its minimum monthly pay.
/// </summary>
public class CollectiveAgreementLevel : ITenantScopedEntity
{
    /// <summary>
    /// Gets or sets the primary key identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the company (tenant) identifier.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the collective agreement.
    /// </summary>
    public Guid AgreementId { get; set; }

    /// <summary>
    /// Gets or sets the level or category (for example 4, Q, 1S).
    /// </summary>
    public string Level { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the level.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the minimum monthly pay for full time (minimo tabellare plus contingenza and EDR).
    /// </summary>
    public decimal MonthlyMinimum { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the record was created.
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the date and time when the record was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
