// <copyright file="ProcurementQueue.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Procurement;

/// <summary>
/// The queued writes.
/// </summary>
/// <param name="Pending">Writes waiting to be sent.</param>
/// <param name="Conflicts">Writes the server refused.</param>
public sealed record ProcurementQueue(
    IReadOnlyList<ProcurementPendingAction> Pending,
    IReadOnlyList<ProcurementPendingAction> Conflicts);
