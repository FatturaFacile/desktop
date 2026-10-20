using FatturaFacile.Domain.Entities.Administration;
using FatturaFacile.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.Persistence.Repositories;

/// <summary>
/// Represents tenant repository.
/// </summary>
/// <param name="dbContext">The db context.</param>
public sealed class TenantRepository(FatturaFacileDbContext dbContext) : Repository<Tenant, Guid>(dbContext), ITenantRepository
{
/// <summary>
/// Get By Code Async.
/// </summary>
/// <param name="code">The code.</param>
/// <param name="cancellationToken">The cancellation token.</param>
/// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public Task<Tenant?> GetByCodeAsync(string code, CancellationToken cancellationToken = default) => DbSet.FirstOrDefaultAsync(t => t.Code.Equals(code, StringComparison.OrdinalIgnoreCase), cancellationToken);

/// <summary>
/// Get Active Tenants Async.
/// </summary>
/// <param name="cancellationToken">The cancellation token.</param>
/// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public async Task<IReadOnlyList<Tenant>> GetActiveTenantsAsync(CancellationToken cancellationToken = default) => await DbSet.Where(t => t.IsActive).ToListAsync(cancellationToken);
}
