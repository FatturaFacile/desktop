using FatturaFacile.Domain.Shared.Repositories;
using FatturaFacile.Identity.Domain.Aggregates;
using FatturaFacile.Identity.Domain.Entities;

namespace FatturaFacile.Identity.Domain.Repositories;

/// <summary>
/// Repository contract for Tenants.
/// </summary>
public interface ITenantRepository : IRepository
{
    Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Tenant?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default);
    Task UpdateAsync(Tenant tenant, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository contract for Users.
/// </summary>
public interface IUserRepository : IRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByNormalizedEmailAsync(Guid tenantId, string normalizedEmail, CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository contract for RefreshTokens.
/// </summary>
public interface IRefreshTokenRepository : IRepository
{
    Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task AddAsync(RefreshToken token, CancellationToken cancellationToken = default);
    Task UpdateAsync(RefreshToken token, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository contract for SecurityAuditLogs.
/// </summary>
public interface ISecurityAuditLogRepository : IRepository
{
    Task AddAsync(SecurityAuditLog log, CancellationToken cancellationToken = default);
}
