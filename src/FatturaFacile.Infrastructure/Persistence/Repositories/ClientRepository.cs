using FatturaFacile.Domain.Entities.Crm;
using FatturaFacile.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.Persistence.Repositories;

public sealed class ClientRepository(FatturaFacileDbContext dbContext) : Repository<Client, Guid>(dbContext), IClientRepository
{
    public Task<Client?> GetByVatOrFiscalCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return DbSet.FirstOrDefaultAsync(
            c =>
                (c.FiscalCode != null && c.FiscalCode.Value == code) ||
                (c.VatNumber != null && c.VatNumber.Value == code) ||
                c.SwissUid == code, cancellationToken);
    }

    public async Task<IReadOnlyList<Client>> GetActiveClientsAsync(CancellationToken cancellationToken = default) => await DbSet.Where(c => c.IsActive).ToListAsync(cancellationToken);
}