// <copyright file="ManufacturingSyncResult.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Manufacturing;

/// <summary>
/// The result of a manufacturing sync.
/// </summary>
/// <param name="Status">The outcome.</param>
/// <param name="SyncedAtUtc">When the data was pulled (only when synced).</param>
/// <param name="Detail">The technical detail of a failure.</param>
public sealed record ManufacturingSyncResult(ManufacturingSyncStatus Status, DateTime? SyncedAtUtc = null, string? Detail = null);
