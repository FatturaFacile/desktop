// <copyright file="ReceivablesPendingAction.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.Receivables;

/// <summary>
/// A queued or refused receivables action, for the queue list.
/// </summary>
/// <param name="Id">The entry identifier.</param>
/// <param name="Action">The action.</param>
/// <param name="Reference">The label of the action.</param>
/// <param name="OccurredAtUtc">The instant the user acted.</param>
/// <param name="Error">The reason the server gave when it refused the action.</param>
public sealed record ReceivablesPendingAction(
    Guid Id,
    ReceivablesAction Action,
    string Reference,
    DateTime OccurredAtUtc,
    string? Error);
