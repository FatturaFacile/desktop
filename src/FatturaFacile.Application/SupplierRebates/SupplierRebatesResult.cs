// <copyright file="SupplierRebatesResult.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.SupplierRebates;

/// <summary>
/// The outcome of a supplier rebate sync or server action.
/// </summary>
/// <param name="Status">How the attempt ended.</param>
/// <param name="SyncedAtUtc">When the cache was refreshed or the action was accepted, for a successful attempt.</param>
/// <param name="Detail">The failure detail or the message of the server, when there is one.</param>
/// <param name="Data">The `data` payload the server answered to an action, when there is one.</param>
public sealed record SupplierRebatesResult(SupplierRebatesStatus Status, DateTime? SyncedAtUtc = null, string? Detail = null, string? Data = null)
{
    /// <summary>
    /// Gets a value indicating whether the attempt succeeded.
    /// </summary>
    public bool IsSuccess => Status == SupplierRebatesStatus.Synced;
}
