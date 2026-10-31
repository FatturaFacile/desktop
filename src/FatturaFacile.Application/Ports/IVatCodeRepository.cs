// <copyright file="IVatCodeRepository.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Ports;

/// <summary>
/// Defines data access operations for value-added tax (VAT) definitions, tax rates, and fiscal exemption codes.
/// </summary>
public interface IVatCodeRepository : IRepository<VatCode>
{
    /// <summary>
    /// Retrieves all active and currently selectable VAT codes for document line item calculations.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to observe during query execution.</param>
    /// <returns>A read-only collection of active VAT code configurations.</returns>
    Task<IReadOnlyList<VatCode>> GetActiveVatCodesAsync(CancellationToken cancellationToken = default);
}
