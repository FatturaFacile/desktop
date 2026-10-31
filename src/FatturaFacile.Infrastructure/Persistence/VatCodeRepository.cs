// <copyright file="VatCodeRepository.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Ports;
using FatturaFacile.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core repository implementation for tax rates, standard VAT entries, and exemption codes.
/// </summary>
public class VatCodeRepository : RepositoryBase<VatCode>, IVatCodeRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VatCodeRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public VatCodeRepository(AppDbContext context) : base(context) { }

    /// <inheritdoc />
    public async Task<IReadOnlyList<VatCode>> GetActiveVatCodesAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(v => v.IsActive)
            .OrderByDescending(v => v.Rate)
            .ToListAsync(cancellationToken);
    }
}
