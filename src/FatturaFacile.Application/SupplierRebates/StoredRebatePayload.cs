// <copyright file="StoredRebatePayload.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.SupplierRebates;

/// <summary>
/// A cached payload and the instant it was pulled.
/// </summary>
/// <param name="Json">The `data` payload as received.</param>
/// <param name="SyncedAtUtc">When it was pulled.</param>
public sealed record StoredRebatePayload(string Json, DateTime SyncedAtUtc);
