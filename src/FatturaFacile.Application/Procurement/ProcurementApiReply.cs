// <copyright file="ProcurementApiReply.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Procurement;

/// <summary>
/// The raw answer of the server to a write.
/// </summary>
/// <param name="Status">The HTTP status.</param>
/// <param name="Body">The body.</param>
public sealed record ProcurementApiReply(
    System.Net.HttpStatusCode Status,
    string Body)
{
    /// <summary>
    /// Gets a value indicating whether the status is 2xx.
    /// </summary>
    public bool IsSuccess => (int)Status is >= 200 and < 300;
}
