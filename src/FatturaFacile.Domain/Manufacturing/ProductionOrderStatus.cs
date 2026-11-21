// <copyright file="ProductionOrderStatus.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Manufacturing;

/// <summary>
/// Lifecycle status of a production order (mirrors the web-api manufacturing module).
/// </summary>
public enum ProductionOrderStatus
{
    /// <summary>
    /// Planned.
    /// </summary>
    Planned = 0,

    /// <summary>
    /// Released to the shop floor.
    /// </summary>
    Released = 1,

    /// <summary>
    /// In progress.
    /// </summary>
    InProgress = 2,

    /// <summary>
    /// Completed.
    /// </summary>
    Completed = 3,

    /// <summary>
    /// Closed and costed.
    /// </summary>
    Closed = 4,

    /// <summary>
    /// Cancelled.
    /// </summary>
    Cancelled = 5,
}
