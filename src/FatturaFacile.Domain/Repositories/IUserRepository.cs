using FatturaFacile.Domain.Entities.Administration;

namespace FatturaFacile.Domain.Repositories;

public interface IUserRepository : IRepository<User, Guid>
{
    public Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
}