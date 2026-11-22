// <copyright file="ProvisionLine.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Receivables;

/// <summary>
/// The provision of one aging bucket.
/// </summary>
/// <param name="Key">The bucket key.</param>
/// <param name="From">The first day past due.</param>
/// <param name="To">The last day past due, null when open-ended.</param>
/// <param name="OpenAmount">The open amount in the bucket.</param>
/// <param name="RatePercent">The rate in percent.</param>
/// <param name="Provision">The provision: open amount times the rate.</param>
/// <param name="IsDefaultRate">Whether the rate is the default one.</param>
public sealed record ProvisionLine(
    string Key,
    int From,
    int? To,
    decimal OpenAmount,
    decimal RatePercent,
    decimal Provision,
    bool IsDefaultRate);
