// <copyright file="GeneralLedgerRepository.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Accounting.Domain.Aggregates;
using FatturaFacile.Accounting.Domain.Repositories;
using FatturaFacile.Accounting.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Accounting.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of the <see cref="IGeneralLedgerRepository"/> interface.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GeneralLedgerRepository"/> class.
/// </remarks>
/// <param name="context">The accounting database context.</param>
public sealed class GeneralLedgerRepository(AccountingDbContext context) : IGeneralLedgerRepository
{
    private readonly AccountingDbContext _context = context;

    /// <inheritdoc />
    public Task<GeneralLedgerEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _context.GeneralLedgerEntries
            .Include(g => g.Lines)
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(GeneralLedgerEntry entry, CancellationToken cancellationToken = default)
    {
        _ = await _context.GeneralLedgerEntries.AddAsync(entry, cancellationToken);
        _ = await _context.SaveChangesAsync(cancellationToken);
    }
}
