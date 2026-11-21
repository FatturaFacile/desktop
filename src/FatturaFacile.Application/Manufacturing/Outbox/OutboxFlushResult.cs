// <copyright file="OutboxFlushResult.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Manufacturing.Outbox;

/// <summary>
/// The outcome of a flush of the outbox.
/// </summary>
/// <param name="Sent">The entries applied on the server.</param>
/// <param name="Replayed">The confirmations the server had already applied (HTTP 200).</param>
/// <param name="Conflicts">The entries waiting for a manual decision.</param>
/// <param name="Discarded">The entries dropped because the server state won.</param>
/// <param name="Failed">The entries rejected or out of retries.</param>
/// <param name="Deferred">The entries held back.</param>
/// <param name="AuthRequired">A value indicating whether the session expired.</param>
/// <param name="Errors">The error messages.</param>
public sealed record OutboxFlushResult(int Sent, int Replayed, int Conflicts, int Discarded, int Failed, int Deferred, bool AuthRequired, IReadOnlyList<string> Errors);
