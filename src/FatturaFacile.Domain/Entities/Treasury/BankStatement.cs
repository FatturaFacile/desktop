using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Treasury;

public sealed class BankStatement : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    public Guid TenantId { get; set; }
    public string StatementNumber { get; private set; } = string.Empty;
    public string AccountIban { get; private set; } = string.Empty;
    public Currency Currency { get; private set; }
    public decimal OpeningBalance { get; private set; }
    public decimal ClosingBalance { get; private set; }
    public DateTime StatementDate { get; private set; }

    /// <summary>
    /// camt.053, camt.054, CBI
    /// </summary>
    public string SourceFormat { get; private set; } = "camt.053";

    private readonly List<BankTransactionLine> _lines = [];
    public IReadOnlyCollection<BankTransactionLine> Lines => _lines.AsReadOnly();

    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }

    private BankStatement()
    {
    }

    public static BankStatement Create(
        Guid tenantId,
        string statementNumber,
        string accountIban,
        Currency currency,
        decimal openingBalance,
        decimal closingBalance,
        DateTime statementDate,
        string sourceFormat = "camt.053")
    {
        return new BankStatement
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            StatementNumber = statementNumber,
            AccountIban = accountIban.Replace(" ", string.Empty, StringComparison.Ordinal),
            Currency = currency,
            OpeningBalance = openingBalance,
            ClosingBalance = closingBalance,
            StatementDate = statementDate,
            SourceFormat = sourceFormat,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

    public void AddTransaction(
        DateTime bookingDate,
        DateTime valueDate,
        decimal amount,
        string remittance,
        string? counterpartyIban = null,
        string? counterpartyName = null)
    {
        var line = new BankTransactionLine(Id, bookingDate, valueDate, amount, Currency, remittance, counterpartyIban, counterpartyName);
        _lines.Add(line);
    }
}