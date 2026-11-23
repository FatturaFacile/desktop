// <copyright file="ProcurementPendingAction.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.Procurement;

/// <summary>
/// A queued write for the list.
/// </summary>
/// <param name="Id">The entry.</param>
/// <param name="Action">The action.</param>
/// <param name="Reference">The label.</param>
/// <param name="OccurredAtUtc">When the user acted (UTC).</param>
/// <param name="Error">The last error.</param>
public sealed record ProcurementPendingAction(
    Guid Id,
    ProcurementAction Action,
    string Reference,
    DateTime OccurredAtUtc,
    string? Error);
