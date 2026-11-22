// <copyright file="FieldServiceOutboxSummary.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.FieldService;

/// <summary>
/// The state of the outbox of field operations.
/// </summary>
/// <param name="Pending">The operations waiting to be sent.</param>
/// <param name="Conflicts">The rejected operations the user has not dismissed.</param>
public sealed record FieldServiceOutboxSummary(int Pending, IReadOnlyList<FieldServiceConflict> Conflicts);
