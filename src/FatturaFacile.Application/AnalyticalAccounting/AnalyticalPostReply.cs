// <copyright file="AnalyticalPostReply.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.AnalyticalAccounting;

/// <summary>
/// The answer of the web-api to a queued entry.
/// </summary>
/// <param name="Accepted">Whether the entry was created.</param>
/// <param name="Refused">Whether the server understood and refused it (a 4xx other than an authentication error): retrying cannot help.</param>
/// <param name="Detail">The reason of a refusal.</param>
public sealed record AnalyticalPostReply(bool Accepted, bool Refused, string? Detail = null);
