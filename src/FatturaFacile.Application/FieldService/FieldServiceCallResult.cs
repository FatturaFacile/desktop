// <copyright file="FieldServiceCallResult.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.FieldService;

/// <summary>
/// How an online call to the web-api ended.
/// </summary>
/// <param name="Status">The outcome.</param>
/// <param name="Detail">The error detail, when there is one.</param>
/// <param name="AtUtc">When the call succeeded (UTC).</param>
/// <param name="Id">The identifier of the record the server created or changed.</param>
public sealed record FieldServiceCallResult(FieldServiceCallStatus Status, string? Detail = null, DateTime? AtUtc = null, string? Id = null);
