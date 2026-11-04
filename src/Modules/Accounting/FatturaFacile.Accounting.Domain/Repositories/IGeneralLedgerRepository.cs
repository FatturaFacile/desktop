using FatturaFacile.Domain.Shared.Repositories;
using FatturaFacile.Accounting.Domain.Aggregates;

namespace FatturaFacile.Accounting.Domain.Repositories;

public interface IGeneralLedgerRepository : IRepository
{
    Task<GeneralLedgerEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(GeneralLedgerEntry entry, CancellationToken cancellationToken = default);
}
