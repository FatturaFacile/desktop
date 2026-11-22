// <copyright file="AnalyticalSyncStatus.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.AnalyticalAccounting;

/// <summary>
/// How a sync of the analytical cost accounting module ended.
/// </summary>
public enum AnalyticalSyncStatus
{
    /// <summary>
    /// The server answered and the cache was refreshed.
    /// </summary>
    Synced = 0,

    /// <summary>
    /// The server could not be reached; the cache and the queue are untouched.
    /// </summary>
    Offline = 1,

    /// <summary>
    /// The user is not signed in, or the server refused the token.
    /// </summary>
    Unauthorized = 2,

    /// <summary>
    /// The user may not read the analytical accounting.
    /// </summary>
    Forbidden = 3,

    /// <summary>
    /// The server answered with an error or an unreadable payload; the cache is untouched.
    /// </summary>
    Failed = 4,
}
