using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Treasury;

public sealed class BankTransactionLine : Entity<Guid>
{
    public Guid BankStatementId { get; }
    public DateTime BookingDate { get; }
    public DateTime ValueDate { get; }
    public decimal Amount { get; }
    public Currency Currency { get; }
    public string RemittanceInformation { get; } = string.Empty;
    public string? CounterpartyIban { get; }
    public string? CounterpartyName { get; }
    public Guid? ReconciledInvoiceId { get; private set; }
    public bool IsReconciled => ReconciledInvoiceId.HasValue;

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

    public void Reconcile(Guid invoiceId) => ReconciledInvoiceId = invoiceId;
}