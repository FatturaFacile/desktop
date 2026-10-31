// <copyright file="IProductRepository.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Ports;

/// <summary>
/// Defines data access operations for product and service catalog items within a tenant.
/// </summary>
public interface IProductRepository : IRepository<Product>
{
    /// <summary>
    /// Retrieves all products and services available in the tenant's catalog.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to observe during query execution.</param>
    /// <returns>A read-only collection of catalog products.</returns>
    Task<IReadOnlyList<Product>> GetAllProductsAsync(CancellationToken cancellationToken = default);
}
