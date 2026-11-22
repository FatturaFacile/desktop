// <copyright file="AllocationSplitResult.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.AnalyticalAccounting;

/// <summary>
/// The result of splitting a line over allocation vectors.
/// </summary>
/// <param name="IsValid">Whether the vectors are valid and add up.</param>
/// <param name="Errors">The error codes (resource keys) when invalid.</param>
/// <param name="Amounts">The amount of every vector, in order, summing exactly to the line amount; empty when invalid.</param>
public sealed record AllocationSplitResult(bool IsValid, IReadOnlyList<string> Errors, IReadOnlyList<decimal> Amounts);
