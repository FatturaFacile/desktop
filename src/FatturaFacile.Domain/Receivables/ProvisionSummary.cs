// <copyright file="ProvisionSummary.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Receivables;

/// <summary>
/// The prudential bad-debt provision over the buckets.
/// </summary>
/// <param name="Lines">The lines in aging order.</param>
/// <param name="TotalOpen">The total open amount.</param>
/// <param name="TotalProvision">The total provision.</param>
/// <param name="NetRealizableValue">The open amount minus the provision.</param>
public sealed record ProvisionSummary(
    IReadOnlyList<ProvisionLine> Lines,
    decimal TotalOpen,
    decimal TotalProvision,
    decimal NetRealizableValue);
