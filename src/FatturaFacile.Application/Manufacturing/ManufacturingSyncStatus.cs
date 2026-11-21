// <copyright file="ManufacturingSyncStatus.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Manufacturing;

/// <summary>
/// The outcome of a manufacturing sync.
/// </summary>
public enum ManufacturingSyncStatus
{
    /// <summary>
    /// The data was pulled and cached.
    /// </summary>
    Synced = 0,

    /// <summary>
    /// The server could not be reached; the cache is untouched.
    /// </summary>
    Offline = 1,

    /// <summary>
    /// There is no session or it expired.
    /// </summary>
    Unauthorized = 2,

    /// <summary>
    /// The user may not read the manufacturing data.
    /// </summary>
    Forbidden = 3,

    /// <summary>
    /// The server or the payload failed; the cache is untouched.
    /// </summary>
    Failed = 4,
}
