// <copyright file="ReceivablesApiReply.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Net;

namespace FatturaFacile.Application.Receivables;

/// <summary>
/// The raw answer of a receivables write.
/// </summary>
/// <param name="Status">The HTTP status.</param>
/// <param name="Body">The body text.</param>
public sealed record ReceivablesApiReply(HttpStatusCode Status, string Body)
{
    /// <summary>
    /// Gets a value indicating whether the status is a success (2xx).
    /// </summary>
    public bool IsSuccess => (int)Status is >= 200 and < 300;
}
