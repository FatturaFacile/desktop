// <copyright file="VatRegisterEntry.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// One line of a VAT register (Mod 029): a document, or one rate of a document, with its progressive protocol number inside the register, section and year. A definitively printed line is locked.
/// </summary>
public class VatRegisterEntry : ITenantScopedEntity
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
    /// Gets or sets the register.
    /// </summary>
    public VatRegisterKind Kind { get; set; }

    /// <summary>
    /// Gets or sets the sezionale.
    /// </summary>
    public VatRegisterSection Section { get; set; }

    /// <summary>
    /// Gets or sets the register year.
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// Gets or sets the progressive protocol number inside kind, section and year; the rate lines of one document share it.
    /// </summary>
    public int ProtocolNumber { get; set; }

    /// <summary>
    /// Gets or sets the date of registration.
    /// </summary>
    public DateTime RegistrationDate { get; set; } = DateTime.Today;

    /// <summary>
    /// Gets or sets the date of the document.
    /// </summary>
    public DateTime DocumentDate { get; set; } = DateTime.Today;

    /// <summary>
    /// Gets or sets the number of the document.
    /// </summary>
    public string DocumentNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the customer or supplier.
    /// </summary>
    public string? PartnerName { get; set; }

    /// <summary>
    /// Gets or sets the VAT number or tax code of the customer or supplier.
    /// </summary>
    public string? PartnerVatNumber { get; set; }

    /// <summary>
    /// Gets or sets the description (goods held for third parties, daily takings).
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the taxable amount (the value for goods held for third parties).
    /// </summary>
    public decimal TaxableAmount { get; set; }

    /// <summary>
    /// Gets or sets the VAT rate in percent.
    /// </summary>
    public decimal VatRate { get; set; }

    /// <summary>
    /// Gets or sets the VAT amount.
    /// </summary>
    public decimal VatAmount { get; set; }

    /// <summary>
    /// Gets or sets the deductible VAT of a purchase, after the pro-rata.
    /// </summary>
    public decimal DeductibleVat { get; set; }

    /// <summary>
    /// Gets or sets the non-deductible VAT of a purchase.
    /// </summary>
    public decimal NonDeductibleVat { get; set; }

    /// <summary>
    /// Gets or sets the FatturaPA nature code of a zero-rate line (N1, N2.1, N3.5, N4, ...).
    /// </summary>
    public string? VatNature { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the line is under reverse charge.
    /// </summary>
    public bool IsReverseCharge { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the line is under split payment.
    /// </summary>
    public bool IsSplitPayment { get; set; }

    /// <summary>
    /// Gets or sets the kind of source: Invoice, PurchaseInvoice or Manual.
    /// </summary>
    public string SourceType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the identifier of the source document.
    /// </summary>
    public Guid? SourceId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the line was printed definitively and is locked.
    /// </summary>
    public bool IsFinalized { get; set; }

    /// <summary>
    /// Gets or sets the definitive print that locked the line.
    /// </summary>
    public Guid? PrintRunId { get; set; }

    /// <summary>
    /// Gets or sets the protocol number the web-api assigned to the same document, once synced.
    /// </summary>
    public int? ServerProtocolNumber { get; set; }

    /// <summary>
    /// Gets or sets the creation instant in UTC.
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last update instant in UTC.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
