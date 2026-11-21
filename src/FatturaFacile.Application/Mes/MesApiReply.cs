// <copyright file="MesApiReply.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Net;

namespace FatturaFacile.Application.Mes;

/// <summary>
/// The answer of the web-api to a MES command.
/// </summary>
/// <param name="Status">The HTTP status.</param>
/// <param name="Body">The body text.</param>
public sealed record MesApiReply(HttpStatusCode Status, string Body)
{
    /// <summary>
    /// Gets a value indicating whether the server accepted the command.
    /// </summary>
    public bool IsSuccess => (int)Status is >= 200 and < 300;
}
