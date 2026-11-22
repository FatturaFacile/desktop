// <copyright file="StoredFieldServicePayload.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.FieldService;

/// <summary>
/// A cached payload with its timestamp.
/// </summary>
/// <param name="Json">The payload as received.</param>
/// <param name="SyncedAtUtc">When it was pulled (UTC).</param>
public sealed record StoredFieldServicePayload(string Json, DateTime SyncedAtUtc);
