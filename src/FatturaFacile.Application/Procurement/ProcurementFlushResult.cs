// <copyright file="ProcurementFlushResult.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Procurement;

/// <summary>
/// The outcome of one outbox replay.
/// </summary>
/// <param name="Sent">Entries the server accepted.</param>
/// <param name="Conflicts">Entries the server refused.</param>
/// <param name="Remaining">Entries still waiting.</param>
/// <param name="Offline">Whether the server could not be reached.</param>
public sealed record ProcurementFlushResult(
    int Sent,
    int Conflicts,
    int Remaining,
    bool Offline);
