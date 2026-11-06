// <copyright file="FixedAssetDepreciation.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// A posted yearly depreciation of a fixed asset.
/// </summary>
public class FixedAssetDepreciation : ITenantScopedEntity
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
    /// Gets or sets the fixed asset identifier.
    /// </summary>
    public Guid FixedAssetId { get; set; }

    /// <summary>
    /// Gets or sets the fiscal year.
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// Gets or sets the depreciation amount.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the book value after the depreciation.
    /// </summary>
    public decimal BookValueAfter { get; set; }

    /// <summary>
    /// Gets or sets the general ledger entry.
    /// </summary>
    public Guid? JournalEntryId { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last modification timestamp.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
