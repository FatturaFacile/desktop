using FatturaFacile.Domain.Shared.Repositories;
using FatturaFacile.Crm.Domain.Aggregates;

namespace FatturaFacile.Crm.Domain.Repositories;

public interface IContactRepository : IRepository
{
    Task<Contact?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Contact contact, CancellationToken cancellationToken = default);
    Task UpdateAsync(Contact contact, CancellationToken cancellationToken = default);
}
