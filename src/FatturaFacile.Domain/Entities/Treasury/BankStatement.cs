using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Treasury;

/// <summary>
/// Represents bank statement.
/// </summary>
public sealed class BankStatement : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    /// <summary>
    /// Gets or sets the tenant id.
    /// </summary>
    public Guid TenantId { get; set; }

/// <summary>
    /// Gets the statement number.
    /// </summary>
    public string StatementNumber { get; private set; } = string.Empty;

/// <summary>
    /// Gets the account iban.
    /// </summary>
    public string AccountIban { get; private set; } = string.Empty;

/// <summary>
    /// Gets the currency.
    /// </summary>
    public Currency Currency { get; private set; }

/// <summary>
    /// Gets the opening balance.
    /// </summary>
    public decimal OpeningBalance { get; private set; }

/// <summary>
    /// Gets the closing balance.
    /// </summary>
    public decimal ClosingBalance { get; private set; }

/// <summary>
    /// Gets the statement date.
    /// </summary>
    public DateTime StatementDate { get; private set; }

/// <summary>
    /// Gets the camt.053, camt.054, CBI.
    /// </summary>
    public string SourceFormat { get; private set; } = "camt.053";

private readonly List<BankTransactionLine> _lines = [];

/// <summary>
    /// Gets the lines.
    /// </summary>
    public IReadOnlyCollection<BankTransactionLine> Lines => _lines.AsReadOnly();

/// <summary>
    /// Gets or sets the created at utc.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

/// <summary>
    /// Gets or sets the created by.
    /// </summary>
    public string? CreatedBy { get; set; }

/// <summary>
    /// Gets or sets the updated at utc.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; set; }

/// <summary>
    /// Gets or sets the updated by.
    /// </summary>
    public string? UpdatedBy { get; set; }

private BankStatement()
    {
    }

/// <summary>
    /// Create.
    /// </summary>
    /// <param name="tenantId">The tenant id.</param>
    /// <param name="statementNumber">The statement number.</param>
    /// <param name="accountIban">The account iban.</param>
    /// <param name="currency">The currency.</param>
    /// <param name="openingBalance">The opening balance.</param>
    /// <param name="closingBalance">The closing balance.</param>
    /// <param name="statementDate">The statement date.</param>
    /// <param name="sourceFormat">The source format.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
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

/// <summary>
    /// Add Transaction.
    /// </summary>
    /// <param name="bookingDate">The booking date.</param>
    /// <param name="valueDate">The value date.</param>
    /// <param name="amount">The amount.</param>
    /// <param name="remittance">The remittance.</param>
    /// <param name="counterpartyIban">The counterparty iban.</param>
    /// <param name="counterpartyName">The counterparty name.</param>
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
