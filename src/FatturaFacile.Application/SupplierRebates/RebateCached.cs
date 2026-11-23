// <copyright file="RebateCached.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.SupplierRebates;

/// <summary>
/// A value read from the local cache and the instant it was last pulled from the server.
/// </summary>
/// <typeparam name="T">The type of the value.</typeparam>
/// <param name="Value">The value (empty or default when nothing was pulled yet).</param>
/// <param name="SyncedAtUtc">When it was pulled, or null when nothing was pulled yet.</param>
public sealed record RebateCached<T>(T Value, DateTime? SyncedAtUtc);
