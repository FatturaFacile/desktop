// <copyright file="ProjectBillingSyncResult.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ProjectBilling;

/// <summary>
/// The outcome of a project billing sync.
/// </summary>
/// <param name="Status">How the attempt ended.</param>
/// <param name="SyncedAtUtc">When the cache was refreshed, for a successful sync.</param>
/// <param name="Detail">The failure detail, when there is one.</param>
public sealed record ProjectBillingSyncResult(ProjectBillingSyncStatus Status, DateTime? SyncedAtUtc = null, string? Detail = null);
