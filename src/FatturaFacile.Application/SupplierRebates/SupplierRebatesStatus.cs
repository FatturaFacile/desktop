// <copyright file="SupplierRebatesStatus.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.SupplierRebates;

/// <summary>
/// How a supplier rebate sync or server action ended.
/// </summary>
public enum SupplierRebatesStatus
{
    /// <summary>
    /// The server answered and the operation succeeded.
    /// </summary>
    Synced = 0,

    /// <summary>
    /// The server could not be reached; nothing changed (the cache is untouched, an action is not queued).
    /// </summary>
    Offline = 1,

    /// <summary>
    /// The user is not signed in, or the server refused the token.
    /// </summary>
    Unauthorized = 2,

    /// <summary>
    /// The user may not use the supplier rebates.
    /// </summary>
    Forbidden = 3,

    /// <summary>
    /// The server answered with an error or an unreadable payload; nothing changed.
    /// </summary>
    Failed = 4,

    /// <summary>
    /// The server refused the action with a business message (validation, state or version conflict).
    /// </summary>
    Rejected = 5,
}
