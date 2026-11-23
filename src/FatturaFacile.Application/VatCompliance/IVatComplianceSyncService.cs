// <copyright file="IVatComplianceSyncService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.VatCompliance;

/// <summary>
/// Syncs the VAT compliance data of a year with the web-api: sends the offline changes of the declarations of intent and of the
/// withholding payments, then pulls letters, settlements, withholdings and register protocols.
/// </summary>
public interface IVatComplianceSyncService
{
    /// <summary>
    /// Runs one sync of a year; a failure never discards local data.
    /// </summary>
    /// <param name="year">The year.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>How the attempt ended.</returns>
    public Task<VatComplianceSyncResult> SyncAsync(int year, CancellationToken cancellationToken);
}
