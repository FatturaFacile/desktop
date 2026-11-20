// <copyright file="CopilotApiErrorKind.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Copilot;

/// <summary>
/// Classes of failures of the Copilot API, each shown to the user with a localized message.
/// </summary>
public enum CopilotApiErrorKind
{
    /// <summary>Not configured or not authenticated (401).</summary>
    Unauthorized = 0,

    /// <summary>The user or tenant may not use the Copilot (403).</summary>
    Forbidden = 1,

    /// <summary>The conversation or message does not exist (404).</summary>
    NotFound = 2,

    /// <summary>The action was already rejected or failed (409).</summary>
    Conflict = 3,

    /// <summary>Invalid overrides or the action failed (422).</summary>
    Unprocessable = 4,

    /// <summary>Rate limit or token quota exceeded (429).</summary>
    RateLimited = 5,

    /// <summary>The language model provider failed (502).</summary>
    ProviderUnavailable = 6,

    /// <summary>The server is not reachable.</summary>
    Offline = 7,

    /// <summary>Any other failure.</summary>
    Unknown = 8,
}
