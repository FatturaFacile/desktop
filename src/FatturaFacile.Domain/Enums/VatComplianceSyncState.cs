// <copyright file="VatComplianceSyncState.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// The offline sync state of a locally editable VAT compliance row.
/// </summary>
public enum VatComplianceSyncState
{
    /// <summary>
    /// The row equals its server copy.
    /// </summary>
    Synced = 0,

    /// <summary>
    /// Created offline; waits to be sent.
    /// </summary>
    PendingCreate = 1,

    /// <summary>
    /// Changed offline; waits to be sent.
    /// </summary>
    PendingUpdate = 2,

    /// <summary>
    /// Deleted offline; waits to be sent.
    /// </summary>
    PendingDelete = 3,

    /// <summary>
    /// Refused by the server; waits for the user to correct or discard it.
    /// </summary>
    Conflict = 4,
}
