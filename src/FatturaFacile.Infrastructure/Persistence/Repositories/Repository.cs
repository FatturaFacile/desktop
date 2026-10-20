using System.Linq.Expressions;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.Persistence.Repositories;

/// <summary>
/// Generic repository implementation providing CRUD access using Entity Framework Core.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
/// <typeparam name="TId">The identifier type.</typeparam>
/// <param name="dbContext">The database context instance.</param>
public class Repository<TEntity, TId>(FatturaFacileDbContext dbContext) : IRepository<TEntity, TId>
    where TEntity : Entity<TId>
    where TId : notnull
{
    /// <summary>
    /// Gets the database context instance.
    /// </summary>
    protected FatturaFacileDbContext DbContext { get; } = dbContext;

    /// <summary>
    /// Gets the entity database set.
    /// </summary>
    protected DbSet<TEntity> DbSet { get; } = dbContext.Set<TEntity>();

    /// <inheritdoc/>
    public virtual async Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default) => await DbSet.FindAsync([id], cancellationToken);

    /// <inheritdoc/>
    public virtual async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking().ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async Task<IReadOnlyList<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking().Where(predicate).ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _ = await DbSet.AddAsync(entity, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _ = DbSet.Update(entity);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public virtual Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _ = DbSet.Remove(entity);
        return Task.CompletedTask;
    }
}
