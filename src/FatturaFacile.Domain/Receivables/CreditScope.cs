// <copyright file="CreditScope.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Receivables;

/// <summary>
/// The operation a credit check or an override applies to.
/// </summary>
public enum CreditScope
{
    /// <summary>
    /// Every operation (overrides only).
    /// </summary>
    All = 0,

    /// <summary>
    /// The confirmation of an order.
    /// </summary>
    Order = 1,

    /// <summary>
    /// The issue of a delivery note.
    /// </summary>
    DeliveryNote = 2,
}
