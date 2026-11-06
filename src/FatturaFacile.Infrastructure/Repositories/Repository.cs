// <copyright file="Repository.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Linq.Expressions;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Domain.Common;
using FatturaFacile.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.Repositories;

/// <summary>
/// Generic repository implementation backed by EF Core.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public class Repository<T> : IRepository<T>
    where T : class, IBaseEntity
{
    private readonly DbSet<T> _dbSet;

    /// <summary>
    /// Initializes a new instance of the <see cref="Repository{T}"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public Repository(FatturaFacileDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _dbSet = context.Set<T>();
    }

    /// <inheritdoc/>
    public IQueryable<T> Query() => _dbSet;

    /// <inheritdoc/>
    public Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => _dbSet.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) => await _dbSet.Where(predicate).ToListAsync(cancellationToken).ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task AddAsync(T entity, CancellationToken cancellationToken = default) => await _dbSet.AddAsync(entity, cancellationToken).ConfigureAwait(false);

    /// <inheritdoc/>
    public void Update(T entity) => _dbSet.Update(entity);

    /// <inheritdoc/>
    public void Delete(T entity) => _dbSet.Remove(entity);
}
