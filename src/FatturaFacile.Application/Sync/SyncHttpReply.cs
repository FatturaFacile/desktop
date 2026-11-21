// <copyright file="SyncHttpReply.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Net;

namespace FatturaFacile.Application.Sync;

/// <summary>
/// Raw reply of an authenticated web-api request.
/// </summary>
/// <param name="Status">The HTTP status code.</param>
/// <param name="Body">The response body.</param>
public sealed record SyncHttpReply(HttpStatusCode Status, string Body)
{
    /// <summary>
    /// Gets a value indicating whether the status is a 2xx success.
    /// </summary>
    public bool IsSuccess => (int)Status is >= 200 and < 300;
}
