// <copyright file="FixedAsset.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// A fixed asset (cespite / immobilizzazione) with its depreciation plan.
/// </summary>
public class FixedAsset : ITenantScopedEntity
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
    /// Gets or sets the asset code.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the category (e.g. vehicles, machinery, IT equipment).
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the acquisition date.
    /// </summary>
    public DateTime AcquisitionDate { get; set; } = DateTime.Today;

    /// <summary>
    /// Gets or sets the date the asset was put into service.
    /// </summary>
    public DateTime? InServiceDate { get; set; }

    /// <summary>
    /// Gets or sets the acquisition cost.
    /// </summary>
    public decimal AcquisitionCost { get; set; }

    /// <summary>
    /// Gets or sets the residual value at the end of the useful life.
    /// </summary>
    public decimal ResidualValue { get; set; }

    /// <summary>
    /// Gets or sets the yearly depreciation rate in percent.
    /// </summary>
    public decimal RatePercent { get; set; }

    /// <summary>
    /// Gets or sets the method: StraightLine (on cost) or Declining (on book value, allowed in Switzerland at double rate).
    /// </summary>
    public string Method { get; set; } = "StraightLine";

    /// <summary>
    /// Gets or sets a value indicating whether the first year uses half the rate (Italian practice).
    /// </summary>
    public bool HalfRateFirstYear { get; set; }

    /// <summary>
    /// Gets or sets the asset account.
    /// </summary>
    public Guid? AssetAccountId { get; set; }

    /// <summary>
    /// Gets or sets the depreciation expense account.
    /// </summary>
    public Guid? DepreciationAccountId { get; set; }

    /// <summary>
    /// Gets or sets the accumulated depreciation account.
    /// </summary>
    public Guid? AccumulatedAccountId { get; set; }

    /// <summary>
    /// Gets or sets the disposal date.
    /// </summary>
    public DateTime? DisposalDate { get; set; }

    /// <summary>
    /// Gets or sets the disposal proceeds.
    /// </summary>
    public decimal? DisposalAmount { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last modification timestamp.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
