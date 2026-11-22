// <copyright file="FieldServiceCached.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.FieldService;

/// <summary>
/// A cached read model with the instant it was pulled.
/// </summary>
/// <typeparam name="T">The read model type.</typeparam>
/// <param name="Value">The read model.</param>
/// <param name="SyncedAtUtc">When it was pulled from the server (UTC).</param>
public sealed record FieldServiceCached<T>(T Value, DateTime SyncedAtUtc);
