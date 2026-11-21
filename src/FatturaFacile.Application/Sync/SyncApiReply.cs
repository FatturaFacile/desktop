// <copyright file="SyncApiReply.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Net;
using System.Text;

namespace FatturaFacile.Application.Sync;

/// <summary>
/// The raw answer of an authorized web-api request.
/// </summary>
/// <param name="Status">The HTTP status.</param>
/// <param name="Content">The body bytes.</param>
public sealed record SyncApiReply(HttpStatusCode Status, ReadOnlyMemory<byte> Content)
{
    /// <summary>
    /// Gets a value indicating whether the status is a success (2xx).
    /// </summary>
    public bool IsSuccess => (int)Status is >= 200 and < 300;

    /// <summary>
    /// Gets the body as UTF-8 text.
    /// </summary>
    public string Text => Encoding.UTF8.GetString(Content.Span);
}
