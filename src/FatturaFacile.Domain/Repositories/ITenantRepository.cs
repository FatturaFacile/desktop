using FatturaFacile.Domain.Entities.Administration;

namespace FatturaFacile.Domain.Repositories;

/// <summary>
/// Defines the contract for tenant repository.
/// </summary>
public interface ITenantRepository : IRepository<Tenant, Guid>
{
    /// <summary>
    /// Get By Code Async.
    /// </summary>
    /// <param name="code">The code.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public Task<Tenant?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

/// <summary>
    /// Get Active Tenants Async.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public Task<IReadOnlyList<Tenant>> GetActiveTenantsAsync(CancellationToken cancellationToken = default);
}
