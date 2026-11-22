// <copyright file="FieldServiceApiReply.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Net;

namespace FatturaFacile.Application.FieldService;

/// <summary>
/// The answer of the web-api to a field service call.
/// </summary>
/// <param name="Status">The HTTP status.</param>
/// <param name="Body">The body text.</param>
public sealed record FieldServiceApiReply(HttpStatusCode Status, string Body)
{
    /// <summary>
    /// Gets a value indicating whether the status is a success (2xx).
    /// </summary>
    public bool IsSuccess => (int)Status is >= 200 and < 300;
}
