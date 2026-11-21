// <copyright file="StoredDssPayload.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Dss;

/// <summary>
/// A stored payload and the instant it was pulled.
/// </summary>
/// <param name="Payload">The payload.</param>
/// <param name="SyncedAtUtc">When it was pulled.</param>
public sealed record StoredDssPayload(DssPayload Payload, DateTime SyncedAtUtc);
