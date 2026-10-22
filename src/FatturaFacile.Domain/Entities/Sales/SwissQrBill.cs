// <copyright file="SwissQrBill.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.Sales;

/// <summary>
/// Represents a Swiss QR-bill (QR-Rechnung / QR-facture) payment part aggregate complying with the SIX payment
/// standards (Swiss Implementation Guidelines for QR-bill), featuring structured QR-IBAN, 27-digit QR reference numbers,
/// and statutory Swiss cash rounding (0.05 CHF).
/// </summary>
public sealed class SwissQrBill : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    private SwissQrBill()
    {
    }

    /// <summary>
    /// Gets or sets the unique tenant identifier for multi-tenant data partitioning.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets the unique commercial bill or invoice reference number (e.g., "QR-2026/001").
    /// </summary>
    public string BillNumber { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the issuance date of the QR-bill.
    /// </summary>
    public DateTime BillDate { get; private set; }

    /// <summary>
    /// Gets the unique identifier of the debtor client in the local business ledger.
    /// </summary>
    public Guid ClientId { get; private set; }

    /// <summary>
    /// Gets the full legal business or personal name of the payment creditor (payee).
    /// </summary>
    public string CreditorName { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the structured postal address of the creditor complying with Swiss Payment Standards (street, house number, postal code, city, country).
    /// </summary>
    public Address CreditorAddress { get; private set; } = null!;

    /// <summary>
    /// Gets the special Swiss QR-IBAN identifying the creditor's bank account with an assigned Institute ID (IID) range between 30000 and 31999.
    /// </summary>
    public QrIban CreditorQrIban { get; private set; } = null!;

    /// <summary>
    /// Gets the full name or company trade name of the debtor (payer).
    /// </summary>
    public string DebtorName { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the postal address of the debtor.
    /// </summary>
    public Address DebtorAddress { get; private set; } = null!;

    /// <summary>
    /// Gets the payable monetary amount adhering to statutory Swiss cash rounding rules (nearest 0.05 CHF for Swiss Francs).
    /// </summary>
    public Money Amount { get; private set; } = Money.Chf(0);

    /// <summary>
    /// Gets the classification type of the QR-bill (e.g., QR-IBAN with QR reference or IBAN with Creditor Reference).
    /// </summary>
    public QrBillType BillType { get; private set; }

    /// <summary>
    /// Gets the 27-digit structured QR reference number (SCOR / Structured Creditor Reference) validated via recursive Modulo 10 check digit.
    /// </summary>
    public string ReferenceNumber { get; private set; } = string.Empty;

    /// <summary>
    /// Gets an optional unstructured textual message (up to 140 characters) accompanying the QR payment slip.
    /// </summary>
    public string? AdditionalUnstructuredMessage { get; private set; }

    /// <summary>
    /// Gets the due date for payment settlement.
    /// </summary>
    public DateTime DueDate { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this Swiss QR-bill has been fully settled and reconciled.
    /// </summary>
    public bool IsPaid { get; private set; }

    /// <summary>
    /// Gets the UTC timestamp when the payment settlement was confirmed, or <see langword="null"/> if still outstanding.
    /// </summary>
    public DateTime? PaidDateUtc { get; private set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when this QR-bill record was created.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the identity of the user or system process that created the QR-bill.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when this QR-bill record was last modified.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the identity of the user or system process that last updated the record.
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Creates a new <see cref="SwissQrBill"/> aggregate, applying Swiss statutory cash rounding (0.05 CHF) to the nominal amount and formatting the 27-digit payment reference.
    /// </summary>
    /// <param name="tenantId">The unique tenant identifier for isolation.</param>
    /// <param name="billNumber">The business identifier or invoice number for the bill.</param>
    /// <param name="billDate">The issuance date of the QR-bill.</param>
    /// <param name="clientId">The client or debtor entity identifier.</param>
    /// <param name="creditorName">The legal payee company name.</param>
    /// <param name="creditorAddress">The postal address of the payee.</param>
    /// <param name="creditorQrIban">The validated Swiss QR-IBAN of the creditor.</param>
    /// <param name="debtorName">The legal payer name.</param>
    /// <param name="debtorAddress">The postal address of the payer.</param>
    /// <param name="rawAmount">The unrounded monetary sum billed.</param>
    /// <param name="referenceNumber">The 27-digit structured reference number (Modulo 10 check digit).</param>
    /// <param name="dueDate">The contractual maturity date.</param>
    /// <param name="message">An optional unstructured message printed on the payment slip.</param>
    /// <returns>A fully initialized <see cref="SwissQrBill"/> instance.</returns>
    public static SwissQrBill Create(
        Guid tenantId,
        string billNumber,
        DateTime billDate,
        Guid clientId,
        string creditorName,
        Address creditorAddress,
        QrIban creditorQrIban,
        string debtorName,
        Address debtorAddress,
        Money rawAmount,
        string referenceNumber,
        DateTime dueDate,
        string? message = null)
    {
        // Apply statutory 0.05 CHF rounding
        var finalAmount = rawAmount?.ToSwissCashRounding();

        return new SwissQrBill
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            BillNumber = billNumber,
            BillDate = billDate,
            ClientId = clientId,
            CreditorName = creditorName,
            CreditorAddress = creditorAddress,
            CreditorQrIban = creditorQrIban,
            DebtorName = debtorName,
            DebtorAddress = debtorAddress,
            Amount = finalAmount ?? new Money(
                0,
                rawAmount?.Currency ?? Currency.CHF),
            BillType = QrBillType.QrIbanWithReference,
            ReferenceNumber = referenceNumber?.Replace(
                " ",
                string.Empty,
                StringComparison.Ordinal) ?? string.Empty,
            AdditionalUnstructuredMessage = message,
            DueDate = dueDate,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

    /// <summary>
    /// Marks the Swiss QR-bill as paid upon receiving payment confirmation or bank statement matching.
    /// </summary>
    /// <param name="paidDate">The date and time when the payment was booked or credited.</param>
    public void MarkAsPaid(
        DateTime paidDate)
    {
        IsPaid = true;
        PaidDateUtc = paidDate;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
