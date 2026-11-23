// <copyright file="SwissVatConfig.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// Tenant-level Swiss VAT (MWST / IVA / TVA) configuration.
/// </summary>
public class SwissVatConfig : ITenantScopedEntity
{
    /// <summary>
    /// Gets or sets the primary key identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the owning tenant identifier.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the Swiss UID-MWST number (e.g., CHE-116.281.710 MWST).
    /// </summary>
    public string UidMwst { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the accounting method (Effective or Saldosteuersatz).
    /// </summary>
    public SwissVatMethod AccountingMethod { get; set; } = SwissVatMethod.Effective;

    /// <summary>
    /// Gets or sets the consideration basis (Agreed or Received).
    /// </summary>
    public SwissVatConsiderationBasis ConsiderationBasis { get; set; } = SwissVatConsiderationBasis.Agreed;

    /// <summary>
    /// Gets or sets the reporting frequency.
    /// </summary>
    public SwissVatFrequency ReportingFrequency { get; set; } = SwissVatFrequency.Quarterly;

    /// <summary>
    /// Gets or sets the primary Saldosteuersatz rate percentage when applicable.
    /// </summary>
    public decimal? SssRatePrimary { get; set; }

    /// <summary>
    /// Gets or sets the secondary Saldosteuersatz rate percentage when applicable.
    /// </summary>
    public decimal? SssRateSecondary { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether automatic Bezugsteuer detection is enabled.
    /// </summary>
    public bool AutoDetectBezugsteuer { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether commercial 0.05 CHF rounding is applied.
    /// </summary>
    public bool UseCommercial005Rounding { get; set; } = true;

    /// <summary>
    /// Gets or sets the UTC creation timestamp.
    /// </summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the UTC last-updated timestamp.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
