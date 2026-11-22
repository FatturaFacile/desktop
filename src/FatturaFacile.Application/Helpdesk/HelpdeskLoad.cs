// <copyright file="HelpdeskLoad.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Helpdesk;

/// <summary>
/// A helpdesk read with where it came from.
/// </summary>
/// <typeparam name="T">The kind of data.</typeparam>
/// <param name="Data">The data.</param>
/// <param name="FromCache">Whether the data is the last cached answer because the server could not be reached.</param>
/// <param name="SyncedAtUtc">The instant the data was pulled from the server.</param>
public sealed record HelpdeskLoad<T>(T Data, bool FromCache, DateTime SyncedAtUtc);
