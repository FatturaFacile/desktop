// <copyright file="ManufacturingSendOutcome.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Sync;

namespace FatturaFacile.Application.Manufacturing.Outbox;

/// <summary>
/// The classified outcome of sending one outbox entry.
/// </summary>
/// <param name="Result">The classified result.</param>
/// <param name="StatusCode">The HTTP status code (0 when no response was received).</param>
/// <param name="Error">The error text.</param>
/// <param name="ResponseBody">The response body.</param>
public sealed record ManufacturingSendOutcome(SyncSendResult Result, int StatusCode, string? Error, string? ResponseBody);
