// <copyright file="AgingBucket.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Receivables;

/// <summary>
/// A range of days past the due date; current holds what is not yet due.
/// </summary>
/// <param name="Key">The key shared with the web-api: current, 1_30, 31_60, 61_90, 91_120, 121_plus.</param>
/// <param name="From">The first day past due.</param>
/// <param name="To">The last day past due, null for the open-ended bucket.</param>
public sealed record AgingBucket(
    string Key,
    int From,
    int? To);
