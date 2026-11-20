// <copyright file="SyncSendResult.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Sync;

/// <summary>
/// Classified outcome of replaying one write.
/// </summary>
public enum SyncSendResult
{
    /// <summary>
    /// Accepted by the server.
    /// </summary>
    Success = 0,

    /// <summary>
    /// The server answered 409 or 412: the record changed on the server.
    /// </summary>
    Conflict = 1,

    /// <summary>
    /// The server permanently refused the write (validation or authorization).
    /// </summary>
    Rejected = 2,

    /// <summary>
    /// Network failure, throttling or server error: retry later.
    /// </summary>
    Transient = 3,

    /// <summary>
    /// The session expired and could not be refreshed.
    /// </summary>
    Unauthorized = 4,
}
