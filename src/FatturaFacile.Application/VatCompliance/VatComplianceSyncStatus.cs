// <copyright file="VatComplianceSyncStatus.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.VatCompliance;

/// <summary>
/// How a sync attempt of the VAT compliance data ended.
/// </summary>
public enum VatComplianceSyncStatus
{
    /// <summary>
    /// Pushed and pulled.
    /// </summary>
    Synced = 0,

    /// <summary>
    /// The server could not be reached; the local data stays as it was.
    /// </summary>
    Offline = 1,

    /// <summary>
    /// No user is signed in, or the session expired.
    /// </summary>
    Unauthorized = 2,

    /// <summary>
    /// The user may not read or write the data.
    /// </summary>
    Forbidden = 3,

    /// <summary>
    /// The server answered something the application cannot use.
    /// </summary>
    Failed = 4,
}
