// <copyright file="OptimisticOrderChange.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Manufacturing;

namespace FatturaFacile.Application.Manufacturing.Outbox;

/// <summary>
/// The local effect applied immediately when a mutation is queued, so the workbench reflects it while offline.
/// </summary>
/// <param name="OrderId">The local order identifier.</param>
/// <param name="NewStatus">The new status, when the mutation moves the order.</param>
/// <param name="OperationId">The confirmed operation, when any.</param>
/// <param name="GoodDelta">The good quantity to add to the operation.</param>
/// <param name="ScrapDelta">The scrap quantity to add to the operation.</param>
/// <param name="MinutesDelta">The setup plus run minutes to add to the operation.</param>
/// <param name="CountsAsOutput">A value indicating whether the good quantity also counts as output of the order (last operation).</param>
public sealed record OptimisticOrderChange(
    Guid OrderId,
    ProductionOrderStatus? NewStatus,
    Guid? OperationId = null,
    decimal GoodDelta = 0m,
    decimal ScrapDelta = 0m,
    decimal MinutesDelta = 0m,
    bool CountsAsOutput = false);
