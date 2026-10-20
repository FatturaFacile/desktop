using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Treasury;

/// <summary>
/// Represents bank transaction line.
/// </summary>
public sealed class BankTransactionLine : Entity<Guid>
{
    /// <summary>
    /// Gets the bank statement id.
    /// </summary>
    public Guid BankStatementId { get; }

/// <summary>
    /// Gets the booking date.
    /// </summary>
    public DateTime BookingDate { get; }

/// <summary>
    /// Gets the value date.
    /// </summary>
    public DateTime ValueDate { get; }

/// <summary>
    /// Gets the amount.
    /// </summary>
    public decimal Amount { get; }

/// <summary>
    /// Gets the currency.
    /// </summary>
    public Currency Currency { get; }

/// <summary>
    /// Gets the remittance information.
    /// </summary>
    public string RemittanceInformation { get; } = string.Empty;

/// <summary>
    /// Gets the counterparty iban.
    /// </summary>
    public string? CounterpartyIban { get; }

/// <summary>
    /// Gets the counterparty name.
    /// </summary>
    public string? CounterpartyName { get; }

/// <summary>
    /// Gets the reconciled invoice id.
    /// </summary>
    public Guid? ReconciledInvoiceId { get; private set; }

/// <summary>
    /// Gets a value indicating whether reconciled.
    /// </summary>
    public bool IsReconciled => ReconciledInvoiceId.HasValue;

/// <summary>
    /// Initializes a new instance of the <see cref="BankTransactionLine"/> class.
    /// </summary>
    /// <param name="statementId">The statement id.</param>
    /// <param name="bookingDate">The booking date.</param>
    /// <param name="valueDate">The value date.</param>
    /// <param name="amount">The amount.</param>
    /// <param name="currency">The currency.</param>
    /// <param name="remittance">The remittance.</param>
    /// <param name="counterpartyIban">The counterparty iban.</param>
    /// <param name="counterpartyName">The counterparty name.</param>
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
    /// Reconcile.
    /// </summary>
    /// <param name="invoiceId">The invoice id.</param>
    public void Reconcile(Guid invoiceId) => ReconciledInvoiceId = invoiceId;
}
