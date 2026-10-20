using FatturaFacile.Domain.Entities.Administration;

namespace FatturaFacile.Domain.Repositories;

public interface ITenantRepository : IRepository<Tenant, Guid>
{
    public Task<Tenant?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    public Task<IReadOnlyList<Tenant>> GetActiveTenantsAsync(CancellationToken cancellationToken = default);
}