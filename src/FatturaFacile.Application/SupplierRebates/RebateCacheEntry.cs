// <copyright file="RebateCacheEntry.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.SupplierRebates;

/// <summary>
/// A payload to store in the local cache of the supplier rebates.
/// </summary>
/// <param name="Kind">The kind of payload (see <see cref="RebateCacheKinds"/>).</param>
/// <param name="Key">The key of the record the payload belongs to.</param>
/// <param name="Json">The `data` payload as received.</param>
public sealed record RebateCacheEntry(string Kind, string Key, string Json);
