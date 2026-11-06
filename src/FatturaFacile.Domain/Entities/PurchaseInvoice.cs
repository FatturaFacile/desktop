// <copyright file="PurchaseInvoice.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// A supplier invoice (ciclo passivo): registration, VAT deduction, posting to the general ledger and payment.
/// </summary>
public class PurchaseInvoice : ITenantScopedEntity
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
    /// Gets or sets the supplier contact identifier.
    /// </summary>
    public Guid SupplierId { get; set; }

    /// <summary>
    /// Gets or sets the supplier document number.
    /// </summary>
    public string DocumentNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the internal registration (protocol) number.
    /// </summary>
    public string? ProtocolNumber { get; set; }

    /// <summary>
    /// Gets or sets the document date.
    /// </summary>
    public DateTime DocumentDate { get; set; } = DateTime.Today;

    /// <summary>
    /// Gets or sets the registration date (VAT period of the deduction).
    /// </summary>
    public DateTime RegistrationDate { get; set; } = DateTime.Today;

    /// <summary>
    /// Gets or sets the due date.
    /// </summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// Gets or sets the currency.
    /// </summary>
    public string Currency { get; set; } = "CHF";

    /// <summary>
    /// Gets or sets the net total.
    /// </summary>
    public decimal TotalNet { get; set; }

    /// <summary>
    /// Gets or sets the VAT total.
    /// </summary>
    public decimal TotalVat { get; set; }

    /// <summary>
    /// Gets or sets the deductible input VAT.
    /// </summary>
    public decimal DeductibleVat { get; set; }

    /// <summary>
    /// Gets or sets the gross total.
    /// </summary>
    public decimal TotalGross { get; set; }

    /// <summary>
    /// Gets or sets the amount already paid.
    /// </summary>
    public decimal PaidAmount { get; set; }

    /// <summary>
    /// Gets or sets the status: Draft, Posted or Paid.
    /// </summary>
    public string Status { get; set; } = "Draft";

    /// <summary>
    /// Gets or sets the supplier IBAN or QR-IBAN for the payment.
    /// </summary>
    public string? PaymentIban { get; set; }

    /// <summary>
    /// Gets or sets the payment reference (QRR, SCOR or free text).
    /// </summary>
    public string? PaymentReference { get; set; }

    /// <summary>
    /// Gets or sets the general ledger entry created at posting.
    /// </summary>
    public Guid? JournalEntryId { get; set; }

    /// <summary>
    /// Gets or sets notes.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last modification timestamp.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
