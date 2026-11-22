// <copyright file="StoredAnalyticalPayload.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.AnalyticalAccounting;

/// <summary>
/// A cached payload with the instant it was pulled.
/// </summary>
/// <param name="Json">The payload as received.</param>
/// <param name="SyncedAtUtc">When it was pulled.</param>
public sealed record StoredAnalyticalPayload(string Json, DateTime SyncedAtUtc);
