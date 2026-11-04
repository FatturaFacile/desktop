using Microsoft.EntityFrameworkCore;
using FatturaFacile.Accounting.Domain.Aggregates;
using FatturaFacile.Accounting.Domain.Repositories;
using FatturaFacile.Accounting.Infrastructure.Persistence;

namespace FatturaFacile.Accounting.Infrastructure.Repositories;

public sealed class GeneralLedgerRepository : IGeneralLedgerRepository
{
    private readonly AccountingDbContext _context;

    public GeneralLedgerRepository(AccountingDbContext context)
    {
        _context = context;
    }

    public async Task<GeneralLedgerEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.GeneralLedgerEntries
            .Include(g => g.Lines)
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
    }

    public async Task AddAsync(GeneralLedgerEntry entry, CancellationToken cancellationToken = default)
    {
        await _context.GeneralLedgerEntries.AddAsync(entry, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
