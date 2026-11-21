// <copyright file="ScheduledTask.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Manufacturing.Scheduling;

/// <summary>
/// A scheduled operation.
/// </summary>
/// <param name="OrderId">The order identifier.</param>
/// <param name="OrderNumber">The order number.</param>
/// <param name="OperationId">The operation identifier.</param>
/// <param name="Sequence">The operation sequence.</param>
/// <param name="OperationName">The operation name.</param>
/// <param name="WorkCenterId">The work centre identifier.</param>
/// <param name="WorkCenterCode">The work centre code.</param>
/// <param name="Start">The start.</param>
/// <param name="End">The finish.</param>
/// <param name="Minutes">The scheduled minutes.</param>
public sealed record ScheduledTask(Guid OrderId, string OrderNumber, Guid OperationId, int Sequence, string OperationName, Guid WorkCenterId, string WorkCenterCode, DateTime Start, DateTime End, decimal Minutes);
