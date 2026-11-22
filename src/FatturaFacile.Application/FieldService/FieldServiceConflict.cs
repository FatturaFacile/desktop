// <copyright file="FieldServiceConflict.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.FieldService;

/// <summary>
/// A queued operation the server rejected.
/// </summary>
/// <param name="Id">The outbox entry.</param>
/// <param name="Operation">The kind of operation.</param>
/// <param name="WorkOrderId">The work order.</param>
/// <param name="OccurredAtUtc">When the technician acted.</param>
/// <param name="Error">What the server answered.</param>
public sealed record FieldServiceConflict(Guid Id, FieldServiceOperationType Operation, Guid WorkOrderId, DateTime OccurredAtUtc, string Error);
