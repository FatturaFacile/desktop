// <copyright file="AiRefreshStatus.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.AiCopilot;

/// <summary>
/// Outcome of refreshing the cache from the server.
/// </summary>
public enum AiRefreshStatus
{
    /// <summary>
    /// The cache is up to date.
    /// </summary>
    Ok = 0,

    /// <summary>
    /// The server could not be reached: the cached data is shown.
    /// </summary>
    Offline = 1,

    /// <summary>
    /// The user lacks the ai_review module (403).
    /// </summary>
    Forbidden = 2,

    /// <summary>
    /// There is no valid web-api session.
    /// </summary>
    AuthRequired = 3,
}
