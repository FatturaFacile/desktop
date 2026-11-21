// <copyright file="AuditQueueStatus.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// Delivery state of an entry of the local audit queue.
/// </summary>
public enum AuditQueueStatus
{
    /// <summary>
    /// Waiting to be uploaded to the web-api.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Accepted by the web-api.
    /// </summary>
    Synced = 1,
}
