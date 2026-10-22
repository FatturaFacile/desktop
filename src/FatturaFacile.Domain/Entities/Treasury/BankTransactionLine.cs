// <copyright file="BankTransactionLine.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Treasury;

/// <summary>
/// Represents an individual debit or credit transaction line within an electronic bank statement,
/// tracking settlement dates, remittance details, counterparties, and invoice reconciliation status.
/// </summary>
public sealed class BankTransactionLine : Entity<Guid>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BankTransactionLine"/> class with booking details,
    /// monetary amount, counterparty metadata, and remittance references.
    /// </summary>
    /// <param name="statementId">The unique identifier of the parent <see cref="BankStatement"/>.</param>
    /// <param name="bookingDate">The date when the bank recorded the transaction on the account.</param>
    /// <param name="valueDate">The value date for interest computation.</param>
    /// <param name="amount">The signed transaction value (positive for inflow credits, negative for outflow debits).</param>
    /// <param name="currency">The currency of the transaction amount.</param>
    /// <param name="remittance">The payment reference or unstructured remittance text.</param>
    /// <param name="counterpartyIban">The optional IBAN of the transaction counterparty.</param>
    /// <param name="counterpartyName">The optional name of the transaction counterparty.</param>
    internal BankTransactionLine(
        Guid statementId,
        DateTime bookingDate,
        DateTime valueDate,
        decimal amount,
        Currency currency,
        string remittance,
        string? counterpartyIban,
        string? counterpartyName)
    {
        Id = Guid.NewGuid();
        BankStatementId = statementId;
        BookingDate = bookingDate;
        ValueDate = valueDate;
        Amount = amount;
        Currency = currency;
        RemittanceInformation = remittance;
        CounterpartyIban = counterpartyIban;
        CounterpartyName = counterpartyName;
    }

    private BankTransactionLine()
    {
    }

    /// <summary>
    /// Gets the unique identifier of the parent <see cref="BankStatement"/> holding this transaction line.
    /// </summary>
    public Guid BankStatementId { get; }

    /// <summary>
    /// Gets the accounting booking date (tag &lt;BookgDt&gt;) on which the movement was posted to the bank account.
    /// </summary>
    public DateTime BookingDate { get; }

    /// <summary>
    /// Gets the value date (tag &lt;ValDt&gt;) when the funds legally became effective or interest calculation started.
    /// </summary>
    public DateTime ValueDate { get; }

    /// <summary>
    /// Gets the signed transactional amount, where positive amounts denote incoming credits (CRDT) and negative amounts denote outgoing debits (DBIT).
    /// </summary>
    public decimal Amount { get; }

    /// <summary>
    /// Gets the ISO currency code of this transaction amount.
    /// </summary>
    public Currency Currency { get; }

    /// <summary>
    /// Gets the remittance information (tag &lt;RmtInf&gt;), containing invoice numbers, structured references (e.g. SCOR/CBI), or payment rationales.
    /// </summary>
    public string RemittanceInformation { get; } = string.Empty;

    /// <summary>
    /// Gets the IBAN of the counterparty involved in the transaction, if available.
    /// </summary>
    public string? CounterpartyIban { get; }

    /// <summary>
    /// Gets the name of the counterparty involved in the transaction, if available.
    /// </summary>
    public string? CounterpartyName { get; }

    /// <summary>
    /// Gets the identifier of the matched sales or vendor invoice reconciled against this transaction line, or <see langword="null"/> if unreconciled.
    /// </summary>
    public Guid? ReconciledInvoiceId { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this transaction line has been reconciled against a corresponding invoice.
    /// </summary>
    public bool IsReconciled => ReconciledInvoiceId.HasValue;

    /// <summary>
    /// Reconciles this transaction movement against the specified invoice.
    /// </summary>
    /// <param name="invoiceId">The unique identifier of the matched sales invoice or passive vendor invoice.</param>
    public void Reconcile(
        Guid invoiceId) => ReconciledInvoiceId = invoiceId;
}
