using FatturaFacile.Domain.Entities.Administration;
using FatturaFacile.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.Persistence.Repositories;

public sealed class TranslationRepository(FatturaFacileDbContext dbContext) : Repository<AppTranslation, Guid>(dbContext), ITranslationRepository
{
    public async Task<IReadOnlyList<AppTranslation>> GetByLocaleAsync(string locale, Guid? tenantId = null, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(t => t.Locale == locale && t.TenantId == tenantId)
            .ToListAsync(cancellationToken);
    }

    public Task<AppTranslation?> GetByKeyAsync(string locale, string key, Guid? tenantId = null, CancellationToken cancellationToken = default)
    {
        return DbSet
            .FirstOrDefaultAsync(t => t.Locale == locale && t.Key == key && t.TenantId == tenantId, cancellationToken);
    }
}