// <copyright file="ReceivablesQueue.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Receivables;

/// <summary>
/// The actions waiting for the server and those it refused.
/// </summary>
/// <param name="Pending">The actions waiting.</param>
/// <param name="Conflicts">The actions the server refused.</param>
public sealed record ReceivablesQueue(
    IReadOnlyList<ReceivablesPendingAction> Pending,
    IReadOnlyList<ReceivablesPendingAction> Conflicts);
