using FatturaFacile.Domain.Entities.Administration;
using FatturaFacile.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(FatturaFacileDbContext dbContext) : Repository<User, Guid>(dbContext), IUserRepository
{
    public Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default) => DbSet.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Username.Equals(username, StringComparison.InvariantCultureIgnoreCase), cancellationToken);

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) => DbSet.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Email.Equals(email, StringComparison.InvariantCultureIgnoreCase), cancellationToken);
}