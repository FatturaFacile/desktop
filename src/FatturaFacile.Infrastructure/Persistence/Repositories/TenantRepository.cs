using FatturaFacile.Domain.Entities.Administration;
using FatturaFacile.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.Persistence.Repositories;

public sealed class TenantRepository(FatturaFacileDbContext dbContext) : Repository<Tenant, Guid>(dbContext), ITenantRepository
{
    public Task<Tenant?> GetByCodeAsync(string code, CancellationToken cancellationToken = default) => DbSet.FirstOrDefaultAsync(t => t.Code.Equals(code, StringComparison.InvariantCultureIgnoreCase), cancellationToken);

    public async Task<IReadOnlyList<Tenant>> GetActiveTenantsAsync(CancellationToken cancellationToken = default) => await DbSet.Where(t => t.IsActive).ToListAsync(cancellationToken);
}