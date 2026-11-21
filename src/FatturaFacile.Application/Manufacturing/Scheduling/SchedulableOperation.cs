// <copyright file="SchedulableOperation.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Manufacturing.Scheduling;

/// <summary>
/// An operation with the minutes still to be scheduled.
/// </summary>
/// <param name="OperationId">The operation identifier.</param>
/// <param name="Sequence">The sequence within the order.</param>
/// <param name="Name">The operation name.</param>
/// <param name="WorkCenterId">The work centre.</param>
/// <param name="Minutes">The remaining minutes (setup plus run).</param>
public sealed record SchedulableOperation(Guid OperationId, int Sequence, string Name, Guid WorkCenterId, decimal Minutes);
