// <copyright file="ProjectBillingSyncStatus.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ProjectBilling;

/// <summary>
/// How a project billing sync attempt ended.
/// </summary>
public enum ProjectBillingSyncStatus
{
    /// <summary>
    /// The server answered and the cache was refreshed.
    /// </summary>
    Synced = 0,

    /// <summary>
    /// The server could not be reached; the cache is untouched.
    /// </summary>
    Offline = 1,

    /// <summary>
    /// The user is not signed in, or the server refused the token.
    /// </summary>
    Unauthorized = 2,

    /// <summary>
    /// The user may not read the billing of projects.
    /// </summary>
    Forbidden = 3,

    /// <summary>
    /// The server answered with an error or an unreadable payload; the cache is untouched.
    /// </summary>
    Failed = 4,
}
