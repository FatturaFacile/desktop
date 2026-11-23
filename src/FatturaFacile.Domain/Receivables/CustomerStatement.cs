// <copyright file="CustomerStatement.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Receivables;

/// <summary>
/// The statement of account of a customer for a period.
/// </summary>
/// <param name="CustomerName">The customer name.</param>
/// <param name="CompanyName">The name of the issuing company.</param>
/// <param name="Currency">The ISO currency.</param>
/// <param name="From">The first day of the period.</param>
/// <param name="To">The last day of the period.</param>
/// <param name="AsOf">The day the statement was prepared.</param>
/// <param name="OpeningBalance">The balance at the start of the period.</param>
/// <param name="Movements">The movements of the period.</param>
/// <param name="ClosingBalance">The balance at the end of the period.</param>
/// <param name="OpenItems">The items still open.</param>
/// <param name="TotalOpen">The total open amount.</param>
/// <param name="TotalOverdue">The total overdue amount.</param>
/// <param name="Aging">The open amount per aging bucket.</param>
public sealed record CustomerStatement(
    string CustomerName,
    string CompanyName,
    string Currency,
    DateOnly From,
    DateOnly To,
    DateOnly AsOf,
    decimal OpeningBalance,
    IReadOnlyList<LedgerMovement> Movements,
    decimal ClosingBalance,
    IReadOnlyList<ReceivableItem> OpenItems,
    decimal TotalOpen,
    decimal TotalOverdue,
    IReadOnlyDictionary<string, decimal> Aging);
