// <copyright file="RoutingStepResult.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Manufacturing;

/// <summary>
/// The lead time and cost of one operation step.
/// </summary>
/// <param name="OperationId">The operation identifier.</param>
/// <param name="Sequence">The sequence.</param>
/// <param name="OperationCode">The operation code.</param>
/// <param name="OperationType">The operation type.</param>
/// <param name="WorkCenterId">The work centre identifier.</param>
/// <param name="QueueMinutes">The queue time.</param>
/// <param name="SetupMinutes">The setup time.</param>
/// <param name="RunMinutes">The run time for the whole quantity.</param>
/// <param name="PostOperationWaitMinutes">The wait after the operation.</param>
/// <param name="MoveMinutes">The move time.</param>
/// <param name="DurationMinutes">The duration of the step.</param>
/// <param name="StartOffsetMinutes">The start offset from the beginning of the routing.</param>
/// <param name="EndOffsetMinutes">The end offset from the beginning of the routing.</param>
/// <param name="OverlapApplied">Whether the next step starts on a partial transfer batch.</param>
/// <param name="Cost">The cost of the step.</param>
public sealed record RoutingStepResult(
    Guid OperationId,
    int Sequence,
    string OperationCode,
    string OperationType,
    Guid WorkCenterId,
    double QueueMinutes,
    double SetupMinutes,
    double RunMinutes,
    double PostOperationWaitMinutes,
    double MoveMinutes,
    double DurationMinutes,
    double StartOffsetMinutes,
    double EndOffsetMinutes,
    bool OverlapApplied,
    RoutingCostBreakdown Cost);
