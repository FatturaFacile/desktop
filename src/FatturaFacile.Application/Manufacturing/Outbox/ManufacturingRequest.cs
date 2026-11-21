// <copyright file="ManufacturingRequest.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Manufacturing.Outbox;

/// <summary>
/// A write sent to the web-api.
/// </summary>
/// <param name="Route">The relative route below /api/v1/manufacturing.</param>
/// <param name="JsonPayload">The JSON body.</param>
/// <param name="IdempotencyKey">The idempotency key.</param>
public sealed record ManufacturingRequest(string Route, string? JsonPayload, Guid IdempotencyKey);
