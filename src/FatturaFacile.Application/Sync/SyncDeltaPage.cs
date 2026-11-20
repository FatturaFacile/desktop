// <copyright file="SyncDeltaPage.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.Json;

namespace FatturaFacile.Application.Sync;

/// <summary>
/// One page of the server delta feed (GET /api/v1/mobile/sync/{entity}).
/// </summary>
/// <param name="Data">Created or changed records as raw server JSON.</param>
/// <param name="Deleted">Identifiers of records deleted on the server.</param>
/// <param name="NextCursor">Cursor of the next page, if any.</param>
/// <param name="ServerTime">Server clock (UTC) when the page was produced.</param>
/// <param name="HasMore">Whether more pages follow.</param>
public sealed record SyncDeltaPage(
    IReadOnlyList<JsonElement> Data,
    IReadOnlyList<Guid> Deleted,
    string? NextCursor,
    DateTime ServerTime,
    bool HasMore);
