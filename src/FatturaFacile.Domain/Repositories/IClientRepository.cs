using FatturaFacile.Domain.Entities.Crm;

namespace FatturaFacile.Domain.Repositories;

public interface IClientRepository : IRepository<Client, Guid>
{
    public Task<Client?> GetByVatOrFiscalCodeAsync(string code, CancellationToken cancellationToken = default);
    public Task<IReadOnlyList<Client>> GetActiveClientsAsync(CancellationToken cancellationToken = default);
}