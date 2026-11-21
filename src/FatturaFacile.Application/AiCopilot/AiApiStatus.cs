// <copyright file="AiApiStatus.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.AiCopilot;

/// <summary>
/// Classified result of an approve or reject call.
/// </summary>
public enum AiApiStatus
{
    /// <summary>
    /// The server accepted the decision.
    /// </summary>
    Success = 0,

    /// <summary>
    /// 409: the suggestion was already decided.
    /// </summary>
    AlreadyDecided = 1,

    /// <summary>
    /// 422: the proposal is incomplete; the suggestion stays pending.
    /// </summary>
    Incomplete = 2,

    /// <summary>
    /// 404: the suggestion is unknown to the server.
    /// </summary>
    NotFound = 3,

    /// <summary>
    /// 401: the session expired.
    /// </summary>
    Unauthorized = 4,

    /// <summary>
    /// Network failure, timeout or server error: the decision must be retried later.
    /// </summary>
    Transient = 5,

    /// <summary>
    /// Any other refusal (for example 403); retrying would not help.
    /// </summary>
    Refused = 6,
}
