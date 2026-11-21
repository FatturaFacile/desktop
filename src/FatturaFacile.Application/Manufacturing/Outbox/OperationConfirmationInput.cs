// <copyright file="OperationConfirmationInput.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Manufacturing.Outbox;

/// <summary>
/// A shop-floor operation confirmation entered by the supervisor.
/// </summary>
/// <param name="OrderId">The local order identifier.</param>
/// <param name="OperationId">The operation identifier.</param>
/// <param name="SetupMinutes">The setup minutes.</param>
/// <param name="RunMinutes">The run minutes.</param>
/// <param name="LaborMinutes">The labour minutes.</param>
/// <param name="DowntimeMinutes">The downtime minutes.</param>
/// <param name="GoodQuantity">The good quantity.</param>
/// <param name="ScrapQuantity">The scrap quantity.</param>
/// <param name="ScrapReason">The scrap reason (required when there is scrap).</param>
/// <param name="IsFinal">A value indicating whether this is the final confirmation of the operation.</param>
/// <param name="PostingDate">The posting date.</param>
public sealed record OperationConfirmationInput(
    Guid OrderId,
    Guid OperationId,
    decimal SetupMinutes,
    decimal RunMinutes,
    decimal LaborMinutes,
    decimal DowntimeMinutes,
    decimal GoodQuantity,
    decimal ScrapQuantity,
    string? ScrapReason,
    bool IsFinal,
    DateTime PostingDate);
