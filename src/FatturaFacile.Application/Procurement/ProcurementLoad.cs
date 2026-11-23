// <copyright file="ProcurementLoad.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Procurement;

/// <summary>
/// A read answer with its origin.
/// </summary>
/// <typeparam name="T">The type of the data.</typeparam>
/// <param name="Data">The data.</param>
/// <param name="FromCache">Whether it comes from the local cache.</param>
/// <param name="SyncedAtUtc">When it was pulled (UTC).</param>
public sealed record ProcurementLoad<T>(
    T Data,
    bool FromCache,
    DateTime SyncedAtUtc);
