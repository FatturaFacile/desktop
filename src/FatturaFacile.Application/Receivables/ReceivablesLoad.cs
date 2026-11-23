// <copyright file="ReceivablesLoad.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Receivables;

/// <summary>
/// A read answer and where it came from.
/// </summary>
/// <typeparam name="T">The kind of data.</typeparam>
/// <param name="Data">The data.</param>
/// <param name="FromCache">Whether the server was not reachable and the last cached answer is shown.</param>
/// <param name="SyncedAtUtc">The instant the answer was pulled from the server.</param>
public sealed record ReceivablesLoad<T>(
    T Data,
    bool FromCache,
    DateTime SyncedAtUtc);
