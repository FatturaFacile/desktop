// <copyright file="WithholdingCertificate.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// A Certificazione Unica of one percipient (professional or agent) for one year and causale, built from the withholdings of the year.
/// </summary>
public class WithholdingCertificate : ITenantScopedEntity
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
    /// Gets or sets the year of the payments.
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// Gets or sets the percipient contact.
    /// </summary>
    public Guid SupplierId { get; set; }

    /// <summary>
    /// Gets or sets the name of the percipient.
    /// </summary>
    public string RecipientName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tax code of the percipient.
    /// </summary>
    public string RecipientTaxCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the VAT number of the percipient.
    /// </summary>
    public string? RecipientVatNumber { get; set; }

    /// <summary>
    /// Gets or sets the causale (A, M, L, ...).
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the gross amount paid in the year.
    /// </summary>
    public decimal GrossPaid { get; set; }

    /// <summary>
    /// Gets or sets the taxable amount.
    /// </summary>
    public decimal TaxableAmount { get; set; }

    /// <summary>
    /// Gets or sets the amount withheld in the year.
    /// </summary>
    public decimal WithheldAmount { get; set; }

    /// <summary>
    /// Gets or sets the part of the withholding already paid with the Modello F24.
    /// </summary>
    public decimal WithheldPaidAmount { get; set; }

    /// <summary>
    /// Gets or sets the state.
    /// </summary>
    public WithholdingCertificateStatus Status { get; set; } = WithholdingCertificateStatus.Draft;

    /// <summary>
    /// Gets or sets when the PDF or the telematic file was produced.
    /// </summary>
    public DateTime? IssuedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the creation instant in UTC.
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last update instant in UTC.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
