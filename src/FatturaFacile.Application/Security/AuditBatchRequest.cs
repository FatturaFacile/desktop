// <copyright file="AuditBatchRequest.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Security;

/// <summary>
/// One batch uploaded to the web-api.
/// </summary>
/// <param name="BatchId">The idempotency key of the batch (stable across retries).</param>
/// <param name="DeviceId">The producing device.</param>
/// <param name="Entries">The entries, in chain order.</param>
public sealed record AuditBatchRequest(Guid BatchId, string DeviceId, IReadOnlyList<LocalAuditEntry> Entries);
