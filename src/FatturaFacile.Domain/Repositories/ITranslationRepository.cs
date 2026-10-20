using FatturaFacile.Domain.Entities.Administration;

namespace FatturaFacile.Domain.Repositories;

public interface ITranslationRepository : IRepository<AppTranslation, Guid>
{
    public Task<IReadOnlyList<AppTranslation>> GetByLocaleAsync(string locale, Guid? tenantId = null, CancellationToken cancellationToken = default);
    public Task<AppTranslation?> GetByKeyAsync(string locale, string key, Guid? tenantId = null, CancellationToken cancellationToken = default);
}