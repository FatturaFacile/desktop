// <copyright file="SyncApplyResult.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Sync;

/// <summary>
/// Outcome of applying a pulled page to the local database.
/// </summary>
/// <param name="Applied">Records created, updated or deleted locally.</param>
/// <param name="Skipped">Records left untouched because a local change is still pending.</param>
public sealed record SyncApplyResult(int Applied, int Skipped);
