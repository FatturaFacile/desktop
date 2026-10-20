using System.Linq.Expressions;
using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Repositories;

public interface IRepository<TEntity, TId>
    where TEntity : Entity<TId>
    where TId : notnull
{
    public Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);
    public Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    public Task<IReadOnlyList<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    public Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    public Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    public Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);
}

public interface IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
