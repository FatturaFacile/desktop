// <copyright file="ConflictResolution.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Manufacturing.Outbox;

/// <summary>
/// The decision taken for a conflicting outbox entry.
/// </summary>
/// <param name="Decision">The decision.</param>
/// <param name="Reason">The human-readable reason.</param>
public sealed record ConflictResolution(ConflictDecision Decision, string Reason);
