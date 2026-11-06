// <copyright file="BankTransaction.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// A bank account movement, imported from an ISO 20022 camt.053/camt.054 statement or entered manually, with its reconciliation.
/// </summary>
public class BankTransaction : ITenantScopedEntity
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
    /// Gets or sets the bank account identifier.
    /// </summary>
    public Guid BankAccountId { get; set; }

    /// <summary>
    /// Gets or sets the booking date.
    /// </summary>
    public DateTime BookingDate { get; set; } = DateTime.Today;

    /// <summary>
    /// Gets or sets the value date.
    /// </summary>
    public DateTime? ValueDate { get; set; }

    /// <summary>
    /// Gets or sets the amount (positive = credit, negative = debit).
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the currency.
    /// </summary>
    public string Currency { get; set; } = "CHF";

    /// <summary>
    /// Gets or sets the counterparty name.
    /// </summary>
    public string? Counterparty { get; set; }

    /// <summary>
    /// Gets or sets the counterparty IBAN.
    /// </summary>
    public string? CounterpartyIban { get; set; }

    /// <summary>
    /// Gets or sets the structured reference (QRR, SCOR) or end-to-end identifier.
    /// </summary>
    public string? Reference { get; set; }

    /// <summary>
    /// Gets or sets the remittance information.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the bank entry reference, used to avoid duplicate imports.
    /// </summary>
    public string? ExternalId { get; set; }

    /// <summary>
    /// Gets or sets the status: Unmatched, Matched or Ignored.
    /// </summary>
    public string Status { get; set; } = "Unmatched";

    /// <summary>
    /// Gets or sets the matched document type: Invoice or PurchaseInvoice.
    /// </summary>
    public string? MatchedDocumentType { get; set; }

    /// <summary>
    /// Gets or sets the matched document identifier.
    /// </summary>
    public Guid? MatchedDocumentId { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last modification timestamp.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
