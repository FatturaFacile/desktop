// <copyright file="DssSyncStatus.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Dss;

/// <summary>
/// How a DSS sync attempt ended.
/// </summary>
public enum DssSyncStatus
{
    /// <summary>
    /// The snapshot was pulled and stored.
    /// </summary>
    Synced = 0,

    /// <summary>
    /// The server could not be reached: the cached snapshot (if any) stays in use.
    /// </summary>
    Offline = 1,

    /// <summary>
    /// There is no session or it expired: the user has to sign in.
    /// </summary>
    Unauthorized = 2,

    /// <summary>
    /// The user may not read the Executive DSS on the server.
    /// </summary>
    Forbidden = 3,

    /// <summary>
    /// The server answered with an error.
    /// </summary>
    Failed = 4,
}
