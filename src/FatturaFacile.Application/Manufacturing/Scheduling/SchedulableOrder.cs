// <copyright file="SchedulableOrder.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Manufacturing.Scheduling;

/// <summary>
/// An order to schedule.
/// </summary>
/// <param name="OrderId">The order identifier.</param>
/// <param name="OrderNumber">The order number.</param>
/// <param name="Priority">The priority (lower first).</param>
/// <param name="EarliestStart">The earliest start.</param>
/// <param name="Operations">The operations.</param>
public sealed record SchedulableOrder(Guid OrderId, string OrderNumber, int Priority, DateTime EarliestStart, IReadOnlyList<SchedulableOperation> Operations);
