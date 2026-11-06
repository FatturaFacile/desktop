// <copyright file="IDataRetentionService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.TimeTracking;

namespace FatturaFacile.Application.Common.Interfaces;

/// <summary>
/// Deletes the working time records whose retention period has expired.
/// </summary>
public interface IDataRetentionService
{
    /// <summary>
    /// Applies the retention rules to one company.
    /// </summary>
    /// <param name="tenantId">The company.</param>
    /// <param name="today">The current day.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The outcome.</returns>
    public Task<DataRetentionResult> ApplyAsync(Guid tenantId, DateTime today, CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies the retention rules to every company (run automatically at start-up).
    /// </summary>
    /// <param name="today">The current day.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The outcome for each company.</returns>
    public Task<IReadOnlyList<DataRetentionResult>> ApplyAllTenantsAsync(DateTime today, CancellationToken cancellationToken = default);
}
