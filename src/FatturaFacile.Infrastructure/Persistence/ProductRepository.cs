// <copyright file="ProductRepository.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Ports;
using FatturaFacile.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core repository implementation for products and services in the tenant catalog.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ProductRepository"/> class.
/// </remarks>
/// <param name="context">The database context.</param>
public class ProductRepository(AppDbContext context) : RepositoryBase<Product>(context), IProductRepository
{


    /// <inheritdoc />
    public async Task<IReadOnlyList<Product>> GetAllProductsAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }
}
