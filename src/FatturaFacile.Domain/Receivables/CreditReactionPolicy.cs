// <copyright file="CreditReactionPolicy.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Receivables;

/// <summary>
/// What the system does when a customer breaches the credit limit.
/// </summary>
public enum CreditReactionPolicy
{
    /// <summary>
    /// Only warns the sales desk.
    /// </summary>
    Warning = 0,

    /// <summary>
    /// Blocks the order confirmation; a delivery note only warns.
    /// </summary>
    BlockOrder = 1,

    /// <summary>
    /// Blocks the delivery note issue; an order only warns.
    /// </summary>
    BlockDeliveryNote = 2,

    /// <summary>
    /// Blocks both orders and delivery notes.
    /// </summary>
    CreditHold = 3,
}
