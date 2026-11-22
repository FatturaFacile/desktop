// <copyright file="ReceivablesAction.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// A receivables action that waits in the outbox for the server.
/// </summary>
public enum ReceivablesAction
{
    /// <summary>
    /// Applies allocations (payment, credit note, write-off, discount, debit adjustment) to open items.
    /// </summary>
    ApplyAllocations = 0,

    /// <summary>
    /// Reverses an allocation.
    /// </summary>
    ReverseAllocation = 1,

    /// <summary>
    /// Marks or clears the dispute of an item.
    /// </summary>
    SetDispute = 2,

    /// <summary>
    /// Creates or updates a credit limit.
    /// </summary>
    SaveCreditLimit = 3,

    /// <summary>
    /// Deletes a credit limit.
    /// </summary>
    DeleteCreditLimit = 4,

    /// <summary>
    /// Grants a credit override.
    /// </summary>
    GrantOverride = 5,

    /// <summary>
    /// Revokes a credit override.
    /// </summary>
    RevokeOverride = 6,

    /// <summary>
    /// Changes the bad-debt provision rates.
    /// </summary>
    UpdateProvisionRates = 7,

    /// <summary>
    /// Sends a customer statement by email.
    /// </summary>
    SendStatement = 8,
}
