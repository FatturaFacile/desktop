// <copyright file="IWithholdingTaxTariffStore.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Common.Interfaces;

/// <summary>
/// Bulk storage of the ESTV withholding tax tariffs (tens of thousands of rows per canton).
/// </summary>
public interface IWithholdingTaxTariffStore
{
    /// <summary>
    /// Replaces the rows of the cantons and validity dates contained in the import.
    /// </summary>
    /// <param name="rows">The imported rows.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of stored rows.</returns>
    public Task<int> ReplaceAsync(IReadOnlyList<WithholdingTaxTariff> rows, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the rows of a canton and tariff code (all validity dates).
    /// </summary>
    /// <param name="canton">The canton.</param>
    /// <param name="tariffCode">The tariff code.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The rows.</returns>
    public Task<IReadOnlyList<WithholdingTaxTariff>> GetAsync(string canton, string tariffCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a summary of the imported tariffs by canton and validity date.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The summary.</returns>
    public Task<IReadOnlyList<WithholdingTaxTariffSummaryDto>> GetSummaryAsync(CancellationToken cancellationToken = default);
}
