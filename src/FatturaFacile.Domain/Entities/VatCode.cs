// <copyright file="VatCode.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// Represents a Value Added Tax (VAT / IVA / MWST) rate definition.
/// Supports global shared rates (TenantId == null) and custom tenant rates.
/// </summary>
public class VatCode : IOptionalTenantEntity
{

    /// <summary>
    /// Gets or sets the primary key identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the optional owning tenant identifier. If null, this rate is global.
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// Gets or sets the unique alphanumeric tax code (e.g. "IT_22", "CH_81", "IT_N1").
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the human-readable description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the country ISO code ("IT", "CH").
    /// </summary>
    public string CountryIso { get; set; } = "IT";

    /// <summary>
    /// Gets or sets the numerical percentage rate (e.g. 22.0, 8.1, 2.6, 0.0).
    /// </summary>
    public decimal Rate { get; set; }

    /// <summary>
    /// Gets or sets the tax rate percentage field for legacy compatibility.
    /// </summary>
    public decimal? TaxRatePercent { get; set; }

    /// <summary>
    /// Gets or sets the Italian FatturaPA exemption nature code (e.g., N1, N2.1, N3.1, N4).
    /// </summary>
    public string? Nature { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether split payment applies.
    /// </summary>
    public bool IsSplitPayment { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether reverse charge mechanism applies.
    /// </summary>
    public bool IsReverseCharge { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this VAT rate is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the update timestamp.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the soft delete timestamp.
    /// </summary>
    public DateTime? DeletedAt { get; set; }
}
