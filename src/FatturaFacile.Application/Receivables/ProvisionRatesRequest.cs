// <copyright file="ProvisionRatesRequest.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Receivables;

/// <summary>
/// Changes the bad-debt provision rates.
/// </summary>
/// <param name="CommandId">The identifier of the command, which is also its idempotency key.</param>
/// <param name="Rates">The rate in percent per bucket key.</param>
/// <param name="OccurredAtUtc">The instant the user acted.</param>
public sealed record ProvisionRatesRequest(
    Guid CommandId,
    IReadOnlyDictionary<string, decimal> Rates,
    DateTime OccurredAtUtc);
