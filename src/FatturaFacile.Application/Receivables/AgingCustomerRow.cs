// <copyright file="AgingCustomerRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Receivables;

/// <summary>
/// The aging of one customer.
/// </summary>
/// <param name="CustomerId">The customer.</param>
/// <param name="CustomerName">The customer name.</param>
/// <param name="Buckets">The open amount per bucket key.</param>
/// <param name="TotalOpen">The total open amount.</param>
/// <param name="TotalOverdue">The overdue amount.</param>
/// <param name="Items">The number of open items.</param>
public sealed record AgingCustomerRow(
    Guid CustomerId,
    string CustomerName,
    IReadOnlyDictionary<string, decimal> Buckets,
    decimal TotalOpen,
    decimal TotalOverdue,
    int Items);
