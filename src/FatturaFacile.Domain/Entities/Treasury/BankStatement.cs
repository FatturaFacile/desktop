// <copyright file="BankStatement.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Treasury;

/// <summary>
/// Represents an electronic bank statement aggregate imported from financial interchange files (e.g. ISO 20022 camt.053,
/// camt.054 notification, or Italian CBI format), tracking account balances and transaction line entries.
/// </summary>
public sealed class BankStatement : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{

    /// <summary>
    /// The internal collection of bank transaction lines included in this statement.
    /// </summary>
    private readonly List<BankTransactionLine> lines = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="BankStatement"/> class.
    /// </summary>
    private BankStatement()
    {
    }

    /// <summary>
    /// Gets or sets the unique tenant identifier for multi-tenant data partitioning.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets the bank statement sequence or reference number assigned by the reporting financial institution (e.g., "STMT-2026/04").
    /// </summary>
    public string StatementNumber { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the International Bank Account Number (IBAN) of the reporting bank account.
    /// </summary>
    public string AccountIban { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the currency of the bank statement account balances and transactions.
    /// </summary>
    public Currency Currency { get; private set; }

    /// <summary>
    /// Gets the opening balance of the bank account at the start of the statement period.
    /// </summary>
    public decimal OpeningBalance { get; private set; }

    /// <summary>
    /// Gets the closing balance of the bank account at the end of the statement period.
    /// </summary>
    public decimal ClosingBalance { get; private set; }

    /// <summary>
    /// Gets the statement reporting date or period ending date.
    /// </summary>
    public DateTime StatementDate { get; private set; }

    /// <summary>
    /// Gets the electronic format identifier of the source bank payload (e.g. "camt.053", "camt.054", or "CBI").
    /// </summary>
    public string SourceFormat { get; private set; } = "camt.053";

    /// <summary>
    /// Gets the read-only collection of individual transaction debit and credit lines included in this statement.
    /// </summary>
    public IReadOnlyCollection<BankTransactionLine> Lines => lines.AsReadOnly();

    /// <summary>
    /// Gets or sets the UTC timestamp when this bank statement record was ingested into the system.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the user identifier or system process that imported this bank statement.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when this statement record was last updated.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the user identifier or system process that last updated this record.
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Creates a new <see cref="BankStatement"/> aggregate instance representing an imported electronic bank statement.
    /// </summary>
    /// <param name="data">The parameter object containing data required to create a bank statement.</param>
    /// <returns>A newly configured <see cref="BankStatement"/> aggregate.</returns>
    public static BankStatement Create(
        BankStatementData data)
    {
        ArgumentNullException.ThrowIfNull(data);

        return new BankStatement
        {
            Id              = Guid.NewGuid(),
            TenantId        = data.TenantId,
            StatementNumber = data.StatementNumber,
            AccountIban = data.AccountIban?.Replace(
                                               oldValue: " ",
                                               newValue: string.Empty,
                                               comparisonType: StringComparison.Ordinal) ?? string.Empty,
            Currency       = data.Currency,
            OpeningBalance = data.OpeningBalance,
            ClosingBalance = data.ClosingBalance,
            StatementDate  = data.StatementDate,
            SourceFormat   = data.SourceFormat,
            CreatedAtUtc   = DateTime.UtcNow,
        };
    }

    /// <summary>
    /// Appends an individual bank transaction movement line to this statement.
    /// </summary>
    /// <param name="bookingDate">The accounting booking date when the bank recorded the transaction.</param>
    /// <param name="valueDate">The interest or value date when the funds became effective.</param>
    /// <param name="amount">The signed transaction amount (positive for credits/inflows, negative for debits/outflows).</param>
    /// <param name="remittance">The unstructured or structured remittance payment information text.</param>
    /// <param name="counterpartyIban">The optional IBAN of the ordering customer or creditor beneficiary.</param>
    /// <param name="counterpartyName">The optional name of the transaction counterparty.</param>
    public void AddTransaction(
        DateTime bookingDate,
        DateTime valueDate,
        decimal  amount,
        string   remittance,
        string?  counterpartyIban = null,
        string?  counterpartyName = null)
    {
        var lineData = new BankTransactionLineData(
            StatementId: Id,
            BookingDate: bookingDate,
            ValueDate: valueDate,
            Amount: amount,
            Currency: Currency,
            Remittance: remittance,
            CounterpartyIban: counterpartyIban,
            CounterpartyName: counterpartyName);

        var line = new BankTransactionLine(lineData);

        lines.Add(item: line);
    }

}
