// <copyright file="CopilotApiException.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Copilot;

/// <summary>
/// A failure of a call to the Copilot API.
/// </summary>
public sealed class CopilotApiException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CopilotApiException"/> class.
    /// </summary>
    public CopilotApiException()
        : this(CopilotApiErrorKind.Unknown, statusCode: null, message: string.Empty)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CopilotApiException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    public CopilotApiException(string message)
        : this(CopilotApiErrorKind.Unknown, statusCode: null, message: message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CopilotApiException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="innerException">The inner exception.</param>
    public CopilotApiException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CopilotApiException"/> class.
    /// </summary>
    /// <param name="kind">The failure class.</param>
    /// <param name="statusCode">The HTTP status code, if any.</param>
    /// <param name="message">The server message.</param>
    /// <param name="innerException">The inner exception.</param>
    public CopilotApiException(CopilotApiErrorKind kind, int? statusCode, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        Kind = kind;
        StatusCode = statusCode;
    }

    /// <summary>
    /// Gets the failure class.
    /// </summary>
    public CopilotApiErrorKind Kind { get; }

    /// <summary>
    /// Gets the HTTP status code, if any.
    /// </summary>
    public int? StatusCode { get; }

    /// <summary>
    /// Maps an HTTP status code to a failure class.
    /// </summary>
    /// <param name="statusCode">The status code.</param>
    /// <returns>The failure class.</returns>
    public static CopilotApiErrorKind KindFromStatus(int statusCode) => statusCode switch
    {
        401 => CopilotApiErrorKind.Unauthorized,
        403 => CopilotApiErrorKind.Forbidden,
        404 => CopilotApiErrorKind.NotFound,
        409 => CopilotApiErrorKind.Conflict,
        422 => CopilotApiErrorKind.Unprocessable,
        429 => CopilotApiErrorKind.RateLimited,
        502 => CopilotApiErrorKind.ProviderUnavailable,
        _ => CopilotApiErrorKind.Unknown,
    };
}
