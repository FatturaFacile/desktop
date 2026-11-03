// <copyright file="BankStatementData.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Treasury;

/// <summary>
/// Data transfer object holding values required to create a new <see cref="BankStatement"/>.
/// </summary>
/// <param name="TenantId">The unique tenant identifier.</param>
/// <param name="StatementNumber">The bank statement reference or sequence number.</param>
/// <param name="AccountIban">The IBAN of the reporting bank account.</param>
/// <param name="Currency">The currency code for the statement balances.</param>
/// <param name="OpeningBalance">The opening ledger balance.</param>
/// <param name="ClosingBalance">The final closing ledger balance.</param>
/// <param name="StatementDate">The date of the statement report.</param>
/// <param name="SourceFormat">The payload specification format (defaults to "camt.053").</param>
public sealed record BankStatementData(
    Guid     TenantId,
    string   StatementNumber,
    string   AccountIban,
    Currency Currency,
    decimal  OpeningBalance,
    decimal  ClosingBalance,
    DateTime StatementDate,
    string   SourceFormat = "camt.053");
