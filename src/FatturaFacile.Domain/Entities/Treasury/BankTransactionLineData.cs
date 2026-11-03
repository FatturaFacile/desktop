// <copyright file="BankTransactionLineData.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Treasury;

/// <summary>
/// Represents a single line of a bank transaction statement, containing details about the transaction.
/// </summary>
/// <param name="StatementId">The unique identifier of the parent <see cref="BankStatement"/>.</param>
/// <param name="BookingDate">The date when the bank recorded the transaction on the account.</param>
/// <param name="ValueDate">The value date for interest computation.</param>
/// <param name="Amount">The signed transaction value (positive for inflow credits, negative for outflow debits).</param>
/// <param name="Currency">The currency of the transaction amount.</param>
/// <param name="Remittance">The payment reference or unstructured remittance text.</param>
/// <param name="CounterpartyIban">The optional IBAN of the transaction counterparty.</param>
/// <param name="CounterpartyName">The optional name of the transaction counterparty.</param>
public sealed record BankTransactionLineData(
    Guid     StatementId,
    DateTime BookingDate,
    DateTime ValueDate,
    decimal  Amount,
    Currency Currency,
    string   Remittance,
    string?  CounterpartyIban,
    string?  CounterpartyName);
