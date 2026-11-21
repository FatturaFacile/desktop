// <copyright file="ManufacturingMutationKind.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Manufacturing;

/// <summary>
/// Kind of mutation queued while offline.
/// </summary>
public enum ManufacturingMutationKind
{
    /// <summary>
    /// Create a production order.
    /// </summary>
    CreateOrder = 0,

    /// <summary>
    /// Release an order.
    /// </summary>
    Release = 1,

    /// <summary>
    /// Start an order.
    /// </summary>
    Start = 2,

    /// <summary>
    /// Complete an order.
    /// </summary>
    Complete = 3,

    /// <summary>
    /// Close an order.
    /// </summary>
    Close = 4,

    /// <summary>
    /// Confirm an operation (shop-floor posting).
    /// </summary>
    ConfirmOperation = 5,
}
