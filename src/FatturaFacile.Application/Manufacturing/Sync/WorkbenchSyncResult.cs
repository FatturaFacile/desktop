// <copyright file="WorkbenchSyncResult.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Manufacturing.Outbox;

namespace FatturaFacile.Application.Manufacturing.Sync;

/// <summary>
/// The outcome of a manufacturing synchronisation.
/// </summary>
/// <param name="Push">The outbox flush.</param>
/// <param name="Pulled">The rows applied locally.</param>
/// <param name="Skipped">The rows skipped (protected or unchanged).</param>
/// <param name="Offline">A value indicating whether the server could not be reached.</param>
/// <param name="Errors">The error messages.</param>
public sealed record WorkbenchSyncResult(OutboxFlushResult Push, int Pulled, int Skipped, bool Offline, IReadOnlyList<string> Errors);
