// <copyright file="HelpdeskApiReply.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Net;

namespace FatturaFacile.Application.Helpdesk;

/// <summary>
/// The raw answer of a helpdesk write.
/// </summary>
/// <param name="Status">The HTTP status.</param>
/// <param name="Body">The body.</param>
public sealed record HelpdeskApiReply(HttpStatusCode Status, string Body)
{
    /// <summary>
    /// Gets a value indicating whether the status is 2xx.
    /// </summary>
    public bool IsSuccess => (int)Status is >= 200 and < 300;
}
