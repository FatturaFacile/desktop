// <copyright file="BlockedCustomer.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Receivables;

/// <summary>
/// A customer blocked for credit, from the dashboard.
/// </summary>
/// <param name="CustomerId">The customer.</param>
/// <param name="Reason">The reason of the block.</param>
/// <param name="Exposure">The exposure.</param>
/// <param name="Limit">The limit.</param>
public sealed record BlockedCustomer(
    Guid CustomerId,
    string Reason,
    decimal Exposure,
    decimal Limit);
