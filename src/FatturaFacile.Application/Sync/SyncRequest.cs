// <copyright file="SyncRequest.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.Sync;

/// <summary>
/// A write replayed against the web-api.
/// </summary>
/// <param name="Operation">The operation to perform.</param>
/// <param name="Route">Collection route relative to the API root (for example "contacts").</param>
/// <param name="EntityId">Identifier of the record.</param>
/// <param name="IdempotencyKey">Value of the Idempotency-Key header.</param>
/// <param name="JsonPayload">JSON body for create and update, otherwise null.</param>
public sealed record SyncRequest(
    SyncOperation Operation,
    string Route,
    Guid EntityId,
    Guid IdempotencyKey,
    string? JsonPayload);
