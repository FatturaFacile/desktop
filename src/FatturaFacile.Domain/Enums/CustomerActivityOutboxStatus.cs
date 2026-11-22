// <copyright file="CustomerActivityOutboxStatus.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// Delivery state of a customer quick action saved offline.
/// </summary>
public enum CustomerActivityOutboxStatus
{
    /// <summary>
    /// Waiting to be sent.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Refused by the server; kept for inspection and never retried.
    /// </summary>
    Conflict = 1,
}
